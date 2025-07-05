using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.Indexers.Ftps;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Indexers.Ftps
{
    [TestFixture]
    public class FtpsIndexerSettingsFixture : TestBase
    {
        private FtpsIndexerSettings _settings;
        private FtpsIndexerSettingsValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new FtpsIndexerSettingsValidator();
            _settings = new FtpsIndexerSettings();
        }

        [Test]
        public void should_be_valid_when_all_required_properties_are_set()
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.MovieDirectory = "movies";

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

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(65536)]
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
            _settings.MovieDirectory = "movies";

            // Act
            var result = _validator.Validate(_settings);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void should_use_default_values_correctly()
        {
            // Arrange & Act
            var settings = new FtpsIndexerSettings();

            // Assert
            settings.Port.Should().Be(21);
            settings.SecurityMode.Should().Be(FtpsSecurityMode.Explicit);
            settings.ConnectionMode.Should().Be(FtpsConnectionMode.Passive);
            settings.AcceptInvalidCertificates.Should().BeFalse();
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
            var movieDirectory = "releases";
            var securityMode = FtpsSecurityMode.Implicit;
            var connectionMode = FtpsConnectionMode.Active;
            var acceptInvalidCerts = true;

            // Act
            _settings.Host = host;
            _settings.Port = port;
            _settings.Username = username;
            _settings.Password = password;
            _settings.MovieDirectory = movieDirectory;
            _settings.SecurityMode = securityMode;
            _settings.ConnectionMode = connectionMode;
            _settings.AcceptInvalidCertificates = acceptInvalidCerts;

            // Assert
            _settings.Host.Should().Be(host);
            _settings.Port.Should().Be(port);
            _settings.Username.Should().Be(username);
            _settings.Password.Should().Be(password);
            _settings.MovieDirectory.Should().Be(movieDirectory);
            _settings.SecurityMode.Should().Be(securityMode);
            _settings.ConnectionMode.Should().Be(connectionMode);
            _settings.AcceptInvalidCertificates.Should().Be(acceptInvalidCerts);
        }

        [Test]
        public void should_convert_to_ftps_settings()
        {
            // Arrange
            var indexerSettings = new FtpsIndexerSettings
            {
                Host = "test.com",
                Port = 990,
                Username = "user",
                Password = "pass",
                MovieDirectory = "movies",
                SecurityMode = FtpsSecurityMode.Implicit,
                ConnectionMode = FtpsConnectionMode.Active,
                AcceptInvalidCertificates = true
            };

            // Act
            var ftpsSettings = indexerSettings.ToFtpsSettings();

            // Assert
            ftpsSettings.Host.Should().Be(indexerSettings.Host);
            ftpsSettings.Port.Should().Be(indexerSettings.Port);
            ftpsSettings.Username.Should().Be(indexerSettings.Username);
            ftpsSettings.Password.Should().Be(indexerSettings.Password);
            ftpsSettings.SecurityMode.Should().Be(indexerSettings.SecurityMode);
            ftpsSettings.ConnectionMode.Should().Be(indexerSettings.ConnectionMode);
            ftpsSettings.AcceptInvalidCertificates.Should().Be(indexerSettings.AcceptInvalidCertificates);
            ftpsSettings.BasePath.Should().Be("/");
            ftpsSettings.MovieDirectory.Should().Be(indexerSettings.MovieDirectory);
        }

        [Test]
        public void should_handle_null_and_empty_strings()
        {
            // Arrange
            var settings = new FtpsIndexerSettings
            {
                Host = null,
                Username = "",
                Password = "   ",
                MovieDirectory = null
            };

            // Act
            var result = _validator.Validate(settings);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThan(3);
        }

        [Test]
        public void should_validate_host_format()
        {
            // Arrange
            _settings.Host = "test.ftps.com";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.MovieDirectory = "movies";

            // Act
            var result = _validator.Validate(_settings);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestCase("localhost")]
        [TestCase("192.168.1.1")]
        [TestCase("test.server.com")]
        [TestCase("ftp.example.org")]
        public void should_accept_valid_hostnames(string host)
        {
            // Arrange
            _settings.Host = host;
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.MovieDirectory = "movies";

            // Act
            var result = _validator.Validate(_settings);

            // Assert
            result.IsValid.Should().BeTrue();
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
        public void should_handle_validation_summary_correctly()
        {
            // Arrange
            var invalidSettings = new FtpsIndexerSettings
            {
                Host = "",
                Port = 0,
                Username = "",
                Password = "",
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
            result.Errors.Should().Contain(e => e.PropertyName == "MovieDirectory");
        }

        [Test]
        public void should_have_appropriate_default_settings()
        {
            // Act
            var settings = new FtpsIndexerSettings();

            // Assert
            settings.Port.Should().Be(21, "Standard FTP port");
            settings.SecurityMode.Should().Be(FtpsSecurityMode.Explicit, "Explicit is more common");
            settings.ConnectionMode.Should().Be(FtpsConnectionMode.Passive, "Passive is firewall-friendly");
            settings.AcceptInvalidCertificates.Should().BeFalse("Security best practice");
            settings.MovieDirectory.Should().Be("movies", "Common directory name");
        }
    }
}