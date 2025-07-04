namespace NzbDrone.Core.Download.Clients.Ftps
{
    public enum FtpsDownloadStatus
    {
        Unknown = 0,
        Queued = 1,
        Downloading = 2,
        Paused = 3,
        Completed = 4,
        Failed = 5,
        Warning = 6,
        Cancelled = 7,
        Verifying = 8,
        Moving = 9
    }
}