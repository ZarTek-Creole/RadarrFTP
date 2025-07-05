using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Ftps
{
    public class FtpsIndexer : IndexerBase<FtpsIndexerSettings>
    {
        private readonly IFtpsProxy _ftpsProxy;

        public override string Name => "FTPS Indexer";
        public override DownloadProtocol Protocol => DownloadProtocol.Ftps;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;

        public FtpsIndexer(IFtpsProxy ftpsProxy, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(indexerStatusService, configService, parsingService, logger)
        {
            _ftpsProxy = ftpsProxy;
        }

        public override async Task<IList<ReleaseInfo>> FetchRecent()
        {
            _logger.Debug("Fetching recent releases from FTPS server: {0}", Settings.Host);
            
            try
            {
                var releases = new List<ReleaseInfo>();
                var directories = await _ftpsProxy.GetDirectoryListingAsync(Settings, Settings.MovieDirectory);

                foreach (var directory in directories.Where(d => d.IsDirectory))
                {
                    var movieFiles = await _ftpsProxy.GetDirectoryListingAsync(Settings, $"{Settings.MovieDirectory}/{directory.Name}");
                    
                    foreach (var file in movieFiles.Where(f => !f.IsDirectory && IsVideoFile(f.Name)))
                    {
                        var release = CreateReleaseInfo(directory.Name, file);
                        if (release != null)
                        {
                            releases.Add(release);
                        }
                    }
                }

                _logger.Debug("Found {0} releases from FTPS server", releases.Count);
                return CleanupReleases(releases);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching recent releases from FTPS server: {0}", Settings.Host);
                throw;
            }
        }

        public override async Task<IList<ReleaseInfo>> Fetch(MovieSearchCriteria searchCriteria)
        {
            _logger.Debug("Searching for movie: {0} on FTPS server: {1}", searchCriteria.Movie?.Title, Settings.Host);
            
            try
            {
                var releases = new List<ReleaseInfo>();
                var directories = await _ftpsProxy.GetDirectoryListingAsync(Settings, Settings.MovieDirectory);

                var searchTerm = searchCriteria.Movie?.Title;
                var searchYear = searchCriteria.Movie?.Year;

                foreach (var directory in directories.Where(d => d.IsDirectory))
                {
                    // Check if directory name matches search criteria
                    if (!string.IsNullOrWhiteSpace(searchTerm) && 
                        !directory.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                    {
                        continue;
                    }

                    if (searchYear > 0 && 
                        !directory.Name.Contains(searchYear.ToString()))
                    {
                        continue;
                    }

                    var movieFiles = await _ftpsProxy.GetDirectoryListingAsync(Settings, $"{Settings.MovieDirectory}/{directory.Name}");
                    
                    foreach (var file in movieFiles.Where(f => !f.IsDirectory && IsVideoFile(f.Name)))
                    {
                        var release = CreateReleaseInfo(directory.Name, file);
                        if (release != null)
                        {
                            releases.Add(release);
                        }
                    }
                }

                _logger.Debug("Found {0} matching releases for search: {1}", releases.Count, searchTerm);
                return CleanupReleases(releases);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching for movie on FTPS server: {0}", Settings.Host);
                throw;
            }
        }

        public override HttpRequest GetDownloadRequest(string link)
        {
            // This is not used for FTPS, but required by interface
            // The actual download is handled by the FtpsClient download client
            return null;
        }

        private ReleaseInfo CreateReleaseInfo(string directoryName, FtpsDirectoryItem file)
        {
            try
            {
                var fullPath = $"{Settings.MovieDirectory}/{directoryName}/{file.Name}";
                var title = $"{directoryName} - {file.Name}";
                
                // Try to parse movie information from directory name
                var movieInfo = Parser.Parser.ParseMovieTitle(directoryName);
                if (movieInfo != null)
                {
                    title = movieInfo.MovieTitle;
                    if (movieInfo.Year > 0)
                    {
                        title += $" ({movieInfo.Year})";
                    }
                    if (!string.IsNullOrWhiteSpace(movieInfo.Quality?.Quality?.Name))
                    {
                        title += $" {movieInfo.Quality.Quality.Name}";
                    }
                }

                return new ReleaseInfo
                {
                    Guid = $"ftps://{Settings.Host}:{Settings.Port}/{fullPath}",
                    Title = title,
                    DownloadUrl = $"ftps://{Settings.Host}:{Settings.Port}/{fullPath}",
                    InfoUrl = $"ftps://{Settings.Host}:{Settings.Port}/{Settings.MovieDirectory}/{directoryName}",
                    PublishDate = file.LastModified,
                    Size = file.Size,
                    IndexerFlags = 0
                };
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error creating release info for file: {0}", file.Name);
                return null;
            }
        }

        private bool IsVideoFile(string fileName)
        {
            var videoExtensions = new[] { ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg", ".ts", ".m2ts" };
            return videoExtensions.Any(ext => fileName.ToLowerInvariant().EndsWith(ext));
        }

        protected override async Task Test(List<ValidationFailure> failures)
        {
            try
            {
                var testResult = await _ftpsProxy.TestConnectionAsync(Settings);
                if (!testResult)
                {
                    failures.Add(new ValidationFailure("Host", "Unable to connect to FTPS server"));
                    return;
                }

                // Test directory access
                var directories = await _ftpsProxy.GetDirectoryListingAsync(Settings, Settings.MovieDirectory);
                if (directories == null)
                {
                    failures.Add(new ValidationFailure("MovieDirectory", "Unable to access movie directory"));
                    return;
                }

                _logger.Debug("FTPS Indexer test successful for {0}", Settings.Host);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "FTPS Indexer test failed for {0}", Settings.Host);
                failures.Add(new ValidationFailure("", $"Test failed: {ex.Message}"));
            }
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return new IndexerDefinition
                {
                    EnableRss = false,
                    EnableAutomaticSearch = false,
                    EnableInteractiveSearch = false,
                    Name = "FTPS Server",
                    Implementation = GetType().Name,
                    Settings = new FtpsIndexerSettings
                    {
                        Host = "your-ftps-server.com",
                        Port = 21,
                        Username = "username",
                        Password = "password",
                        MovieDirectory = "movies",
                        SecurityMode = FtpsSecurityMode.Explicit,
                        ConnectionMode = FtpsConnectionMode.Passive
                    },
                    Protocol = DownloadProtocol.Ftps,
                    SupportsRss = SupportsRss,
                    SupportsSearch = SupportsSearch
                };
            }
        }
    }
}