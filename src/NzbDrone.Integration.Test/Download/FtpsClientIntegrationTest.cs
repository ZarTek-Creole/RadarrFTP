using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NLog;

namespace NzbDrone.Integration.Test.Download
{
    [TestFixture]
    public class FtpsClientIntegrationTest
    {
        private FtpsSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new FtpsSettings
            {
                Host = "test.rebex.net",
                Port = 21,
                Username = "demo",
                Password = "password",
                BasePath = "/pub/example",
                MovieDirectory = "",
                SecurityMode = FtpsSecurityMode.None, // Test server uses plain FTP
                ConnectionMode = FtpsConnectionMode.Passive,
                ValidateCertificate = false
            };
        }

        [Test]
        public async Task should_connect_to_test_server()
        {
            // Arrange
            var proxy = new FtpsProxy(LogManager.GetCurrentClassLogger());

            // Act
            var result = await proxy.TestConnectionAsync(_settings);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task should_get_directory_listing()
        {
            // Arrange
            var proxy = new FtpsProxy(LogManager.GetCurrentClassLogger());

            // Act
            var items = await proxy.GetDirectoryListingAsync(_settings, "/pub/example");

            // Assert
            items.Should().NotBeEmpty();
        }

        [Test]
        public async Task should_check_file_exists()
        {
            // Arrange
            var proxy = new FtpsProxy(LogManager.GetCurrentClassLogger());

            // Act
            var exists = await proxy.FileExistsAsync(_settings, "/pub/example/readme.txt");

            // Assert
            // Note: This test depends on the specific test server content
            // The test.rebex.net server usually has some demo files
            exists.Should().BeTrue();
        }

        [Test]
        public void should_validate_settings_correctly()
        {
            // Test invalid settings
            var invalidSettings = new FtpsSettings();
            var invalidResult = invalidSettings.Validate();
            invalidResult.IsValid.Should().BeFalse();

            // Test valid settings
            var validSettings = new FtpsSettings
            {
                Host = "localhost",
                Port = 21,
                Username = "user",
                Password = "pass",
                BasePath = "/movies",
                MovieDirectory = "releases"
            };
            var validResult = validSettings.Validate();
            validResult.IsValid.Should().BeTrue();
        }
    }
}