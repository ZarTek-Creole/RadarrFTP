using System.ComponentModel;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public enum FtpsConnectionMode
    {
        [Description("Passive")]
        Passive = 0,

        [Description("Active")]
        Active = 1
    }
}