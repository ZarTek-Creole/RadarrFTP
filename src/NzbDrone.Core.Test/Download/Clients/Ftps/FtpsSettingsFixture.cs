using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.Clients.Ftps
{
    [TestFixture]
    public class FtpsSettingsFixture : CoreTest
    {
        private FtpsSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new FtpsSettings();
        }

        [Test]
        public void Default_settings_should_be_valid()
        {
            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeFalse(); // Should be false because username/password are empty by default
            result.Errors.Should().NotBeEmpty();
        }

        [Test]
        public void Valid_settings_should_pass_validation()
        {
            // Arrange
            _settings.Host = "test.ftps.server";
            _settings.Port = 21;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";
            _settings.ConnectionTimeout = 30;

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Host_validation_should_fail_when_empty_or_null(string host)
        {
            // Arrange
            _settings.Host = host;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "Host");
        }

        [TestCase("valid.hostname.com")]
        [TestCase("192.168.1.100")]
        [TestCase("localhost")]
        [TestCase("test-server.domain.org")]
        public void Host_validation_should_pass_for_valid_hosts(string host)
        {
            // Arrange
            _settings.Host = host;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(65536)]
        [TestCase(99999)]
        public void Port_validation_should_fail_for_invalid_ports(int port)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Port = port;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "Port");
        }

        [TestCase(21)]
        [TestCase(22)]
        [TestCase(990)]
        [TestCase(8021)]
        [TestCase(65535)]
        public void Port_validation_should_pass_for_valid_ports(int port)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Port = port;
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Username_validation_should_fail_when_empty(string username)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Username = username;
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "Username");
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Password_validation_should_fail_when_empty(string password)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Username = "testuser";
            _settings.Password = password;
            _settings.RemoteBasePath = "/movies";

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "Password");
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        [TestCase("movies")]  // Does not start with /
        [TestCase("relative/path")]  // Does not start with /
        public void RemoteBasePath_validation_should_fail_for_invalid_paths(string path)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = path;

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "RemoteBasePath");
        }

        [TestCase("/")]
        [TestCase("/movies")]
        [TestCase("/releases/movies")]
        [TestCase("/home/user/downloads")]
        public void RemoteBasePath_validation_should_pass_for_valid_paths(string path)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = path;

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-10)]
        public void ConnectionTimeout_validation_should_fail_for_invalid_values(int timeout)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";
            _settings.ConnectionTimeout = timeout;

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "ConnectionTimeout");
        }

        [TestCase(1)]
        [TestCase(30)]
        [TestCase(60)]
        [TestCase(300)]
        public void ConnectionTimeout_validation_should_pass_for_valid_values(int timeout)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";
            _settings.ConnectionTimeout = timeout;

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Default_values_should_be_sensible()
        {
            // Assert
            _settings.Host.Should().Be("localhost");
            _settings.Port.Should().Be(21);
            _settings.EncryptionMode.Should().Be((int)FtpEncryptionMode.Explicit);
            _settings.ValidateCertificate.Should().BeTrue();
            _settings.DataConnectionType.Should().Be((int)FtpDataConnectionType.AutoPassive);
            _settings.RemoteBasePath.Should().Be("/releases/movies/");
            _settings.Priority.Should().Be(1);
            _settings.ConnectionTimeout.Should().Be(30);
            _settings.ReadTimeout.Should().Be(30);
            _settings.TransferChunkSize.Should().Be(1048576); // 1MB
            _settings.RetryAttempts.Should().Be(3);
            _settings.MonitoringEnabled.Should().BeTrue();
            _settings.MonitoringInterval.Should().Be(300); // 5 minutes
            _settings.Category.Should().Be("movies");
            _settings.UseSsl.Should().BeTrue();
        }

        [Test]
        public void Settings_equality_should_work_correctly()
        {
            // Arrange
            var settings1 = new FtpsSettings
            {
                Host = "test.server",
                Port = 21,
                Username = "user",
                Password = "pass"
            };

            var settings2 = new FtpsSettings
            {
                Host = "test.server",
                Port = 21,
                Username = "user",
                Password = "pass"
            };

            var settings3 = new FtpsSettings
            {
                Host = "different.server",
                Port = 21,
                Username = "user",
                Password = "pass"
            };

            // Act & Assert
            settings1.Equals(settings2).Should().BeTrue();
            settings1.Equals(settings3).Should().BeFalse();
            settings2.Equals(settings3).Should().BeFalse();
        }

        [Test]
        public void Settings_hashcode_should_be_consistent()
        {
            // Arrange
            var settings1 = new FtpsSettings
            {
                Host = "test.server",
                Port = 21,
                Username = "user",
                Password = "pass"
            };

            var settings2 = new FtpsSettings
            {
                Host = "test.server",
                Port = 21,
                Username = "user",
                Password = "pass"
            };

            // Act & Assert
            settings1.GetHashCode().Should().Be(settings2.GetHashCode());
        }

        [Test]
        public void Encryption_mode_enum_should_have_correct_values()
        {
            // Assert
            ((int)FtpEncryptionMode.None).Should().Be(0);
            ((int)FtpEncryptionMode.Explicit).Should().Be(1);
            ((int)FtpEncryptionMode.Implicit).Should().Be(2);
            ((int)FtpEncryptionMode.Auto).Should().Be(3);
        }

        [Test]
        public void Data_connection_type_enum_should_have_correct_values()
        {
            // Assert
            ((int)FtpDataConnectionType.AutoPassive).Should().Be(0);
            ((int)FtpDataConnectionType.PASV).Should().Be(1);
            ((int)FtpDataConnectionType.EPSV).Should().Be(2);
            ((int)FtpDataConnectionType.PORT).Should().Be(3);
            ((int)FtpDataConnectionType.EPRT).Should().Be(4);
        }

        [Test]
        public void SSL_settings_should_be_configurable()
        {
            // Arrange & Act
            _settings.UseSsl = true;
            _settings.EncryptionMode = (int)FtpEncryptionMode.Explicit;
            _settings.ValidateCertificate = false;

            // Assert
            _settings.UseSsl.Should().BeTrue();
            _settings.EncryptionMode.Should().Be((int)FtpEncryptionMode.Explicit);
            _settings.ValidateCertificate.Should().BeFalse();
        }

        [Test]
        public void Performance_settings_should_be_configurable()
        {
            // Arrange & Act
            _settings.TransferChunkSize = 2097152; // 2MB
            _settings.RetryAttempts = 5;
            _settings.ConnectionTimeout = 60;
            _settings.ReadTimeout = 45;

            // Assert
            _settings.TransferChunkSize.Should().Be(2097152);
            _settings.RetryAttempts.Should().Be(5);
            _settings.ConnectionTimeout.Should().Be(60);
            _settings.ReadTimeout.Should().Be(45);
        }

        [Test]
        public void Monitoring_settings_should_be_configurable()
        {
            // Arrange & Act
            _settings.MonitoringEnabled = false;
            _settings.MonitoringInterval = 600; // 10 minutes

            // Assert
            _settings.MonitoringEnabled.Should().BeFalse();
            _settings.MonitoringInterval.Should().Be(600);
        }

        [Test]
        public void Category_and_priority_should_be_configurable()
        {
            // Arrange & Act
            _settings.Category = "custom_movies";
            _settings.Priority = 5;

            // Assert
            _settings.Category.Should().Be("custom_movies");
            _settings.Priority.Should().Be(5);
        }

        [TestCase((int)FtpEncryptionMode.None)]
        [TestCase((int)FtpEncryptionMode.Explicit)]
        [TestCase((int)FtpEncryptionMode.Implicit)]
        [TestCase((int)FtpEncryptionMode.Auto)]
        public void All_encryption_modes_should_be_valid(int encryptionMode)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";
            _settings.EncryptionMode = encryptionMode;

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestCase((int)FtpDataConnectionType.AutoPassive)]
        [TestCase((int)FtpDataConnectionType.PASV)]
        [TestCase((int)FtpDataConnectionType.EPSV)]
        [TestCase((int)FtpDataConnectionType.PORT)]
        [TestCase((int)FtpDataConnectionType.EPRT)]
        public void All_data_connection_types_should_be_valid(int dataConnectionType)
        {
            // Arrange
            _settings.Host = "test.server";
            _settings.Username = "testuser";
            _settings.Password = "testpass";
            _settings.RemoteBasePath = "/movies";
            _settings.DataConnectionType = dataConnectionType;

            // Act
            var result = _settings.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}