using FluentValidation;

namespace FtpsDemo.Models
{
    public enum FtpsSecurityMode
    {
        None = 0,
        Explicit = 1,
        Implicit = 2
    }

    public enum FtpsConnectionMode
    {
        Passive = 0,
        Active = 1
    }

    public class FtpsSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 21;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string BasePath { get; set; } = "/";
        public string MovieDirectory { get; set; } = "movies";
        public FtpsSecurityMode SecurityMode { get; set; } = FtpsSecurityMode.Explicit;
        public FtpsConnectionMode ConnectionMode { get; set; } = FtpsConnectionMode.Passive;
        public bool AcceptInvalidCertificates { get; set; } = false;
    }

    public class FtpsSettingsValidator : AbstractValidator<FtpsSettings>
    {
        public FtpsSettingsValidator()
        {
            RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required");
            RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535");
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
            RuleFor(x => x.BasePath).NotEmpty().WithMessage("Base path is required");
            RuleFor(x => x.MovieDirectory).NotEmpty().WithMessage("Movie directory is required");
        }
    }
}