using System.ComponentModel;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsSettingsValidator : AbstractValidator<FtpsSettings>
    {
        public FtpsSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
            RuleFor(c => c.BasePath).NotEmpty();
            RuleFor(c => c.MovieDirectory).NotEmpty();
        }
    }

    public class FtpsSettings : DownloadClientSettingsBase<FtpsSettings>
    {
        private static readonly FtpsSettingsValidator Validator = new();

        public FtpsSettings()
        {
            Host = "localhost";
            Port = 21;
            SecurityMode = FtpsSecurityMode.Explicit;
            ConnectionMode = FtpsConnectionMode.Passive;
            ValidateCertificate = true;
            BasePath = "/";
            MovieDirectory = "movies";
            Priority = 1;
            ScanInterval = 60;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Number)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "SSL/TLS Mode", Type = FieldType.Select, SelectOptions = typeof(FtpsSecurityMode), HelpText = "FTPS security mode")]
        public FtpsSecurityMode SecurityMode { get; set; }

        [FieldDefinition(5, Label = "Connection Mode", Type = FieldType.Select, SelectOptions = typeof(FtpsConnectionMode), HelpText = "FTP connection mode")]
        public FtpsConnectionMode ConnectionMode { get; set; }

        [FieldDefinition(6, Label = "Validate Certificate", Type = FieldType.Checkbox, HelpText = "Validate SSL/TLS certificate")]
        public bool ValidateCertificate { get; set; }

        [FieldDefinition(7, Label = "Base Path", Type = FieldType.Textbox, HelpText = "Base directory path on the FTPS server")]
        public string BasePath { get; set; }

        [FieldDefinition(8, Label = "Movie Directory", Type = FieldType.Textbox, HelpText = "Directory containing movies")]
        public string MovieDirectory { get; set; }

        [FieldDefinition(9, Label = "Priority", Type = FieldType.Number, HelpText = "Server priority (1-100)")]
        public int Priority { get; set; }

        [FieldDefinition(10, Label = "Scan Interval", Type = FieldType.Number, HelpText = "Scan interval in minutes")]
        public int ScanInterval { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}