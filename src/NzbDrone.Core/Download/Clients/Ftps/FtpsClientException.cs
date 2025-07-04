using System;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsClientException : DownloadClientException
    {
        public FtpsClientException(string message) : base(message)
        {
        }

        public FtpsClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class FtpsConnectionException : FtpsClientException
    {
        public FtpsConnectionException(string message) : base($"FTPS Connection Error: {message}")
        {
        }

        public FtpsConnectionException(string message, Exception innerException) : base($"FTPS Connection Error: {message}", innerException)
        {
        }
    }

    public class FtpsAuthenticationException : FtpsClientException
    {
        public FtpsAuthenticationException(string message) : base($"FTPS Authentication Error: {message}")
        {
        }

        public FtpsAuthenticationException(string message, Exception innerException) : base($"FTPS Authentication Error: {message}", innerException)
        {
        }
    }

    public class FtpsTransferException : FtpsClientException
    {
        public string RemotePath { get; }
        public string LocalPath { get; }

        public FtpsTransferException(string message, string remotePath = null, string localPath = null) 
            : base($"FTPS Transfer Error: {message}")
        {
            RemotePath = remotePath;
            LocalPath = localPath;
        }

        public FtpsTransferException(string message, Exception innerException, string remotePath = null, string localPath = null) 
            : base($"FTPS Transfer Error: {message}", innerException)
        {
            RemotePath = remotePath;
            LocalPath = localPath;
        }
    }

    public class FtpsPathException : FtpsClientException
    {
        public string Path { get; }

        public FtpsPathException(string message, string path = null) : base($"FTPS Path Error: {message}")
        {
            Path = path;
        }

        public FtpsPathException(string message, Exception innerException, string path = null) 
            : base($"FTPS Path Error: {message}", innerException)
        {
            Path = path;
        }
    }

    public class FtpsCertificateException : FtpsClientException
    {
        public FtpsCertificateException(string message) : base($"FTPS Certificate Error: {message}")
        {
        }

        public FtpsCertificateException(string message, Exception innerException) : base($"FTPS Certificate Error: {message}", innerException)
        {
        }
    }

    public class FtpsTimeoutException : FtpsClientException
    {
        public TimeSpan Timeout { get; }

        public FtpsTimeoutException(string message, TimeSpan timeout) : base($"FTPS Timeout Error: {message} (Timeout: {timeout})")
        {
            Timeout = timeout;
        }

        public FtpsTimeoutException(string message, Exception innerException, TimeSpan timeout) 
            : base($"FTPS Timeout Error: {message} (Timeout: {timeout})", innerException)
        {
            Timeout = timeout;
        }
    }
}