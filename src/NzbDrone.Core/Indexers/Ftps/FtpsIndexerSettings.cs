using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Ftps
{
    public class FtpsIndexerSettingsValidator : AbstractValidator<FtpsIndexerSettings>
    {
        public FtpsIndexerSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty().WithMessage("Host is required");
            RuleFor(c => c.Port).InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535");
            RuleFor(c => c.Username).NotEmpty().WithMessage("Username is required");
            RuleFor(c => c.Password).NotEmpty().WithMessage("Password is required");
            RuleFor(c => c.MovieDirectory).NotEmpty().WithMessage("Movie Directory is required");
            RuleFor(c => c.ScanInterval).GreaterThan(0).WithMessage("Scan Interval must be greater than 0");
        }
    }

    public class FtpsIndexerSettings : IIndexerSettings
    {
        private static readonly FtpsIndexerSettingsValidator Validator = new();

        public FtpsIndexerSettings()
        {
            Port = 21;
            SecurityMode = FtpsSecurityMode.Explicit;
            ConnectionMode = FtpsConnectionMode.Passive;
            ValidateCertificate = false;
            MovieDirectory = "movies";
            ScanInterval = 30; // 30 minutes
            MaxDepth = 2;
            MultiLanguages = Enumerable.Empty<int>();
            FailDownloads = Enumerable.Empty<int>();
        }

        [FieldDefinition(0, Label = "Host", HelpText = "FTPS server hostname or IP address")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", HelpText = "FTPS server port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Username", HelpText = "FTPS username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password, HelpText = "FTPS password", Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "SSL/TLS Mode", Type = FieldType.Select, SelectOptions = typeof(FtpsSecurityMode), HelpText = "FTPS security mode")]
        public FtpsSecurityMode SecurityMode { get; set; }

        [FieldDefinition(5, Label = "Connection Mode", Type = FieldType.Select, SelectOptions = typeof(FtpsConnectionMode), HelpText = "FTP connection mode")]
        public FtpsConnectionMode ConnectionMode { get; set; }

        [FieldDefinition(6, Label = "Validate Certificate", Type = FieldType.Checkbox, HelpText = "Validate SSL/TLS certificate")]
        public bool ValidateCertificate { get; set; }

        [FieldDefinition(7, Label = "Base Path", HelpText = "Base directory path on the FTPS server")]
        public string BasePath { get; set; }

        [FieldDefinition(8, Label = "Movie Directory", HelpText = "Directory containing movies")]
        public string MovieDirectory { get; set; }

        [FieldDefinition(9, Label = "Scan Interval", Type = FieldType.Number, HelpText = "Scan interval in minutes")]
        public int ScanInterval { get; set; }

        [FieldDefinition(10, Label = "Max Depth", Type = FieldType.Number, HelpText = "Maximum directory depth to scan", Advanced = true)]
        public int MaxDepth { get; set; }

        [FieldDefinition(11, Label = "Additional Tags", HelpText = "Additional tags to look for in filenames (comma separated)", Advanced = true)]
        public string AdditionalTags { get; set; }

        // Required by IIndexerSettings interface
        public string BaseUrl 
        { 
            get => $"ftps://{Host}:{Port}";
            set => _ = value; // Not used for FTPS
        }

        public IEnumerable<int> MultiLanguages { get; set; }
        public IEnumerable<int> FailDownloads { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}