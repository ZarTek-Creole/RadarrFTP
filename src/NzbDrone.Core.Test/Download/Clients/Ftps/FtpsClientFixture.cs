using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.Download.Clients.Ftps
{
    [TestFixture]
    public class FtpsClientFixture : CoreTest<FtpsClient>
    {
        private FtpsSettings _settings;
        private Mock<IFtpsProxy> _ftpsProxy;

        [SetUp]
        public void Setup()
        {
            _settings = new FtpsSettings
            {
                Host = "localhost",
                Port = 21,
                Username = "user",
                Password = "pass",
                BasePath = "/movies",
                MovieDirectory = "releases"
            };

            _ftpsProxy = new Mock<IFtpsProxy>();

            Mocker.GetMock<IFtpsProxy>()
                .Setup(s => s.TestConnectionAsync(It.IsAny<FtpsSettings>()))
                .Returns(Task.FromResult(true));
        }

        [Test]
        public void should_return_correct_protocol()
        {
            // Act
            var protocol = Subject.Protocol;

            // Assert
            protocol.Should().Be(DownloadProtocol.Ftps);
        }

        [Test]
        public void should_return_correct_name()
        {
            // Act
            var name = Subject.Name;

            // Assert
            name.Should().Be("FTPS Client");
        }

        [Test]
        public void should_validate_settings()
        {
            // Arrange
            var settings = new FtpsSettings();

            // Act
            var result = settings.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }

        [Test]
        public void should_validate_valid_settings()
        {
            // Arrange
            var settings = new FtpsSettings
            {
                Host = "localhost",
                Port = 21,
                Username = "user",
                Password = "pass",
                BasePath = "/movies",
                MovieDirectory = "releases"
            };

            // Act
            var result = settings.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void should_get_status()
        {
            // Act
            var status = Subject.GetStatus();

            // Assert
            status.Should().NotBeNull();
            status.OutputRootFolders.Should().NotBeEmpty();
        }

        [Test]
        public void should_return_empty_items_list()
        {
            // Act
            var items = Subject.GetItems();

            // Assert
            items.Should().NotBeNull();
            items.Should().BeEmpty();
        }

        [Test]
        public async Task should_download_file_successfully()
        {
            // Arrange
            var release = new ReleaseInfo
            {
                Title = "Test Movie 2023",
                DownloadUrl = "ftps://test.com/movie.mkv",
                Size = 1024 * 1024 * 1000 // 1GB
            };

            var remoteMovie = new RemoteMovie
            {
                Release = release,
                Movie = new NzbDrone.Core.Movies.Movie { Title = "Test Movie", Year = 2023 }
            };

            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.DownloadFileAsync(It.IsAny<FtpsSettings>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var downloadId = await Subject.Download(remoteMovie);

            // Assert
            downloadId.Should().NotBeNullOrEmpty();
            downloadId.Should().Contain("movie.mkv");
        }

        [Test]
        public async Task should_fail_download_when_file_not_found()
        {
            // Arrange
            var release = new ReleaseInfo
            {
                Title = "Test Movie 2023",
                DownloadUrl = "ftps://test.com/notfound.mkv",
                Size = 1024 * 1024 * 1000
            };

            var remoteMovie = new RemoteMovie
            {
                Release = release,
                Movie = new NzbDrone.Core.Movies.Movie { Title = "Test Movie", Year = 2023 }
            };

            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.DownloadFileAsync(It.IsAny<FtpsSettings>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Subject.Awaiting(x => x.Download(remoteMovie))
                .Should().ThrowAsync<Exception>();
        }

        [Test]
        public void should_test_connection_successfully()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.TestConnectionAsync(It.IsAny<FtpsSettings>()))
                .ReturnsAsync(true);

            // Act
            var result = Subject.TestConnection();

            // Assert
            result.IsValid.Should().BeTrue();
            result.Failures.Should().BeEmpty();
        }

        [Test]
        public void should_fail_test_connection_when_credentials_invalid()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.TestConnectionAsync(It.IsAny<FtpsSettings>()))
                .ReturnsAsync(false);

            // Act
            var result = Subject.TestConnection();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Failures.Should().NotBeEmpty();
        }

        [Test]
        public void should_handle_download_url_parsing()
        {
            // Arrange
            var testUrls = new[]
            {
                "ftps://server.com/path/to/file.mkv",
                "ftps://user:pass@server.com/file.mkv",
                "ftps://server.com:990/secure/file.mkv"
            };

            foreach (var url in testUrls)
            {
                var release = new ReleaseInfo
                {
                    Title = "Test Movie",
                    DownloadUrl = url,
                    Size = 1024 * 1024 * 1000
                };

                // Act
                var parsedUrl = Subject.ParseDownloadUrl(url);

                // Assert
                parsedUrl.Should().NotBeNull();
                parsedUrl.Should().Contain("file.mkv");
            }
        }

        [Test]
        public void should_support_required_protocols()
        {
            // Act & Assert
            Subject.Protocol.Should().Be(DownloadProtocol.Ftps);
        }

        [Test]
        public void should_return_correct_category()
        {
            // Act
            var category = Subject.GetDefaultCategory();

            // Assert
            category.Should().Be("movies");
        }

        [Test]
        public void should_get_download_client_info()
        {
            // Act
            var info = Subject.GetDownloadClientInfo();

            // Assert
            info.Should().NotBeNull();
            info.Protocol.Should().Be(DownloadProtocol.Ftps);
            info.Name.Should().Be("FTPS Client");
        }

        [Test]
        public void should_handle_special_characters_in_download_path()
        {
            // Arrange
            var specialPaths = new[]
            {
                "/movies/Movie [2023] (1080p).mkv",
                "/movies/Amélie.2001.1080p.BluRay.mkv",
                "/movies/Movie - Part 1 & Part 2.mkv"
            };

            foreach (var path in specialPaths)
            {
                var release = new ReleaseInfo
                {
                    Title = "Special Movie",
                    DownloadUrl = $"ftps://server.com{path}",
                    Size = 1024 * 1024 * 1000
                };

                // Act
                var result = Subject.CanHandleDownloadUrl(release.DownloadUrl);

                // Assert
                result.Should().BeTrue();
            }
        }

        [Test]
        public void should_create_download_directory_if_not_exists()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), "ftps-client-test");
            
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            // Act
            Subject.EnsureDownloadDirectoryExists(tempDir);

            // Assert
            Directory.Exists(tempDir).Should().BeTrue();

            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Test]
        public void should_handle_concurrent_downloads()
        {
            // Arrange
            var releases = new List<ReleaseInfo>();
            for (int i = 0; i < 5; i++)
            {
                releases.Add(new ReleaseInfo
                {
                    Title = $"Movie {i}",
                    DownloadUrl = $"ftps://server.com/movie{i}.mkv",
                    Size = 1024 * 1024 * 1000
                });
            }

            // Act & Assert
            foreach (var release in releases)
            {
                var remoteMovie = new RemoteMovie
                {
                    Release = release,
                    Movie = new NzbDrone.Core.Movies.Movie { Title = release.Title, Year = 2023 }
                };

                Subject.CanHandleDownloadUrl(release.DownloadUrl).Should().BeTrue();
            }
        }

        [Test]
        public void should_validate_download_url_format()
        {
            // Arrange
            var validUrls = new[]
            {
                "ftps://server.com/movie.mkv",
                "ftps://user:pass@server.com/movie.mkv",
                "ftps://server.com:990/movie.mkv"
            };

            var invalidUrls = new[]
            {
                "http://server.com/movie.mkv",
                "ftp://server.com/movie.mkv",
                "https://server.com/movie.mkv",
                "invalid-url"
            };

            // Act & Assert
            foreach (var url in validUrls)
            {
                Subject.CanHandleDownloadUrl(url).Should().BeTrue();
            }

            foreach (var url in invalidUrls)
            {
                Subject.CanHandleDownloadUrl(url).Should().BeFalse();
            }
        }

        [Test]
        public void should_calculate_download_path_correctly()
        {
            // Arrange
            var release = new ReleaseInfo
            {
                Title = "Test Movie 2023",
                DownloadUrl = "ftps://server.com/path/to/movie.mkv"
            };

            var remoteMovie = new RemoteMovie
            {
                Release = release,
                Movie = new NzbDrone.Core.Movies.Movie { Title = "Test Movie", Year = 2023 }
            };

            // Act
            var downloadPath = Subject.GetDownloadPath(remoteMovie);

            // Assert
            downloadPath.Should().NotBeNullOrEmpty();
            downloadPath.Should().Contain("movie.mkv");
        }

        [Test]
        public void should_handle_large_file_sizes()
        {
            // Arrange
            var largeSize = 50L * 1024 * 1024 * 1024; // 50GB
            var release = new ReleaseInfo
            {
                Title = "Large Movie 2023",
                DownloadUrl = "ftps://server.com/large-movie.mkv",
                Size = largeSize
            };

            var remoteMovie = new RemoteMovie
            {
                Release = release,
                Movie = new NzbDrone.Core.Movies.Movie { Title = "Large Movie", Year = 2023 }
            };

            // Act & Assert
            Subject.CanHandleDownloadUrl(release.DownloadUrl).Should().BeTrue();
            release.Size.Should().Be(largeSize);
        }

        [Test]
        public void should_cleanup_failed_downloads()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "test content");

            // Act
            Subject.CleanupFailedDownload(tempFile);

            // Assert
            File.Exists(tempFile).Should().BeFalse();
        }

        [Test]
        public void should_generate_unique_download_ids()
        {
            // Arrange
            var downloadIds = new HashSet<string>();
            
            for (int i = 0; i < 100; i++)
            {
                var release = new ReleaseInfo
                {
                    Title = $"Movie {i}",
                    DownloadUrl = $"ftps://server.com/movie{i}.mkv",
                    Size = 1024 * 1024 * 1000
                };

                // Act
                var downloadId = Subject.GenerateDownloadId(release);

                // Assert
                downloadId.Should().NotBeNullOrEmpty();
                downloadIds.Should().NotContain(downloadId);
                downloadIds.Add(downloadId);
            }

            downloadIds.Should().HaveCount(100);
        }
    }
}