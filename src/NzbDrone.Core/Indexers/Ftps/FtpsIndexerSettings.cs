using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Ftps
{
    public class FtpsIndexerSettingsValidator : AbstractValidator<FtpsIndexerSettings>
    {
        public FtpsIndexerSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
        }
    }

    public class FtpsIndexerSettings : IIndexerSettings
    {
        private static readonly FtpsIndexerSettingsValidator Validator = new FtpsIndexerSettingsValidator();

        public FtpsIndexerSettings()
        {
            Host = "localhost";
            Port = 21;
            UseSsl = true;
            MoviePaths = "/movies";
            BaseUrl = "";
            MultiLanguages = new List<int>();
            FailDownloads = new List<int>();
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

        [FieldDefinition(5, Label = "Movie Paths", Type = FieldType.Textbox, HelpText = "Remote paths to search for movies (comma separated)")]
        public string MoviePaths { get; set; }

        // Propriétés requises par IIndexerSettings
        public string BaseUrl { get; set; }
        public IEnumerable<int> MultiLanguages { get; set; }
        public IEnumerable<int> FailDownloads { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}




