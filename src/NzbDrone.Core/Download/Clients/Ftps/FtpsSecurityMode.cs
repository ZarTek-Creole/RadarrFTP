using System.ComponentModel;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public enum FtpsSecurityMode
    {
        [Description("Explicit (AUTH TLS)")]
        Explicit = 0,

        [Description("Implicit (SSL)")]
        Implicit = 1,

        [Description("None (Plain FTP)")]
        None = 2
    }
}