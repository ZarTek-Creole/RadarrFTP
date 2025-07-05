using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsClient : FtpsClientBase<FtpsSettings>
    {
        private readonly IFtpsProxy _proxy;
        private static readonly Regex MovieReleaseRegex = new Regex(
            @"^(?<title>.+?)\.(?<year>\d{4})\..*?(?<quality>1080p|720p|480p|2160p|4K).*?-(?<group>\w+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FtpsClient(IFtpsProxy proxy,
            IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            Logger logger,
            ILocalizationService localizationService)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
            _proxy = proxy;
        }

        public override string Name => "FTPS Client";

        public override async Task<string> Download(RemoteMovie remoteMovie, IIndexer indexer)
        {
            var title = remoteMovie.Release.Title;
            var downloadId = Guid.NewGuid().ToString();
            
            try
            {
                // Recherche du fichier sur le serveur FTPS
                var movieFiles = await FindMovieFilesAsync(title);
                
                if (!movieFiles.Any())
                {
                    throw new ReleaseDownloadException(remoteMovie.Release, "Movie not found on FTPS server");
                }

                // Sélection du meilleur fichier
                var selectedFile = SelectBestFile(movieFiles, remoteMovie);
                
                // Téléchargement
                var localPath = GetDownloadPath(selectedFile.Name);
                var success = await _proxy.DownloadFileAsync(Settings, selectedFile.FullPath, localPath);
                
                if (!success)
                {
                    throw new ReleaseDownloadException(remoteMovie.Release, "Failed to download movie file");
                }

                _logger.Info("Successfully downloaded {0} to {1}", selectedFile.Name, localPath);
                return downloadId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download movie: {0}", title);
                throw;
            }
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            // Implémentation basique pour récupérer les éléments en cours de téléchargement
            // Pour FTPS, cela pourrait inclure un système de cache ou de base de données locale
            return new List<DownloadClientItem>();
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            // Implémentation pour supprimer un élément
            if (deleteData)
            {
                DeleteItemData(item);
            }
        }

        public override DownloadClientInfo GetStatus()
        {
            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { new OsPath(GetDownloadPath("")) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            failures.AddIfNotNull(TestBasePath());
        }

        protected override async Task<bool> TestConnectionAsync()
        {
            return await _proxy.TestConnectionAsync(Settings);
        }

        protected override async Task<IEnumerable<string>> GetDirectoryListingAsync(string path)
        {
            var items = await _proxy.GetDirectoryListingAsync(Settings, path);
            return items.Select(i => i.FullPath);
        }

        protected override async Task<string> DownloadFileAsync(string remotePath, string localPath)
        {
            var success = await _proxy.DownloadFileAsync(Settings, remotePath, localPath);
            return success ? localPath : null;
        }

        private async Task<List<FtpsDirectoryItem>> FindMovieFilesAsync(string title)
        {
            var moviePath = Path.Combine(Settings.BasePath, Settings.MovieDirectory);
            var allFiles = await _proxy.GetDirectoryListingAsync(Settings, moviePath);
            
            return allFiles
                .Where(f => !f.IsDirectory && IsMovieFile(f.Name, title))
                .ToList();
        }

        private bool IsMovieFile(string fileName, string searchTitle)
        {
            var match = MovieReleaseRegex.Match(fileName);
            if (!match.Success) return false;
            
            var fileTitle = match.Groups["title"].Value.Replace(".", " ");
            return fileTitle.ContainsIgnoreCase(searchTitle);
        }

        private FtpsDirectoryItem SelectBestFile(List<FtpsDirectoryItem> files, RemoteMovie remoteMovie)
        {
            // Logique de sélection du meilleur fichier basée sur la qualité
            // Pour l'instant, on prend le plus gros fichier
            return files.OrderByDescending(f => f.Size).First();
        }

        private string GetDownloadPath(string fileName)
        {
            var downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RadarrDownloads");
            Directory.CreateDirectory(downloadDir);
            return Path.Combine(downloadDir, fileName);
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var result = _proxy.TestConnectionAsync(Settings).Result;
                return result ? null : new ValidationFailure("Host", "Unable to connect to FTPS server");
            }
            catch (Exception ex)
            {
                return new ValidationFailure("Host", $"Connection test failed: {ex.Message}");
            }
        }

        private ValidationFailure TestBasePath()
        {
            try
            {
                var items = _proxy.GetDirectoryListingAsync(Settings, Settings.BasePath).Result;
                return null;
            }
            catch (Exception ex)
            {
                return new ValidationFailure("BasePath", $"Invalid base path: {ex.Message}");
            }
        }
    }
}