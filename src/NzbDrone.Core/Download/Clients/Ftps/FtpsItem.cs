using System;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsItem
    {
        public string DownloadId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public string RemotePath { get; set; }
        public OsPath OutputPath { get; set; }
        public FtpsDownloadStatus Status { get; set; }
        public string Message { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateCompleted { get; set; }
        public bool CanBeRemoved { get; set; }
        public bool CanMoveFiles { get; set; }
        public float? Progress { get; set; }
        public string ServerId { get; set; }
        public string ReleaseGroup { get; set; }
        public string Quality { get; set; }
        public string Source { get; set; }
        public string Codec { get; set; }
        public int? Year { get; set; }
        public bool IsEncrypted { get; set; }
        public string Hash { get; set; }
        public string HashType { get; set; }

        public FtpsItem()
        {
            DateAdded = DateTime.UtcNow;
            CanBeRemoved = true;
            CanMoveFiles = true;
            Status = FtpsDownloadStatus.Queued;
        }

        public bool IsCompleted => Status == FtpsDownloadStatus.Completed;
        public bool IsFailed => Status == FtpsDownloadStatus.Failed;
        public bool IsDownloading => Status == FtpsDownloadStatus.Downloading;
        public bool IsQueued => Status == FtpsDownloadStatus.Queued;
        public bool IsPaused => Status == FtpsDownloadStatus.Paused;

        public void UpdateProgress(long downloadedBytes)
        {
            if (TotalSize > 0)
            {
                Progress = (float)(TotalSize - RemainingSize) / TotalSize * 100;
                RemainingSize = Math.Max(0, TotalSize - downloadedBytes);
            }
        }

        public void MarkAsCompleted()
        {
            Status = FtpsDownloadStatus.Completed;
            DateCompleted = DateTime.UtcNow;
            Progress = 100.0f;
            RemainingSize = 0;
            RemainingTime = TimeSpan.Zero;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = FtpsDownloadStatus.Failed;
            Message = errorMessage;
        }

        public void MarkAsStarted()
        {
            Status = FtpsDownloadStatus.Downloading;
        }

        public void MarkAsPaused()
        {
            Status = FtpsDownloadStatus.Paused;
        }
    }

    public class FtpsReleaseItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }
        public DateTime ModifiedTime { get; set; }
        public bool IsDirectory { get; set; }
        public string Extension { get; set; }
        public string ReleaseGroup { get; set; }
        public string Quality { get; set; }
        public string Source { get; set; }
        public string Codec { get; set; }
        public int? Year { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
        public string ServerId { get; set; }

        public FtpsReleaseItem()
        {
            ModifiedTime = DateTime.UtcNow;
        }

        public FtpsReleaseItem(string name, string fullPath, long size, DateTime modifiedTime, bool isDirectory)
        {
            Name = name;
            FullPath = fullPath;
            Size = size;
            ModifiedTime = modifiedTime;
            IsDirectory = isDirectory;
            Extension = isDirectory ? "" : System.IO.Path.GetExtension(name);
        }

        public bool IsVideoFile => !IsDirectory && IsVideoExtension(Extension);

        private static bool IsVideoExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            var videoExtensions = new[] { ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg", ".ts", ".m2ts" };
            return Array.Exists(videoExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsArchive => !IsDirectory && IsArchiveExtension(Extension);

        private static bool IsArchiveExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            var archiveExtensions = new[] { ".rar", ".zip", ".7z", ".tar", ".gz", ".bz2" };
            return Array.Exists(archiveExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class FtpsServerInfo
    {
        public string Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public DateTime? LastConnected { get; set; }
        public DateTime? LastError { get; set; }
        public string LastErrorMessage { get; set; }
        public string ServerType { get; set; }
        public string ServerVersion { get; set; }
        public bool SupportsResume { get; set; }
        public bool SupportsUtf8 { get; set; }
        public long TotalSpace { get; set; }
        public long FreeSpace { get; set; }
        public int Priority { get; set; }
        public bool IsOnline { get; set; }

        public FtpsServerInfo()
        {
            Id = Guid.NewGuid().ToString();
            IsOnline = false;
            Priority = 1;
        }
    }
}