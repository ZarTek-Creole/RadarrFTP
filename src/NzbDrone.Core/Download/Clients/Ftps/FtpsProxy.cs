using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FluentFTP;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public interface IFtpsProxy
    {
        Task<bool> TestConnectionAsync(FtpsSettings settings);
        Task<IEnumerable<FtpsDirectoryItem>> GetDirectoryListingAsync(FtpsSettings settings, string path);
        Task<bool> DownloadFileAsync(FtpsSettings settings, string remotePath, string localPath);
        Task<bool> FileExistsAsync(FtpsSettings settings, string path);
        Task<long> GetFileSizeAsync(FtpsSettings settings, string path);
    }

    public class FtpsProxy : IFtpsProxy
    {
        private readonly Logger _logger;

        public FtpsProxy(Logger logger)
        {
            _logger = logger;
        }

        public async Task<bool> TestConnectionAsync(FtpsSettings settings)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                return client.IsConnected;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test FTPS connection to {0}:{1}", settings.Host, settings.Port);
                return false;
            }
        }

        public async Task<IEnumerable<FtpsDirectoryItem>> GetDirectoryListingAsync(FtpsSettings settings, string path)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                var items = await client.GetListingAsync(path);
                
                return items.Select(item => new FtpsDirectoryItem
                {
                    Name = item.Name,
                    FullPath = item.FullName,
                    Size = item.Size,
                    IsDirectory = item.Type == FtpObjectType.Directory,
                    ModifiedDate = item.Modified
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get directory listing for {0}", path);
                throw;
            }
        }

        public async Task<bool> DownloadFileAsync(FtpsSettings settings, string remotePath, string localPath)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                var result = await client.DownloadFileAsync(localPath, remotePath);
                return result.IsSuccess();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download file from {0} to {1}", remotePath, localPath);
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(FtpsSettings settings, string path)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                return await client.FileExistsAsync(path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to check if file exists: {0}", path);
                return false;
            }
        }

        public async Task<long> GetFileSizeAsync(FtpsSettings settings, string path)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                return await client.GetFileSizeAsync(path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get file size for: {0}", path);
                return 0;
            }
        }

        private FtpClient CreateClient(FtpsSettings settings)
        {
            var client = new FtpClient(settings.Host, settings.Port, settings.Username, settings.Password);
            
            // Configuration SSL/TLS
            switch (settings.SecurityMode)
            {
                case FtpsSecurityMode.Explicit:
                    client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
                    client.Config.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    break;
                    
                case FtpsSecurityMode.Implicit:
                    client.Config.EncryptionMode = FtpEncryptionMode.Implicit;
                    client.Config.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    break;
                    
                case FtpsSecurityMode.None:
                    client.Config.EncryptionMode = FtpEncryptionMode.None;
                    break;
            }
            
            // Configuration du mode de connexion
            client.Config.DataConnectionType = settings.ConnectionMode == FtpsConnectionMode.Active 
                ? FtpDataConnectionType.AutoActive 
                : FtpDataConnectionType.AutoPassive;
            
            // Configuration de la validation des certificats
            if (!settings.ValidateCertificate)
            {
                client.Config.ValidateAnyCertificate = true;
            }
            
            return client;
        }
    }
}