using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    public class FtpsIndexer : HttpIndexerBase<FtpsIndexerSettings>
    {
        private readonly IFtpsProxy _ftpsProxy;

        public override string Name => "FTPS Indexer";
        public override DownloadProtocol Protocol => DownloadProtocol.Ftps;
        public override int PageSize => 100;

        public FtpsIndexer(IFtpsProxy ftpsProxy, IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _ftpsProxy = ftpsProxy;
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new FtpsRequestGenerator()
            {
                Settings = Settings,
                FtpsProxy = _ftpsProxy,
                Logger = _logger
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new FtpsResponseParser()
            {
                Settings = Settings,
                FtpsProxy = _ftpsProxy,
                Logger = _logger
            };
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return new IndexerDefinition
                {
                    EnableRss = true,
                    EnableAutomaticSearch = true,
                    EnableInteractiveSearch = true,
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

    public class FtpsRequestGenerator : IIndexerRequestGenerator
    {
        public FtpsIndexerSettings Settings { get; set; }
        public IFtpsProxy FtpsProxy { get; set; }
        public Logger Logger { get; set; }
        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        public IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();
            
            // Créer une requête factice - les données réelles viendront du parser
            var request = new IndexerRequest("ftps://dummy-request", HttpAccept.Html);
            request.HttpRequest.Method = HttpMethod.Get;
            
            pageableRequests.Add(new IndexerRequest[] { request });
            
            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            
            // Créer une requête factice avec les critères de recherche
            var searchUrl = $"ftps://search?title={searchCriteria.Movie?.Title}&year={searchCriteria.Movie?.Year}";
            var request = new IndexerRequest(searchUrl, HttpAccept.Html);
            request.HttpRequest.Method = HttpMethod.Get;
            
            pageableRequests.Add(new IndexerRequest[] { request });
            
            return pageableRequests;
        }
    }

    public class FtpsResponseParser : IParseIndexerResponse
    {
        public FtpsIndexerSettings Settings { get; set; }
        public IFtpsProxy FtpsProxy { get; set; }
        public Logger Logger { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var releases = new List<ReleaseInfo>();
            
            try
            {
                // Utiliser le proxy injecté au lieu de créer une nouvelle instance
                var directories = FtpsProxy.GetDirectoryListingAsync(Settings, Settings.MovieDirectory).Result;

                foreach (var directory in directories.Where(d => d.IsDirectory))
                {
                    try
                    {
                        var movieFiles = FtpsProxy.GetDirectoryListingAsync(Settings, $"{Settings.MovieDirectory}/{directory.Name}").Result;
                        
                        foreach (var file in movieFiles.Where(f => !f.IsDirectory && IsVideoFile(f.Name)))
                        {
                            var release = CreateReleaseInfo(directory.Name, file);
                            if (release != null)
                            {
                                releases.Add(release);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.Warn(ex, "Error scanning directory: {0}", directory.Name);
                    }
                }

                Logger?.Debug("FTPS Indexer found {0} releases", releases.Count);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error scanning FTPS server: {0}", Settings.Host);
            }

            return releases;
        }

        private ReleaseInfo CreateReleaseInfo(string directoryName, FtpsDirectoryItem file)
        {
            try
            {
                var fullPath = $"{Settings.MovieDirectory}/{directoryName}/{file.Name}";
                var title = $"{directoryName}";
                
                // Parser le nom du film
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
                Logger?.Warn(ex, "Error creating release info for file: {0}", file.Name);
                return null;
            }
        }

        private bool IsVideoFile(string fileName)
        {
            var videoExtensions = new[] { ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg", ".ts", ".m2ts" };
            return videoExtensions.Any(ext => fileName.ToLowerInvariant().EndsWith(ext));
        }
    }
}