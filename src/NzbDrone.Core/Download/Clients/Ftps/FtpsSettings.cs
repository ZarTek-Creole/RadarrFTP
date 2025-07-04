using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsSettingsValidator : AbstractValidator<FtpsSettings>
    {
        public FtpsSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            
            RuleFor(c => c.Username).NotEmpty()
                                    .WithMessage("Username is required for FTPS connections");
            
            RuleFor(c => c.Password).NotEmpty()
                                    .WithMessage("Password is required for FTPS connections");
            
            RuleFor(c => c.RemoteBasePath).NotEmpty()
                                          .WithMessage("Remote base path is required")
                                          .Must(path => path.StartsWith("/"))
                                          .WithMessage("Remote base path must start with '/'");
            
            RuleFor(c => c.ConnectionTimeout).GreaterThan(0)
                                             .WithMessage("Connection timeout must be greater than 0");
        }
    }

    public class FtpsSettings : DownloadClientSettingsBase<FtpsSettings>
    {
        private static readonly FtpsSettingsValidator Validator = new();

        public FtpsSettings()
        {
            Host = "localhost";
            Port = 21;
            Username = "";
            Password = "";
            EncryptionMode = (int)FtpEncryptionMode.Explicit;
            ValidateCertificate = true;
            DataConnectionType = (int)FtpDataConnectionType.AutoPassive;
            RemoteBasePath = "/releases/movies/";
            Priority = 1;
            ConnectionTimeout = 30;
            ReadTimeout = 30;
            TransferChunkSize = 1048576; // 1MB
            RetryAttempts = 3;
            MonitoringEnabled = true;
            MonitoringInterval = 300; // 5 minutes
            Category = "movies";
            UseSsl = true;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox, HelpText = "FTPS server hostname or IP address")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox, HelpText = "FTPS server port (default: 21 for explicit FTPS, 990 for implicit)")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Enable SSL/TLS encryption")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "SSL/TLS Mode", Type = FieldType.Select, SelectOptions = typeof(FtpEncryptionMode), 
                         HelpText = "Explicit: Upgrade to SSL after initial connection. Implicit: Direct SSL connection")]
        public int EncryptionMode { get; set; }

        [FieldDefinition(6, Label = "Validate Certificate", Type = FieldType.Checkbox, 
                         HelpText = "Enable strict SSL certificate validation (disable only for testing)")]
        public bool ValidateCertificate { get; set; }

        [FieldDefinition(7, Label = "Transfer Mode", Type = FieldType.Select, SelectOptions = typeof(FtpDataConnectionType),
                         HelpText = "Data connection type (Passive recommended for firewalls)")]
        public int DataConnectionType { get; set; }

        [FieldDefinition(8, Label = "Remote Base Path", Type = FieldType.Textbox, 
                         HelpText = "Base directory path on FTPS server where movies are located")]
        public string RemoteBasePath { get; set; }

        [FieldDefinition(9, Label = "Category", Type = FieldType.Textbox, 
                         HelpText = "Category folder for movie downloads")]
        public string Category { get; set; }

        [FieldDefinition(10, Label = "Priority", Type = FieldType.Textbox, 
                         HelpText = "Server priority when multiple FTPS servers are configured")]
        public int Priority { get; set; }

        [FieldDefinition(11, Label = "Connection Timeout (seconds)", Type = FieldType.Textbox, Advanced = true)]
        public int ConnectionTimeout { get; set; }

        [FieldDefinition(12, Label = "Read Timeout (seconds)", Type = FieldType.Textbox, Advanced = true)]
        public int ReadTimeout { get; set; }

        [FieldDefinition(13, Label = "Transfer Chunk Size (bytes)", Type = FieldType.Textbox, Advanced = true, 
                         HelpText = "Size of data chunks for file transfers (default: 1MB)")]
        public int TransferChunkSize { get; set; }

        [FieldDefinition(14, Label = "Retry Attempts", Type = FieldType.Textbox, Advanced = true)]
        public int RetryAttempts { get; set; }

        [FieldDefinition(15, Label = "Enable Monitoring", Type = FieldType.Checkbox, Advanced = true,
                         HelpText = "Automatically scan server for new releases")]
        public bool MonitoringEnabled { get; set; }

        [FieldDefinition(16, Label = "Monitoring Interval (seconds)", Type = FieldType.Textbox, Advanced = true)]
        public int MonitoringInterval { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum FtpEncryptionMode
    {
        None = 0,
        Explicit = 1,
        Implicit = 2,
        Auto = 3
    }

    public enum FtpDataConnectionType
    {
        AutoPassive = 0,
        PASV = 1,
        EPSV = 2,
        PORT = 3,
        EPRT = 4
    }
}