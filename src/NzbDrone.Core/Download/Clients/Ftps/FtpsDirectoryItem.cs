using System;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsDirectoryItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }
        public bool IsDirectory { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime LastModified { get; set; }
    }
}