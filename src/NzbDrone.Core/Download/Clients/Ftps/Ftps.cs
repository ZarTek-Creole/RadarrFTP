using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Movies;
using FluentFTP;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class Ftps : DownloadClientBase<FtpsSettings>
    {
        private readonly IFtpsProxy _proxy;
        private readonly IParsingService _parsingService;
        private readonly IMovieService _movieService;
        private readonly ConcurrentDictionary<string, FtpsItem> _downloadItems;
        private readonly Timer _monitoringTimer;

        // Scene release naming patterns
        private static readonly Regex SceneReleaseRegex = new Regex(
            @"^(?<title>.+?)\.(?<year>\d{4})\..*?\.(?<quality>480p|720p|1080p|2160p|UHD)\.(?<source>BluRay|WEB-DL|WEBRip|HDTV|CAM|TS|TC|R5|DVDRip|BDRip).*?-(?<group>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex QualityRegex = new Regex(
            @"(?<quality>480p|720p|1080p|2160p|UHD|4K)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SourceRegex = new Regex(
            @"(?<source>BluRay|BDRip|WEB-DL|WEBRip|HDTV|CAM|TS|TC|R5|DVDRip)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CodecRegex = new Regex(
            @"(?<codec>x264|x265|HEVC|H\.264|H\.265|AVC|MPEG-2)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string Name => "FTPS";
        public override DownloadProtocol Protocol => DownloadProtocol.Ftps;

        public Ftps(IFtpsProxy proxy,
                   IParsingService parsingService,
                   IMovieService movieService,
                   IHttpClient httpClient,
                   IConfigService configService,
                   IDiskProvider diskProvider,
                   IRemotePathMappingService remotePathMappingService,
                   Logger logger,
                   ILocalizationService localizationService)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
            _proxy = proxy;
            _parsingService = parsingService;
            _movieService = movieService;
            _downloadItems = new ConcurrentDictionary<string, FtpsItem>();

            // Setup monitoring timer if enabled
            if (Settings?.MonitoringEnabled == true)
            {
                _monitoringTimer = new Timer(MonitorServersAsync, null, 
                    TimeSpan.FromSeconds(Settings.MonitoringInterval), 
                    TimeSpan.FromSeconds(Settings.MonitoringInterval));
            }
        }

        public override async Task<string> Download(RemoteMovie remoteMovie, IIndexer indexer)
        {
            try
            {
                _logger.Info("Starting FTPS download search for movie: {0} ({1})", 
                    remoteMovie.Movie.Title, remoteMovie.Movie.Year);

                var releases = await ScanForMovieReleases(remoteMovie);
                
                if (!releases.Any())
                {
                    throw new DownloadClientRejectedReleaseException(remoteMovie.Release, 
                        "No suitable releases found on FTPS server");
                }

                var bestRelease = SelectBestRelease(releases, remoteMovie);
                
                if (bestRelease == null)
                {
                    throw new DownloadClientRejectedReleaseException(remoteMovie.Release, 
                        "No release matches quality requirements");
                }

                var downloadId = Guid.NewGuid().ToString();
                var downloadItem = CreateDownloadItem(bestRelease, downloadId, remoteMovie);
                
                _downloadItems.TryAdd(downloadId, downloadItem);

                // Start download in background
                _ = Task.Run(async () => await PerformDownloadAsync(downloadItem));

                _logger.Info("FTPS download initiated for {0} - ID: {1}", bestRelease.Name, downloadId);
                return downloadId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initiate FTPS download for {0}", remoteMovie.Movie.Title);
                throw;
            }
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            return _downloadItems.Values.Select(ConvertToDownloadClientItem).ToList();
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            if (_downloadItems.TryRemove(item.DownloadId, out var ftpsItem))
            {
                if (deleteData && ftpsItem.OutputPath.IsNotNullOrWhiteSpace())
                {
                    DeleteItemData(item);
                }

                _logger.Info("Removed FTPS download item: {0}", item.Title);
            }
        }

        public override DownloadClientInfo GetStatus()
        {
            var status = new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { new OsPath(_configService.DownloadedMoviesFolder) }
            };

            return status;
        }

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            if (_downloadItems.TryGetValue(downloadClientItem.DownloadId, out var ftpsItem))
            {
                ftpsItem.MarkAsCompleted();
                _logger.Debug("Marked FTPS item as imported: {0}", downloadClientItem.Title);
            }
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            failures.AddIfNotNull(TestBasePath());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var testTask = _proxy.TestConnectionAsync(Settings);
                var result = testTask.GetAwaiter().GetResult();
                
                if (!result)
                {
                    return new ValidationFailure("Host", "Unable to connect to FTPS server");
                }

                return null;
            }
            catch (FtpsAuthenticationException)
            {
                return new ValidationFailure("Username", "Authentication failed - check username and password");
            }
            catch (FtpsCertificateException)
            {
                return new ValidationFailure("ValidateCertificate", "SSL certificate validation failed");
            }
            catch (FtpsConnectionException ex)
            {
                return new ValidationFailure("Host", $"Connection failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new ValidationFailure("", $"Unknown error: {ex.Message}");
            }
        }

        private ValidationFailure TestBasePath()
        {
            if (string.IsNullOrWhiteSpace(Settings.RemoteBasePath))
            {
                return new ValidationFailure("RemoteBasePath", "Remote base path is required");
            }

            try
            {
                var existsTask = _proxy.DirectoryExistsAsync(Settings.RemoteBasePath, Settings);
                var exists = existsTask.GetAwaiter().GetResult();
                
                if (!exists)
                {
                    return new ValidationFailure("RemoteBasePath", 
                        $"Remote base path does not exist: {Settings.RemoteBasePath}");
                }

                return null;
            }
            catch (Exception ex)
            {
                return new ValidationFailure("RemoteBasePath", 
                    $"Unable to verify base path: {ex.Message}");
            }
        }

        private async Task<IEnumerable<FtpsReleaseItem>> ScanForMovieReleases(RemoteMovie remoteMovie)
        {
            var releases = new List<FtpsReleaseItem>();
            var searchPaths = GenerateSearchPaths(remoteMovie);

            foreach (var path in searchPaths)
            {
                try
                {
                    _logger.Debug("Scanning FTPS path: {0}", path);
                    var items = await _proxy.GetDirectoryListingAsync(path, Settings);
                    
                    var movieReleases = items.Where(item => IsMovieRelease(item, remoteMovie))
                                             .ToList();
                    
                    foreach (var release in movieReleases)
                    {
                        ParseReleaseInfo(release);
                    }
                    
                    releases.AddRange(movieReleases);
                    _logger.Debug("Found {0} potential releases in {1}", movieReleases.Count, path);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to scan path: {0}", path);
                }
            }

            _logger.Info("Total releases found: {0}", releases.Count);
            return releases;
        }

        private List<string> GenerateSearchPaths(RemoteMovie remoteMovie)
        {
            var paths = new List<string>();
            var basePath = Settings.RemoteBasePath.TrimEnd('/');
            
            // Primary search paths
            paths.Add(basePath);
            
            // Year-based paths
            if (remoteMovie.Movie.Year.HasValue)
            {
                paths.Add($"{basePath}/{remoteMovie.Movie.Year}");
                paths.Add($"{basePath}/Movies/{remoteMovie.Movie.Year}");
            }
            
            // Title-based paths (first letter organization)
            var firstLetter = remoteMovie.Movie.Title.ToUpperInvariant().FirstOrDefault();
            if (char.IsLetter(firstLetter))
            {
                paths.Add($"{basePath}/{firstLetter}");
                paths.Add($"{basePath}/Movies/{firstLetter}");
            }
            
            // Category-based paths
            if (!string.IsNullOrWhiteSpace(Settings.Category))
            {
                paths.Add($"{basePath}/{Settings.Category}");
                if (remoteMovie.Movie.Year.HasValue)
                {
                    paths.Add($"{basePath}/{Settings.Category}/{remoteMovie.Movie.Year}");
                }
            }

            return paths.Distinct().ToList();
        }

        private bool IsMovieRelease(FtpsReleaseItem item, RemoteMovie remoteMovie)
        {
            if (item.IsDirectory)
            {
                // Check if directory name matches movie patterns
                return SceneReleaseRegex.IsMatch(item.Name) && 
                       ContainsMovieTitle(item.Name, remoteMovie.Movie);
            }

            if (item.IsVideoFile)
            {
                return ContainsMovieTitle(item.Name, remoteMovie.Movie);
            }

            return false;
        }

        private bool ContainsMovieTitle(string fileName, Movie movie)
        {
            var cleanFileName = fileName.ToLowerInvariant().Replace('.', ' ').Replace('_', ' ');
            var cleanMovieTitle = movie.Title.ToLowerInvariant();
            
            // Check if title is contained
            var titleMatch = cleanFileName.Contains(cleanMovieTitle);
            
            // Check year if available
            var yearMatch = !movie.Year.HasValue || cleanFileName.Contains(movie.Year.ToString());
            
            return titleMatch && yearMatch;
        }

        private void ParseReleaseInfo(FtpsReleaseItem release)
        {
            var fileName = release.Name;
            
            // Parse using scene regex
            var sceneMatch = SceneReleaseRegex.Match(fileName);
            if (sceneMatch.Success)
            {
                release.Title = sceneMatch.Groups["title"].Value.Replace('.', ' ');
                if (int.TryParse(sceneMatch.Groups["year"].Value, out var year))
                {
                    release.Year = year;
                }
                release.Quality = sceneMatch.Groups["quality"].Value;
                release.Source = sceneMatch.Groups["source"].Value;
                release.ReleaseGroup = sceneMatch.Groups["group"].Value;
            }
            else
            {
                // Fallback parsing
                var qualityMatch = QualityRegex.Match(fileName);
                if (qualityMatch.Success)
                {
                    release.Quality = qualityMatch.Groups["quality"].Value;
                }
                
                var sourceMatch = SourceRegex.Match(fileName);
                if (sourceMatch.Success)
                {
                    release.Source = sourceMatch.Groups["source"].Value;
                }
            }
            
            // Parse codec
            var codecMatch = CodecRegex.Match(fileName);
            if (codecMatch.Success)
            {
                release.Codec = codecMatch.Groups["codec"].Value;
            }
        }

        private FtpsReleaseItem SelectBestRelease(IEnumerable<FtpsReleaseItem> releases, RemoteMovie remoteMovie)
        {
            return releases.OrderByDescending(r => CalculateReleaseScore(r, remoteMovie))
                          .FirstOrDefault();
        }

        private int CalculateReleaseScore(FtpsReleaseItem release, RemoteMovie remoteMovie)
        {
            var score = 0;
            
            // Size preference (not too small, not too large)
            if (release.Size > 500 * 1024 * 1024) // > 500MB
                score += 20;
            if (release.Size > 2L * 1024 * 1024 * 1024) // > 2GB
                score += 30;
            
            // Quality preference
            switch (release.Quality?.ToLowerInvariant())
            {
                case "1080p":
                    score += 50;
                    break;
                case "720p":
                    score += 40;
                    break;
                case "2160p":
                case "uhd":
                    score += 60;
                    break;
                case "480p":
                    score += 20;
                    break;
            }
            
            // Source preference
            switch (release.Source?.ToLowerInvariant())
            {
                case "bluray":
                case "bdrip":
                    score += 30;
                    break;
                case "web-dl":
                case "webrip":
                    score += 25;
                    break;
                case "hdtv":
                    score += 15;
                    break;
                case "cam":
                case "ts":
                case "tc":
                    score -= 20;
                    break;
            }
            
            // Codec preference
            if (release.Codec?.ToLowerInvariant().Contains("x265") == true ||
                release.Codec?.ToLowerInvariant().Contains("hevc") == true)
            {
                score += 10;
            }
            
            // Prefer newer releases
            var daysSinceModified = (DateTime.UtcNow - release.ModifiedTime).TotalDays;
            if (daysSinceModified < 7)
                score += 10;
            
            return score;
        }

        private FtpsItem CreateDownloadItem(FtpsReleaseItem release, string downloadId, RemoteMovie remoteMovie)
        {
            var outputPath = new OsPath(Path.Combine(
                _configService.DownloadedMoviesFolder,
                release.Name));

            return new FtpsItem
            {
                DownloadId = downloadId,
                Title = release.Name,
                Category = Settings.Category,
                TotalSize = release.Size,
                RemainingSize = release.Size,
                RemotePath = release.FullPath,
                OutputPath = outputPath,
                Status = FtpsDownloadStatus.Queued,
                ServerId = release.ServerId,
                ReleaseGroup = release.ReleaseGroup,
                Quality = release.Quality,
                Source = release.Source,
                Codec = release.Codec,
                Year = release.Year
            };
        }

        private async Task PerformDownloadAsync(FtpsItem downloadItem)
        {
            try
            {
                downloadItem.MarkAsStarted();
                _logger.Info("Starting download: {0}", downloadItem.Title);

                var progress = new Progress<FtpProgress>(p =>
                {
                    if (p.TransferredBytes > 0)
                    {
                        downloadItem.UpdateProgress(p.TransferredBytes);
                        
                        if (p.ETA.HasValue)
                        {
                            downloadItem.RemainingTime = p.ETA;
                        }
                    }
                });

                var success = await _proxy.DownloadFileAsync(
                    downloadItem.RemotePath,
                    downloadItem.OutputPath.FullPath,
                    Settings,
                    progress);

                if (success)
                {
                    downloadItem.MarkAsCompleted();
                    _logger.Info("Download completed successfully: {0}", downloadItem.Title);
                    
                    // Verify download if hash is available
                    await VerifyDownload(downloadItem);
                }
                else
                {
                    downloadItem.MarkAsFailed("Download failed");
                    _logger.Error("Download failed: {0}", downloadItem.Title);
                }
            }
            catch (Exception ex)
            {
                downloadItem.MarkAsFailed($"Download error: {ex.Message}");
                _logger.Error(ex, "Download error for {0}", downloadItem.Title);
            }
        }

        private async Task VerifyDownload(FtpsItem downloadItem)
        {
            try
            {
                var hash = await _proxy.GetHashAsync(downloadItem.RemotePath, Settings);
                if (!string.IsNullOrWhiteSpace(hash))
                {
                    // TODO: Implement local file hash verification
                    downloadItem.Hash = hash;
                    downloadItem.HashType = "MD5";
                    _logger.Debug("Downloaded file hash: {0}", hash);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Could not verify download hash for {0}", downloadItem.Title);
            }
        }

        private DownloadClientItem ConvertToDownloadClientItem(FtpsItem ftpsItem)
        {
            var status = ConvertStatus(ftpsItem.Status);
            
            return new DownloadClientItem
            {
                DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                DownloadId = ftpsItem.DownloadId,
                Category = ftpsItem.Category,
                Title = ftpsItem.Title,
                TotalSize = ftpsItem.TotalSize,
                RemainingSize = ftpsItem.RemainingSize,
                RemainingTime = ftpsItem.RemainingTime,
                OutputPath = ftpsItem.OutputPath,
                Status = status,
                Message = ftpsItem.Message,
                CanBeRemoved = ftpsItem.CanBeRemoved,
                CanMoveFiles = ftpsItem.CanMoveFiles
            };
        }

        private DownloadItemStatus ConvertStatus(FtpsDownloadStatus ftpsStatus)
        {
            return ftpsStatus switch
            {
                FtpsDownloadStatus.Queued => DownloadItemStatus.Queued,
                FtpsDownloadStatus.Downloading => DownloadItemStatus.Downloading,
                FtpsDownloadStatus.Paused => DownloadItemStatus.Paused,
                FtpsDownloadStatus.Completed => DownloadItemStatus.Completed,
                FtpsDownloadStatus.Failed => DownloadItemStatus.Failed,
                FtpsDownloadStatus.Warning => DownloadItemStatus.Warning,
                FtpsDownloadStatus.Cancelled => DownloadItemStatus.Failed,
                FtpsDownloadStatus.Verifying => DownloadItemStatus.Downloading,
                FtpsDownloadStatus.Moving => DownloadItemStatus.Downloading,
                _ => DownloadItemStatus.Unknown
            };
        }

        private async void MonitorServersAsync(object state)
        {
            if (!Settings.MonitoringEnabled)
                return;

            try
            {
                _logger.Debug("Starting FTPS server monitoring");
                
                // This would scan for new releases and automatically add them
                // Implementation depends on specific requirements
                var newReleases = await ScanForNewReleases();
                
                foreach (var release in newReleases)
                {
                    _logger.Info("New release detected: {0}", release.Name);
                    // Could automatically trigger download based on wanted movies
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during FTPS monitoring");
            }
        }

        private async Task<IEnumerable<FtpsReleaseItem>> ScanForNewReleases()
        {
            // Placeholder for monitoring implementation
            // Would scan servers for new content based on wanted movies
            await Task.CompletedTask;
            return Enumerable.Empty<FtpsReleaseItem>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monitoringTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}