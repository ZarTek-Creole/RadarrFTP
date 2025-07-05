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
    public class FtpsIndexer : HttpIndexerBase<FtpsIndexerSettings>
    {
        public override string Name => "FTPS Indexer";
        public override DownloadProtocol Protocol => DownloadProtocol.Ftps;
        public override int PageSize => 0;

        public FtpsIndexer(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new FtpsRequestGenerator();
        }

        public override IParseIndexerResponse GetParser()
        {
            return new FtpsResponseParser();
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

    public class FtpsRequestGenerator : IIndexerRequestGenerator
    {
        public IndexerPageableRequestChain GetRecentRequests()
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }

    public class FtpsResponseParser : IParseIndexerResponse
    {
        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            return new List<ReleaseInfo>();
        }

        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}