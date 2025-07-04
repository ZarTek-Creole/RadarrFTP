using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public interface IFtpsProxy
    {
        Task<bool> TestConnectionAsync(FtpsSettings settings, CancellationToken cancellationToken = default);
        Task<IEnumerable<FtpsReleaseItem>> GetDirectoryListingAsync(string path, FtpsSettings settings, CancellationToken cancellationToken = default);
        Task<bool> DownloadFileAsync(string remotePath, string localPath, FtpsSettings settings, IProgress<FtpProgress> progress = null, CancellationToken cancellationToken = default);
        Task<bool> FileExistsAsync(string remotePath, FtpsSettings settings, CancellationToken cancellationToken = default);
        Task<long> GetFileSizeAsync(string remotePath, FtpsSettings settings, CancellationToken cancellationToken = default);
        Task<DateTime> GetModifiedTimeAsync(string remotePath, FtpsSettings settings, CancellationToken cancellationToken = default);
        Task<FtpsServerInfo> GetServerInfoAsync(FtpsSettings settings, CancellationToken cancellationToken = default);
        Task<string> GetHashAsync(string remotePath, FtpsSettings settings, FtpHashAlgorithm algorithm = FtpHashAlgorithm.MD5, CancellationToken cancellationToken = default);
        Task<bool> DirectoryExistsAsync(string remotePath, FtpsSettings settings, CancellationToken cancellationToken = default);
    }

    public class FtpsProxy : IFtpsProxy
    {
        private readonly Logger _logger;

        public FtpsProxy(Logger logger)
        {
            _logger = logger;
        }

        public async Task<bool> TestConnectionAsync(FtpsSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                
                // Test basic connectivity
                var workingDir = await client.GetWorkingDirectory(cancellationToken);
                _logger.Debug("FTPS connection test successful. Working directory: {0}", workingDir);
                
                // Test if base path exists
                if (!string.IsNullOrWhiteSpace(settings.RemoteBasePath))
                {
                    var basePathExists = await client.DirectoryExists(settings.RemoteBasePath, cancellationToken);
                    if (!basePathExists)
                    {
                        _logger.Warn("Base path does not exist: {0}", settings.RemoteBasePath);
                        return false;
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "FTPS connection test failed for {0}:{1}", settings.Host, settings.Port);
                return false;
            }
        }

        public async Task<IEnumerable<FtpsReleaseItem>> GetDirectoryListingAsync(string path, FtpsSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                
                var listing = await client.GetListing(path, FtpListOption.Recursive | FtpListOption.Size | FtpListOption.Modify, cancellationToken);
                
                return listing.Select(item => new FtpsReleaseItem
                {
                    Name = item.Name,
                    FullPath = item.FullName,
                    Size = item.Size,
                    ModifiedTime = item.Modified,
                    IsDirectory = item.Type == FtpObjectType.Directory,
                    Extension = item.Type == FtpObjectType.File ? Path.GetExtension(item.Name) : "",
                    ServerId = $"{settings.Host}:{settings.Port}"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get directory listing for path: {0}", path);
                throw new FtpsPathException($"Failed to get directory listing for path: {path}", ex, path);
            }
        }

        public async Task<bool> DownloadFileAsync(string remotePath, string localPath, FtpsSettings settings, IProgress<FtpProgress> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                
                // Ensure local directory exists
                var localDir = Path.GetDirectoryName(localPath);
                if (!Directory.Exists(localDir))
                {
                    Directory.CreateDirectory(localDir);
                }

                var result = await client.DownloadFile(localPath, remotePath, FtpLocalExists.Overwrite, FtpVerify.Retry, progress, cancellationToken);
                
                if (result == FtpStatus.Success)
                {
                    _logger.Debug("Successfully downloaded file from {0} to {1}", remotePath, localPath);
                    return true;
                }
                else
                {
                    _logger.Error("Failed to download file from {0} to {1}. Status: {2}", remotePath, localPath, result);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error downloading file from {0} to {1}", remotePath, localPath);
                throw new FtpsTransferException($"Failed to download file: {ex.Message}", ex, remotePath, localPath);
            }
        }

        public async Task<bool> FileExistsAsync(string remotePath, FtpsSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                return await client.FileExists(remotePath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking if file exists: {0}", remotePath);
                throw new FtpsPathException($"Failed to check if file exists: {remotePath}", ex, remotePath);
            }
        }

        public async Task<long> GetFileSizeAsync(string remotePath, FtpsSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                return await client.GetFileSize(remotePath, -1, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting file size: {0}", remotePath);
                throw new FtpsPathException($"Failed to get file size: {remotePath}", ex, remotePath);
            }
        }

        public async Task<DateTime> GetModifiedTimeAsync(string remotePath, FtpsSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                return await client.GetModifiedTime(remotePath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting modified time: {0}", remotePath);
                throw new FtpsPathException($"Failed to get modified time: {remotePath}", ex, remotePath);
            }
        }

        public async Task<FtpsServerInfo> GetServerInfoAsync(FtpsSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                
                var serverInfo = new FtpsServerInfo
                {
                    Host = settings.Host,
                    Port = settings.Port,
                    Name = $"{settings.Host}:{settings.Port}",
                    IsConnected = client.IsConnected,
                    LastConnected = DateTime.UtcNow,
                    IsOnline = true,
                    Priority = settings.Priority
                };

                // Get server system information
                try
                {
                    var systemType = await client.GetSystem(cancellationToken);
                    serverInfo.ServerType = systemType;
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Could not get server system type");
                }

                // Check capabilities
                try
                {
                    serverInfo.SupportsResume = client.HasFeature(FtpCapability.REST);
                    serverInfo.SupportsUtf8 = client.HasFeature(FtpCapability.UTF8);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Could not get server capabilities");
                }

                return serverInfo;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting server info for {0}:{1}", settings.Host, settings.Port);
                throw new FtpsConnectionException($"Failed to get server info: {ex.Message}", ex);
            }
        }

        public async Task<string> GetHashAsync(string remotePath, FtpsSettings settings, FtpHashAlgorithm algorithm = FtpHashAlgorithm.MD5, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                
                if (!client.HasFeature(FtpCapability.HASH))
                {
                    _logger.Debug("Server does not support HASH command");
                    return null;
                }

                var hash = await client.GetHash(remotePath, algorithm, cancellationToken);
                return hash?.Value;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting hash for file: {0}", remotePath);
                return null;
            }
        }

        public async Task<bool> DirectoryExistsAsync(string remotePath, FtpsSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(settings, cancellationToken);
                return await client.DirectoryExists(remotePath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking if directory exists: {0}", remotePath);
                throw new FtpsPathException($"Failed to check if directory exists: {remotePath}", ex, remotePath);
            }
        }

        private async Task<AsyncFtpClient> CreateClientAsync(FtpsSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = new AsyncFtpClient(settings.Host, settings.Port, settings.Username, settings.Password);
                
                // Configure encryption
                if (settings.UseSsl)
                {
                    client.Config.EncryptionMode = (FtpEncryptionMode)settings.EncryptionMode;
                    client.Config.DataConnectionEncryption = true;
                    client.Config.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                    
                    if (!settings.ValidateCertificate)
                    {
                        client.Config.ValidateAnyCertificate = true;
                        _logger.Warn("SSL certificate validation is disabled for {0}:{1}", settings.Host, settings.Port);
                    }
                    else
                    {
                        // Custom certificate validation
                        client.ValidateCertificate += (control, e) =>
                        {
                            if (e.PolicyErrors != SslPolicyErrors.None)
                            {
                                _logger.Error("SSL certificate validation failed for {0}:{1} - {2}", 
                                    settings.Host, settings.Port, e.PolicyErrors);
                                e.Accept = false;
                            }
                            else
                            {
                                e.Accept = true;
                            }
                        };
                    }
                }
                else
                {
                    client.Config.EncryptionMode = FtpEncryptionMode.None;
                }

                // Configure data connection
                client.Config.DataConnectionType = (FtpDataConnectionType)settings.DataConnectionType;
                
                // Configure timeouts
                client.Config.ConnectTimeout = TimeSpan.FromSeconds(settings.ConnectionTimeout);
                client.Config.ReadTimeout = TimeSpan.FromSeconds(settings.ReadTimeout);
                client.Config.DataConnectionConnectTimeout = TimeSpan.FromSeconds(settings.ConnectionTimeout);
                client.Config.DataConnectionReadTimeout = TimeSpan.FromSeconds(settings.ReadTimeout);
                
                // Configure performance
                client.Config.TransferChunkSize = settings.TransferChunkSize;
                client.Config.RetryAttempts = settings.RetryAttempts;
                
                // Configure behavior
                client.Config.SocketKeepAlive = true;
                client.Config.StaleDataCheck = true;
                
                // Connect with auto-detection of optimal settings
                await client.AutoConnect(cancellationToken);
                
                if (!client.IsConnected)
                {
                    throw new FtpsConnectionException("Failed to connect to FTPS server");
                }

                _logger.Debug("Successfully connected to FTPS server {0}:{1}", settings.Host, settings.Port);
                return client;
            }
            catch (AuthenticationException ex)
            {
                throw new FtpsAuthenticationException($"Authentication failed: {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                throw new FtpsTimeoutException($"Connection timeout: {ex.Message}", ex, TimeSpan.FromSeconds(settings.ConnectionTimeout));
            }
            catch (Exception ex)
            {
                throw new FtpsConnectionException($"Failed to create FTPS client: {ex.Message}", ex);
            }
        }
    }
}