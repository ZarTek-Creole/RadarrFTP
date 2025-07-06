using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentFTP;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using SharpCompress.Archives;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class Ftps : DownloadClientBase<FtpsSettings>
    {
        private readonly IHttpClient _httpClient;

        public Ftps(IConfigService configService,
                   IDiskProvider diskProvider,
                   IRemotePathMappingService remotePathMappingService,
                   IHttpClient httpClient,
                   ILocalizationService localizationService,
                   Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
            _httpClient = httpClient;
        }

        public override string Name => "FTPS Direct Download";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        public override async Task<string> Download(RemoteMovie remoteMovie, IIndexer indexer)
        {
            var downloadId = Guid.NewGuid().ToString();

            try
            {
                await Task.Run(() =>
                {
                    using (var ftpClient = CreateFtpClient())
                    {
                        ftpClient.Connect();

                        // Recherche du fichier sur le serveur FTPS
                        var remotePath = FindMovieOnServer(ftpClient, remoteMovie);
                        if (string.IsNullOrEmpty(remotePath))
                        {
                            throw new DownloadClientException($"Movie not found on FTPS server: {remoteMovie.Movie.Title}");
                        }

                        // Téléchargement du fichier/répertoire
                        var localPath = Path.Combine(Settings.TvCategory, SanitizeTitle(remoteMovie.Movie.Title));
                        Directory.CreateDirectory(localPath);

                        if (ftpClient.DirectoryExists(remotePath))
                        {
                            DownloadDirectory(ftpClient, remotePath, localPath);
                        }
                        else
                        {
                            var localFile = Path.Combine(localPath, Path.GetFileName(remotePath));
                            ftpClient.DownloadFile(localFile, remotePath);
                        }

                        // Extraction des archives RAR si nécessaire
                        ExtractRarArchives(localPath);

                        _logger.Info($"FTPS download completed: {remoteMovie.Movie.Title}");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"FTPS download failed: {remoteMovie.Movie.Title}");
                throw new DownloadClientException($"FTPS download failed: {ex.Message}", ex);
            }

            return downloadId;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            // FTPS downloads are handled locally, nothing to remove from server
            _logger.Info($"FTPS download removal requested: {item.DownloadId}");
        }

        public override DownloadClientInfo GetStatus()
        {
            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { new OsPath(Settings.TvCategory) }
            };
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            // FTPS downloads are handled locally, return empty list
            return Enumerable.Empty<DownloadClientItem>();
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            failures.AddIfNotNull(TestCategory());
        }

        private FtpClient CreateFtpClient()
        {
            var client = new FtpClient(Settings.Host, Settings.Username, Settings.Password, Settings.Port);

            if (Settings.UseSsl)
            {
                client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
                client.Config.ValidateAnyCertificate = true;
            }

            return client;
        }

        private string FindMovieOnServer(FtpClient ftpClient, RemoteMovie remoteMovie)
        {
            var searchPaths = Settings.MoviePaths?.Split(',') ?? new[] { "/" };

            foreach (var searchPath in searchPaths)
            {
                var items = ftpClient.GetListing(searchPath.Trim());

                foreach (var item in items)
                {
                    if (IsMovieMatch(item.Name, remoteMovie.Movie.Title, remoteMovie.ParsedMovieInfo.Year))
                    {
                        return item.FullName;
                    }
                }
            }

            return null;
        }

        private bool IsMovieMatch(string filename, string movieTitle, int? year)
        {
            var normalizedFilename = filename.ToLowerInvariant();
            var normalizedTitle = movieTitle.ToLowerInvariant().Replace(" ", ".");

            if (!normalizedFilename.Contains(normalizedTitle))
            {
                return false;
            }

            if (year.HasValue && !normalizedFilename.Contains(year.Value.ToString()))
            {
                return false;
            }

            return true;
        }

        private void DownloadDirectory(FtpClient ftpClient, string remotePath, string localPath)
        {
            var items = ftpClient.GetListing(remotePath);

            foreach (var item in items)
            {
                var localItemPath = Path.Combine(localPath, item.Name);

                if (item.Type == FtpObjectType.Directory)
                {
                    Directory.CreateDirectory(localItemPath);
                    DownloadDirectory(ftpClient, item.FullName, localItemPath);
                }
                else
                {
                    ftpClient.DownloadFile(localItemPath, item.FullName);
                }
            }
        }

        private void ExtractRarArchives(string directory)
        {
            var rarFiles = Directory.GetFiles(directory, "*.rar", SearchOption.AllDirectories)
                                  .Where(f => !Path.GetFileName(f).Contains(".part") ||
                                            Path.GetFileName(f).Contains(".part01.rar") ||
                                            Path.GetFileName(f).Contains(".part001.rar"))
                                  .ToList();

            foreach (var rarFile in rarFiles)
            {
                try
                {
                    using (var archive = ArchiveFactory.Open(rarFile))
                    {
                        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                        {
                            var extractPath = Path.Combine(Path.GetDirectoryName(rarFile), entry.Key);
                            Directory.CreateDirectory(Path.GetDirectoryName(extractPath));

                            using (var entryStream = entry.OpenEntryStream())
                            using (var outputStream = File.Create(extractPath))
                            {
                                entryStream.CopyTo(outputStream);
                            }
                        }
                    }

                    _logger.Info($"Extracted RAR archive: {rarFile}");

                    // Suppression des fichiers RAR après extraction
                    if (Settings.RemoveCompletedDownloads)
                    {
                        File.Delete(rarFile);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, $"Failed to extract RAR archive: {rarFile}");
                }
            }
        }

        private string SanitizeTitle(string title)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", title.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                using (var ftpClient = CreateFtpClient())
                {
                    ftpClient.Connect();
                    _logger.Info("FTPS connection test successful");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "FTPS connection test failed");
                return new ValidationFailure("Host", $"Unable to connect to FTPS server: {ex.Message}");
            }

            return null;
        }

        private ValidationFailure TestCategory()
        {
            if (string.IsNullOrWhiteSpace(Settings.TvCategory))
            {
                return new ValidationFailure("TvCategory", "Download directory is required");
            }

            if (!_diskProvider.FolderExists(Settings.TvCategory))
            {
                return new ValidationFailure("TvCategory", "Download directory does not exist");
            }

            return null;
        }
    }
}
