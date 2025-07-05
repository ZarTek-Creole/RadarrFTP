using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.Clients.Ftps
{
    [TestFixture]
    public class FtpsSettingsFixture : TestBase
    {
        private FtpsSettings _settings;
        private FtpsSettingsValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new FtpsSettingsValidator();
            _settings = new FtpsSettings();
        }

        [Test]
        public void should_be_valid_when_all_required_properties_are_set()
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.BasePath = "/movies";
            _settings.MovieDirectory = "releases";

            // Act
            var result = _validator.Validate(_settings);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public void should_be_invalid_when_host_is_empty()
        {
            // Arrange
            _settings.Host = "";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";

            // Act
            var result = _validator.TestValidate(_settings);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Host);
        }

        [Test]
        public void should_be_invalid_when_host_is_null()
        {
            // Arrange
            _settings.Host = null;
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";

            // Act
            var result = _validator.TestValidate(_settings);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Host);
        }

        [Test]
        public void should_be_invalid_when_host_is_whitespace()
        {
            // Arrange
            _settings.Host = "   ";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";

            // Act
            var result = _validator.TestValidate(_settings);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Host);
        }

        [Test]
        public void should_be_invalid_when_username_is_empty()
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = 21;
            _settings.Username = "";
            _settings.Password = "testpass";

            // Act
            var result = _validator.TestValidate(_settings);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Test]
        public void should_be_invalid_when_password_is_empty()
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "";

            // Act
            var result = _validator.TestValidate(_settings);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(65536)]
        [TestCase(100000)]
        public void should_be_invalid_when_port_is_out_of_range(int port)
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = port;
            _settings.Username = "testuser";
            _settings.Password = "testpass";

            // Act
            var result = _validator.TestValidate(_settings);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Port);
        }

        [TestCase(1)]
        [TestCase(21)]
        [TestCase(990)]
        [TestCase(65535)]
        public void should_be_valid_when_port_is_in_range(int port)
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = port;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.BasePath = "/";

            // Act
            var result = _validator.Validate(_settings);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void should_use_default_values_correctly()
        {
            // Arrange & Act
            var settings = new FtpsSettings();

            // Assert
            settings.Port.Should().Be(21);
            settings.SecurityMode.Should().Be(FtpsSecurityMode.Explicit);
            settings.ConnectionMode.Should().Be(FtpsConnectionMode.Passive);
            settings.AcceptInvalidCertificates.Should().BeFalse();
            settings.BasePath.Should().Be("/");
            settings.MovieDirectory.Should().Be("movies");
        }

        [Test]
        public void should_set_properties_correctly()
        {
            // Arrange
            var host = "test.server.com";
            var port = 990;
            var username = "testuser";
            var password = "testpass";
            var basePath = "/ftps";
            var movieDirectory = "releases";
            var securityMode = FtpsSecurityMode.Implicit;
            var connectionMode = FtpsConnectionMode.Active;
            var acceptInvalidCerts = true;

            // Act
            _settings.Host = host;
            _settings.Port = port;
            _settings.Username = username;
            _settings.Password = password;
            _settings.BasePath = basePath;
            _settings.MovieDirectory = movieDirectory;
            _settings.SecurityMode = securityMode;
            _settings.ConnectionMode = connectionMode;
            _settings.AcceptInvalidCertificates = acceptInvalidCerts;

            // Assert
            _settings.Host.Should().Be(host);
            _settings.Port.Should().Be(port);
            _settings.Username.Should().Be(username);
            _settings.Password.Should().Be(password);
            _settings.BasePath.Should().Be(basePath);
            _settings.MovieDirectory.Should().Be(movieDirectory);
            _settings.SecurityMode.Should().Be(securityMode);
            _settings.ConnectionMode.Should().Be(connectionMode);
            _settings.AcceptInvalidCertificates.Should().Be(acceptInvalidCerts);
        }

        [Test]
        public void should_validate_base_path_format()
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.BasePath = "invalid-path"; // Should start with /

            // Act
            var result = _validator.TestValidate(_settings);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BasePath);
        }

        [TestCase("/")]
        [TestCase("/movies")]
        [TestCase("/path/to/movies")]
        [TestCase("/deep/nested/path/movies")]
        public void should_accept_valid_base_paths(string basePath)
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.BasePath = basePath;

            // Act
            var result = _validator.Validate(_settings);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_invalid_when_movie_directory_is_empty()
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.MovieDirectory = "";

            // Act
            var result = _validator.TestValidate(_settings);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MovieDirectory);
        }

        [Test]
        public void should_have_correct_enum_values()
        {
            // Assert Security Modes
            FtpsSecurityMode.None.Should().Be((FtpsSecurityMode)0);
            FtpsSecurityMode.Explicit.Should().Be((FtpsSecurityMode)1);
            FtpsSecurityMode.Implicit.Should().Be((FtpsSecurityMode)2);

            // Assert Connection Modes
            FtpsConnectionMode.Passive.Should().Be((FtpsConnectionMode)0);
            FtpsConnectionMode.Active.Should().Be((FtpsConnectionMode)1);
        }

        [Test]
        public void should_convert_settings_to_ftps_settings()
        {
            // Arrange
            var originalSettings = new FtpsSettings
            {
                Host = "test.com",
                Port = 990,
                Username = "user",
                Password = "pass",
                BasePath = "/movies",
                MovieDirectory = "releases",
                SecurityMode = FtpsSecurityMode.Implicit,
                ConnectionMode = FtpsConnectionMode.Active,
                AcceptInvalidCertificates = true
            };

            // Act
            var convertedSettings = originalSettings.ToFtpsSettings();

            // Assert
            convertedSettings.Host.Should().Be(originalSettings.Host);
            convertedSettings.Port.Should().Be(originalSettings.Port);
            convertedSettings.Username.Should().Be(originalSettings.Username);
            convertedSettings.Password.Should().Be(originalSettings.Password);
            convertedSettings.BasePath.Should().Be(originalSettings.BasePath);
            convertedSettings.SecurityMode.Should().Be(originalSettings.SecurityMode);
            convertedSettings.ConnectionMode.Should().Be(originalSettings.ConnectionMode);
            convertedSettings.AcceptInvalidCertificates.Should().Be(originalSettings.AcceptInvalidCertificates);
        }

        [Test]
        public void should_handle_validation_summary_correctly()
        {
            // Arrange
            var invalidSettings = new FtpsSettings
            {
                Host = "",
                Port = 0,
                Username = "",
                Password = "",
                BasePath = "invalid",
                MovieDirectory = ""
            };

            // Act
            var result = _validator.Validate(invalidSettings);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThan(4);
            result.Errors.Should().Contain(e => e.PropertyName == "Host");
            result.Errors.Should().Contain(e => e.PropertyName == "Port");
            result.Errors.Should().Contain(e => e.PropertyName == "Username");
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
            result.Errors.Should().Contain(e => e.PropertyName == "BasePath");
            result.Errors.Should().Contain(e => e.PropertyName == "MovieDirectory");
        }
    }
}