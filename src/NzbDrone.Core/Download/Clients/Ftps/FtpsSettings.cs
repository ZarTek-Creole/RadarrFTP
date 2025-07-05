using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsSettingsValidator : AbstractValidator<FtpsSettings>
    {
        public FtpsSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
            RuleFor(c => c.TvCategory).NotEmpty();
        }
    }

    public class FtpsSettings : IProviderConfig
    {
        private static readonly FtpsSettingsValidator Validator = new FtpsSettingsValidator();

        public FtpsSettings()
        {
            Host = "localhost";
            Port = 21;
            UseSsl = true;
            TvCategory = @"/downloads/movies";
            MoviePaths = "/movies";
            RemoveCompletedDownloads = false;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL/TLS", Type = FieldType.Checkbox, HelpText = "Use FTPS (FTP over SSL/TLS)")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Username", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "Movie Category", Type = FieldType.Textbox, HelpText = "Local directory where movies will be downloaded")]
        public string TvCategory { get; set; }

        [FieldDefinition(6, Label = "Movie Paths", Type = FieldType.Textbox, HelpText = "Remote paths to search for movies (comma separated)")]
        public string MoviePaths { get; set; }

        [FieldDefinition(7, Label = "Remove Completed Downloads", Type = FieldType.Checkbox, HelpText = "Remove RAR files after extraction")]
        public bool RemoveCompletedDownloads { get; set; }

        [FieldDefinition(8, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(SabnzbdPriority), HelpText = "Priority to use when grabbing movies that aired within the last 14 days")]
        public int RecentTvPriority { get; set; }

        [FieldDefinition(9, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(SabnzbdPriority), HelpText = "Priority to use when grabbing movies that aired over 14 days ago")]
        public int OlderTvPriority { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum SabnzbdPriority
    {
        Default = -100,
        Paused = -2,
        Low = -1,
        Normal = 0,
        High = 1,
        Force = 2
    }
}




