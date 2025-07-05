using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;
using NLog;

namespace NzbDrone.Core.Download
{
    public abstract class FtpsClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected FtpsClientBase(IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            Logger logger,
            ILocalizationService localizationService)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
        }

        public override DownloadProtocol Protocol => DownloadProtocol.Ftps;

        protected abstract Task<bool> TestConnectionAsync();
        protected abstract Task<IEnumerable<string>> GetDirectoryListingAsync(string path);
        protected abstract Task<string> DownloadFileAsync(string remotePath, string localPath);
    }
}