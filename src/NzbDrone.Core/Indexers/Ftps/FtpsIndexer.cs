using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFTP;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Ftps
{
    public class FtpsIndexer : IndexerBase<FtpsIndexerSettings>
    {
        public FtpsIndexer(IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(indexerStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "FTPS Direct Indexer";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        public override bool SupportsRss => false;
        public override bool SupportsSearch => true;

        public override Task<IList<ReleaseInfo>> FetchRecent()
        {
            return Task.FromResult<IList<ReleaseInfo>>(new List<ReleaseInfo>());
        }

        public override async Task<IList<ReleaseInfo>> Fetch(MovieSearchCriteria searchCriteria)
        {
            var releases = new List<ReleaseInfo>();

            try
            {
                await Task.Run(() =>
                {
                    using (var ftpClient = CreateFtpClient())
                    {
                        ftpClient.Connect();

                        var searchPaths = Settings.MoviePaths?.Split(',') ?? new[] { "/" };

                        foreach (var searchPath in searchPaths)
                        {
                            var items = ftpClient.GetListing(searchPath.Trim());

                            foreach (var item in items)
                            {
                                if (IsMovieMatch(item.Name, searchCriteria.Movie.Title, searchCriteria.Movie.Year.ToString()))
                                {
                                    var release = CreateReleaseInfo(item, searchCriteria.Movie.Title, searchCriteria.Movie.Year.ToString());
                                    if (release != null)
                                    {
                                        releases.Add(release);
                                    }
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching FTPS releases");
            }

            return releases;
        }

        public override HttpRequest GetDownloadRequest(string link)
        {
            return new HttpRequest(link);
        }

        protected override async Task Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(await TestConnectionAsync());
        }

        private async Task<ValidationFailure> TestConnectionAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var ftpClient = CreateFtpClient())
                    {
                        ftpClient.Connect();
                        _logger.Info("FTPS indexer connection test successful");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "FTPS indexer connection test failed");
                return new ValidationFailure("Host", $"Unable to connect to FTPS server: {ex.Message}");
            }

            return null;
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

        private bool IsMovieMatch(string filename, string movieTitle, string year)
        {
            if (string.IsNullOrEmpty(movieTitle))
            {
                return false;
            }

            var normalizedFilename = filename.ToLowerInvariant();
            var normalizedTitle = movieTitle.ToLowerInvariant().Replace(" ", ".");

            if (!normalizedFilename.Contains(normalizedTitle))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(year) && !normalizedFilename.Contains(year))
            {
                return false;
            }

            return true;
        }

        private ReleaseInfo CreateReleaseInfo(FtpListItem item, string title, string year)
        {
            try
            {
                var release = new ReleaseInfo
                {
                    Title = item.Name,
                    DownloadUrl = $"ftps://{Settings.Host}:{Settings.Port}{item.FullName}",
                    InfoUrl = $"ftps://{Settings.Host}:{Settings.Port}{item.FullName}",
                    Guid = item.FullName,
                    PublishDate = item.Modified,
                    Size = item.Size,
                    Indexer = "FTPS Direct"
                };

                // Parsing de la qualité depuis le nom du fichier
                var quality = ParseQuality(item.Name);
                release.Title = $"{title} {year} {quality}";

                return release;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"Failed to create release info for: {item.Name}");
                return null;
            }
        }

        private string ParseQuality(string filename)
        {
            var qualities = new Dictionary<string, string>
            {
                { "2160p", "4K" },
                { "1080p", "1080p" },
                { "720p", "720p" },
                { "480p", "480p" },
                { "bluray", "BluRay" },
                { "bdrip", "BDRip" },
                { "webrip", "WEBRip" },
                { "webdl", "WEB-DL" },
                { "hdtv", "HDTV" },
                { "dvdrip", "DVDRip" }
            };

            var normalizedFilename = filename.ToLowerInvariant();

            foreach (var quality in qualities)
            {
                if (normalizedFilename.Contains(quality.Key))
                {
                    return quality.Value;
                }
            }

            return "Unknown";
        }
    }
}



