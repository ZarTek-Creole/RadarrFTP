using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using FluentFTP;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.Clients.Ftps
{
    [TestFixture]
    public class FtpsProxyFixture : TestBase
    {
        private FtpsProxy _subject;
        private FtpsSettings _settings;
        private Mock<IAsyncFtpClient> _mockFtpClient;

        [SetUp]
        public void Setup()
        {
            _mockFtpClient = new Mock<IAsyncFtpClient>();
            _subject = new FtpsProxy();
            
            _settings = new FtpsSettings
            {
                Host = "test.ftps.com",
                Port = 21,
                Username = "testuser",
                Password = "testpass",
                BasePath = "/",
                SecurityMode = FtpsSecurityMode.Explicit,
                ConnectionMode = FtpsConnectionMode.Passive,
                AcceptInvalidCertificates = true
            };
        }

        [Test]
        public async Task TestConnectionAsync_ValidSettings_ShouldReturnTrue()
        {
            // Arrange
            _mockFtpClient.Setup(x => x.AutoConnectAsync(default))
                .Returns(Task.CompletedTask);
            _mockFtpClient.Setup(x => x.GetWorkingDirectoryAsync(default))
                .ReturnsAsync("/");

            // Act
            var result = await _subject.TestConnectionAsync(_settings);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task TestConnectionAsync_InvalidCredentials_ShouldReturnFalse()
        {
            // Arrange
            _mockFtpClient.Setup(x => x.AutoConnectAsync(default))
                .ThrowsAsync(new FtpAuthenticationException("Invalid credentials"));

            // Act
            var result = await _subject.TestConnectionAsync(_settings);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task TestConnectionAsync_NetworkError_ShouldReturnFalse()
        {
            // Arrange
            _mockFtpClient.Setup(x => x.AutoConnectAsync(default))
                .ThrowsAsync(new FtpException("Network unreachable"));

            // Act
            var result = await _subject.TestConnectionAsync(_settings);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task GetDirectoryListingAsync_ValidPath_ShouldReturnItems()
        {
            // Arrange
            var ftpItems = new[]
            {
                new FtpListItem { Name = "Movie.2023.1080p.BluRay", Type = FtpObjectType.Directory },
                new FtpListItem { Name = "Movie.2023.720p.WEB", Type = FtpObjectType.Directory },
                new FtpListItem { Name = "readme.txt", Type = FtpObjectType.File }
            };

            _mockFtpClient.Setup(x => x.GetListingAsync("/movies", default))
                .ReturnsAsync(ftpItems);

            // Act
            var result = await _subject.GetDirectoryListingAsync(_settings, "/movies");

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(x => x.Name == "Movie.2023.1080p.BluRay" && x.IsDirectory);
            result.Should().Contain(x => x.Name == "Movie.2023.720p.WEB" && x.IsDirectory);
            result.Should().Contain(x => x.Name == "readme.txt" && !x.IsDirectory);
        }

        [Test]
        public async Task GetDirectoryListingAsync_EmptyDirectory_ShouldReturnEmptyList()
        {
            // Arrange
            _mockFtpClient.Setup(x => x.GetListingAsync("/empty", default))
                .ReturnsAsync(new FtpListItem[0]);

            // Act
            var result = await _subject.GetDirectoryListingAsync(_settings, "/empty");

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetDirectoryListingAsync_InvalidPath_ShouldThrowException()
        {
            // Arrange
            _mockFtpClient.Setup(x => x.GetListingAsync("/invalid", default))
                .ThrowsAsync(new FtpCommandException("550 Directory not found"));

            // Act & Assert
            await _subject.Awaiting(x => x.GetDirectoryListingAsync(_settings, "/invalid"))
                .Should().ThrowAsync<FtpCommandException>();
        }

        [Test]
        public async Task DownloadFileAsync_ValidFile_ShouldDownloadSuccessfully()
        {
            // Arrange
            var localPath = Path.GetTempFileName();
            var remotePath = "/movies/test.mkv";

            _mockFtpClient.Setup(x => x.DownloadFileAsync(localPath, remotePath, 
                FtpLocalExists.Overwrite, FtpVerify.Retry, null, default))
                .ReturnsAsync(FtpStatus.Success);

            try
            {
                // Act
                var result = await _subject.DownloadFileAsync(_settings, remotePath, localPath);

                // Assert
                result.Should().BeTrue();
            }
            finally
            {
                File.Delete(localPath);
            }
        }

        [Test]
        public async Task DownloadFileAsync_FileNotFound_ShouldReturnFalse()
        {
            // Arrange
            var localPath = Path.GetTempFileName();
            var remotePath = "/movies/notfound.mkv";

            _mockFtpClient.Setup(x => x.DownloadFileAsync(localPath, remotePath, 
                FtpLocalExists.Overwrite, FtpVerify.Retry, null, default))
                .ThrowsAsync(new FtpCommandException("550 File not found"));

            try
            {
                // Act
                var result = await _subject.DownloadFileAsync(_settings, remotePath, localPath);

                // Assert
                result.Should().BeFalse();
            }
            finally
            {
                File.Delete(localPath);
            }
        }

        [Test]
        public async Task GetFileSizeAsync_ValidFile_ShouldReturnSize()
        {
            // Arrange
            var remotePath = "/movies/test.mkv";
            var expectedSize = 1024 * 1024 * 500; // 500MB

            _mockFtpClient.Setup(x => x.GetFileSizeAsync(remotePath, -1, default))
                .ReturnsAsync(expectedSize);

            // Act
            var result = await _subject.GetFileSizeAsync(_settings, remotePath);

            // Assert
            result.Should().Be(expectedSize);
        }

        [Test]
        public async Task GetFileSizeAsync_FileNotFound_ShouldReturnZero()
        {
            // Arrange
            var remotePath = "/movies/notfound.mkv";

            _mockFtpClient.Setup(x => x.GetFileSizeAsync(remotePath, -1, default))
                .ThrowsAsync(new FtpCommandException("550 File not found"));

            // Act
            var result = await _subject.GetFileSizeAsync(_settings, remotePath);

            // Assert
            result.Should().Be(0);
        }

        [Test]
        public void GetFtpEncryptionMode_ExplicitMode_ShouldReturnExplicit()
        {
            // Arrange
            _settings.SecurityMode = FtpsSecurityMode.Explicit;

            // Act
            var result = _subject.GetFtpEncryptionMode(_settings);

            // Assert
            result.Should().Be(FtpEncryptionMode.Explicit);
        }

        [Test]
        public void GetFtpEncryptionMode_ImplicitMode_ShouldReturnImplicit()
        {
            // Arrange
            _settings.SecurityMode = FtpsSecurityMode.Implicit;

            // Act
            var result = _subject.GetFtpEncryptionMode(_settings);

            // Assert
            result.Should().Be(FtpEncryptionMode.Implicit);
        }

        [Test]
        public void GetFtpEncryptionMode_NoneMode_ShouldReturnNone()
        {
            // Arrange
            _settings.SecurityMode = FtpsSecurityMode.None;

            // Act
            var result = _subject.GetFtpEncryptionMode(_settings);

            // Assert
            result.Should().Be(FtpEncryptionMode.None);
        }

        [Test]
        public void GetFtpDataConnectionType_PassiveMode_ShouldReturnPassive()
        {
            // Arrange
            _settings.ConnectionMode = FtpsConnectionMode.Passive;

            // Act
            var result = _subject.GetFtpDataConnectionType(_settings);

            // Assert
            result.Should().Be(FtpDataConnectionType.PASV);
        }

        [Test]
        public void GetFtpDataConnectionType_ActiveMode_ShouldReturnActive()
        {
            // Arrange
            _settings.ConnectionMode = FtpsConnectionMode.Active;

            // Act
            var result = _subject.GetFtpDataConnectionType(_settings);

            // Assert
            result.Should().Be(FtpDataConnectionType.PORT);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task TestConnectionAsync_InvalidHost_ShouldReturnFalse(string host)
        {
            // Arrange
            _settings.Host = host;

            // Act
            var result = await _subject.TestConnectionAsync(_settings);

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(65536)]
        public async Task TestConnectionAsync_InvalidPort_ShouldReturnFalse(int port)
        {
            // Arrange
            _settings.Port = port;

            // Act
            var result = await _subject.TestConnectionAsync(_settings);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Dispose_ShouldDisposeClientProperly()
        {
            // Arrange
            _mockFtpClient.Setup(x => x.Dispose());

            // Act
            await _subject.DisposeAsync();

            // Assert
            _mockFtpClient.Verify(x => x.Dispose(), Times.AtLeastOnce);
        }
    }
}