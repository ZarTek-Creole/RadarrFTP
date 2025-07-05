using FluentFTP;
using FtpsDemo.Models;
using System.Security.Authentication;

namespace FtpsDemo.Services
{
    public class FtpsDirectoryItem
    {
        public string Name { get; set; } = "";
        public string FullPath { get; set; } = "";
        public long Size { get; set; }
        public bool IsDirectory { get; set; }
    }

    public interface IFtpsService
    {
        Task<bool> TestConnectionAsync(FtpsSettings settings);
        Task<List<FtpsDirectoryItem>> GetDirectoryListingAsync(FtpsSettings settings, string path);
        Task<bool> DownloadFileAsync(FtpsSettings settings, string remotePath, string localPath);
    }

    public class FtpsService : IFtpsService
    {
        public async Task<bool> TestConnectionAsync(FtpsSettings settings)
        {
            try
            {
                using var client = CreateFtpClient(settings);
                await client.Connect();
                await client.GetWorkingDirectory();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<FtpsDirectoryItem>> GetDirectoryListingAsync(FtpsSettings settings, string path)
        {
            var items = new List<FtpsDirectoryItem>();
            
            try
            {
                using var client = CreateFtpClient(settings);
                await client.Connect();
                
                var ftpItems = await client.GetListing(path);
                
                foreach (var item in ftpItems)
                {
                    items.Add(new FtpsDirectoryItem
                    {
                        Name = item.Name,
                        FullPath = item.FullName,
                        Size = item.Size,
                        IsDirectory = item.Type == FtpObjectType.Directory
                    });
                }
            }
            catch
            {
                // Return empty list on error
            }
            
            return items;
        }

        public async Task<bool> DownloadFileAsync(FtpsSettings settings, string remotePath, string localPath)
        {
            try
            {
                using var client = CreateFtpClient(settings);
                await client.Connect();
                
                var result = await client.DownloadFile(localPath, remotePath, 
                    FtpLocalExists.Overwrite, FtpVerify.Retry);
                
                return result == FtpStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private AsyncFtpClient CreateFtpClient(FtpsSettings settings)
        {
            var client = new AsyncFtpClient(settings.Host, settings.Username, settings.Password, settings.Port);
            
            // Configure encryption
            client.Config.EncryptionMode = settings.SecurityMode switch
            {
                FtpsSecurityMode.None => FtpEncryptionMode.None,
                FtpsSecurityMode.Explicit => FtpEncryptionMode.Explicit,
                FtpsSecurityMode.Implicit => FtpEncryptionMode.Implicit,
                _ => FtpEncryptionMode.Explicit
            };

            // Configure data connection
            client.Config.DataConnectionType = settings.ConnectionMode == FtpsConnectionMode.Passive 
                ? FtpDataConnectionType.PASV 
                : FtpDataConnectionType.PORT;

            // SSL/TLS configuration
            if (settings.SecurityMode != FtpsSecurityMode.None)
            {
                client.Config.SslProtocols = SslProtocols.Tls12;
                
                if (settings.AcceptInvalidCertificates)
                {
                    client.Config.ValidateAnyCertificate = true;
                }
            }

            return client;
        }
    }
}