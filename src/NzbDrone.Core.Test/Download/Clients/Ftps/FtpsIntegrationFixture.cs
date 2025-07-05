using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Ftps;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.Clients.Ftps
{
    [TestFixture]
    public class FtpsIntegrationFixture : CoreTest
    {
        private FtpsClient _ftpsClient;
        private FtpsIndexer _ftpsIndexer;
        private Mock<IFtpsProxy> _ftpsProxy;
        private FtpsSettings _clientSettings;
        private FtpsIndexerSettings _indexerSettings;

        [SetUp]
        public void Setup()
        {
            _ftpsProxy = new Mock<IFtpsProxy>();
            
            _clientSettings = new FtpsSettings
            {
                Host = "test.ftps.com",
                Port = 21,
                Username = "testuser",
                Password = "testpass",
                BasePath = "/movies",
                MovieDirectory = "releases",
                SecurityMode = FtpsSecurityMode.Explicit,
                ConnectionMode = FtpsConnectionMode.Passive,
                AcceptInvalidCertificates = true
            };

            _indexerSettings = new FtpsIndexerSettings
            {
                Host = "test.ftps.com",
                Port = 21,
                Username = "testuser",
                Password = "testpass",
                MovieDirectory = "movies",
                SecurityMode = FtpsSecurityMode.Explicit,
                ConnectionMode = FtpsConnectionMode.Passive,
                AcceptInvalidCertificates = true
            };

            // Setup client
            _ftpsClient = new FtpsClient();
            var clientMocker = new AutoMocker();
            clientMocker.SetInstance(_ftpsProxy.Object);
            _ftpsClient = clientMocker.CreateInstance<FtpsClient>();
            _ftpsClient.Definition = new DownloadClientDefinition { Settings = _clientSettings };

            // Setup indexer
            _ftpsIndexer = new FtpsIndexer(_ftpsProxy.Object);
            var indexerMocker = new AutoMocker();
            indexerMocker.SetInstance(_ftpsProxy.Object);
            _ftpsIndexer = indexerMocker.CreateInstance<FtpsIndexer>();
            _ftpsIndexer.Definition = new IndexerDefinition { Settings = _indexerSettings };
        }

        [Test]
        public async Task FullWorkflow_IndexerDiscoversReleasesAndClientDownloads_ShouldWork()
        {
            // Arrange - Mock directory structure
            var rootDirectoryItems = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "The.Matrix.1999.1080p.BluRay.x264-GROUP",
                    IsDirectory = true,
                    Size = 0,
                    FullPath = "/movies/The.Matrix.1999.1080p.BluRay.x264-GROUP"
                },
                new FtpsDirectoryItem
                {
                    Name = "Avatar.2009.720p.WEB.x264-RELEASE",
                    IsDirectory = true,
                    Size = 0,
                    FullPath = "/movies/Avatar.2009.720p.WEB.x264-RELEASE"
                }
            };

            var matrixFiles = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "The.Matrix.1999.1080p.BluRay.x264-GROUP.mkv",
                    IsDirectory = false,
                    Size = 8589934592, // 8GB
                    FullPath = "/movies/The.Matrix.1999.1080p.BluRay.x264-GROUP/The.Matrix.1999.1080p.BluRay.x264-GROUP.mkv"
                },
                new FtpsDirectoryItem
                {
                    Name = "sample.mkv",
                    IsDirectory = false,
                    Size = 104857600, // 100MB
                    FullPath = "/movies/The.Matrix.1999.1080p.BluRay.x264-GROUP/sample.mkv"
                }
            };

            var avatarFiles = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "Avatar.2009.720p.WEB.x264-RELEASE.rar",
                    IsDirectory = false,
                    Size = 4294967296, // 4GB
                    FullPath = "/movies/Avatar.2009.720p.WEB.x264-RELEASE/Avatar.2009.720p.WEB.x264-RELEASE.rar"
                },
                new FtpsDirectoryItem
                {
                    Name = "Avatar.2009.720p.WEB.x264-RELEASE.r00",
                    IsDirectory = false,
                    Size = 4294967296, // 4GB
                    FullPath = "/movies/Avatar.2009.720p.WEB.x264-RELEASE/Avatar.2009.720p.WEB.x264-RELEASE.r00"
                }
            };

            // Mock FTPS proxy responses
            _ftpsProxy.Setup(x => x.TestConnectionAsync(It.IsAny<FtpsSettings>()))
                .ReturnsAsync(true);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"))
                .ReturnsAsync(rootDirectoryItems);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), 
                "/movies/The.Matrix.1999.1080p.BluRay.x264-GROUP"))
                .ReturnsAsync(matrixFiles);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), 
                "/movies/Avatar.2009.720p.WEB.x264-RELEASE"))
                .ReturnsAsync(avatarFiles);

            _ftpsProxy.Setup(x => x.DownloadFileAsync(It.IsAny<FtpsSettings>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act - Indexer discovers releases
            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());
            var discoveredReleases = await _ftpsIndexer.FetchIndexerFeed(indexerRequest);

            // Assert - Indexer found releases
            discoveredReleases.Should().NotBeEmpty();
            discoveredReleases.Should().HaveCount(2);

            var matrixRelease = discoveredReleases.First(r => r.Title.Contains("Matrix"));
            var avatarRelease = discoveredReleases.First(r => r.Title.Contains("Avatar"));

            matrixRelease.Should().NotBeNull();
            matrixRelease.Size.Should().Be(8589934592); // Should select the video file
            matrixRelease.DownloadUrl.Should().Contain("The.Matrix.1999.1080p.BluRay.x264-GROUP.mkv");

            avatarRelease.Should().NotBeNull();
            avatarRelease.Size.Should().Be(4294967296); // Should select the RAR file
            avatarRelease.DownloadUrl.Should().Contain("Avatar.2009.720p.WEB.x264-RELEASE.rar");

            // Act - Client downloads one of the releases
            var downloadItem = new RemoteMovie
            {
                Release = matrixRelease,
                Movie = new NzbDrone.Core.Movies.Movie { Title = "The Matrix", Year = 1999 }
            };

            string downloadId = await _ftpsClient.Download(downloadItem);

            // Assert - Download initiated
            downloadId.Should().NotBeNullOrEmpty();
            downloadId.Should().Contain("The.Matrix.1999.1080p.BluRay.x264-GROUP.mkv");

            // Verify FTPS proxy was called correctly
            _ftpsProxy.Verify(x => x.TestConnectionAsync(It.IsAny<FtpsSettings>()), Times.AtLeastOnce);
            _ftpsProxy.Verify(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"), Times.Once);
            _ftpsProxy.Verify(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), 
                "/movies/The.Matrix.1999.1080p.BluRay.x264-GROUP"), Times.Once);
            _ftpsProxy.Verify(x => x.DownloadFileAsync(It.IsAny<FtpsSettings>(), It.IsAny<string>(), It.IsAny<string>()), 
                Times.Once);
        }

        [Test]
        public async Task IndexerAndClientSettings_ShouldBeCompatible()
        {
            // Arrange
            _ftpsProxy.Setup(x => x.TestConnectionAsync(It.IsAny<FtpsSettings>()))
                .ReturnsAsync(true);

            // Act
            var indexerConnection = _ftpsIndexer.TestConnection();
            var clientStatus = _ftpsClient.GetStatus();

            // Assert
            indexerConnection.IsValid.Should().BeTrue();
            clientStatus.Should().NotBeNull();
            clientStatus.OutputRootFolders.Should().NotBeEmpty();
        }

        [Test]
        public void ClientAndIndexer_ShouldUseSameProtocol()
        {
            // Act & Assert
            _ftpsClient.Protocol.Should().Be(DownloadProtocol.Ftps);
            _ftpsIndexer.Protocol.Should().Be(DownloadProtocol.Ftps);
        }

        [Test]
        public void ClientAndIndexer_ShouldHaveCorrectNames()
        {
            // Act & Assert
            _ftpsClient.Name.Should().Be("FTPS Client");
            _ftpsIndexer.Name.Should().Be("FTPS Indexer");
        }

        [Test]
        public async Task ErrorHandling_IndexerConnectionFails_ShouldHandleGracefully()
        {
            // Arrange
            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Connection failed"));

            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());

            // Act & Assert
            await _ftpsIndexer.Awaiting(x => x.FetchIndexerFeed(indexerRequest))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Connection failed");
        }

        [Test]
        public async Task ErrorHandling_ClientDownloadFails_ShouldHandleGracefully()
        {
            // Arrange
            _ftpsProxy.Setup(x => x.DownloadFileAsync(It.IsAny<FtpsSettings>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var release = new ReleaseInfo
            {
                Title = "Test Movie",
                DownloadUrl = "ftps://test.com/movie.mkv",
                Size = 1024 * 1024 * 1000 // 1GB
            };

            var downloadItem = new RemoteMovie
            {
                Release = release,
                Movie = new NzbDrone.Core.Movies.Movie { Title = "Test Movie", Year = 2023 }
            };

            // Act & Assert
            await _ftpsClient.Awaiting(x => x.Download(downloadItem))
                .Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task FileSelection_ShouldPrioritizeCorrectly()
        {
            // Arrange - Directory with mixed files
            var mixedFiles = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem { Name = "movie.nfo", IsDirectory = false, Size = 1024 },
                new FtpsDirectoryItem { Name = "movie.srt", IsDirectory = false, Size = 2048 },
                new FtpsDirectoryItem { Name = "movie.rar", IsDirectory = false, Size = 4294967296 }, // 4GB
                new FtpsDirectoryItem { Name = "movie.mkv", IsDirectory = false, Size = 8589934592 }, // 8GB
                new FtpsDirectoryItem { Name = "movie.r00", IsDirectory = false, Size = 4294967296 },
                new FtpsDirectoryItem { Name = "movie.jpg", IsDirectory = false, Size = 512000 }
            };

            var rootDir = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "Mixed.Movie.2023.1080p.BluRay.x264-GROUP",
                    IsDirectory = true,
                    Size = 0,
                    FullPath = "/movies/Mixed.Movie.2023.1080p.BluRay.x264-GROUP"
                }
            };

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"))
                .ReturnsAsync(rootDir);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), 
                "/movies/Mixed.Movie.2023.1080p.BluRay.x264-GROUP"))
                .ReturnsAsync(mixedFiles);

            // Act
            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());
            var releases = await _ftpsIndexer.FetchIndexerFeed(indexerRequest);

            // Assert - Should select video file over archive
            releases.Should().HaveCount(1);
            var release = releases.First();
            release.DownloadUrl.Should().Contain("movie.mkv");
            release.Size.Should().Be(8589934592); // Video file size
        }

        [Test]
        public async Task FileSelection_OnlyArchives_ShouldSelectMainArchive()
        {
            // Arrange - Directory with only archive files
            var archiveFiles = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem { Name = "movie.rar", IsDirectory = false, Size = 4294967296 }, // Main archive
                new FtpsDirectoryItem { Name = "movie.r00", IsDirectory = false, Size = 4294967296 },
                new FtpsDirectoryItem { Name = "movie.r01", IsDirectory = false, Size = 4294967296 },
                new FtpsDirectoryItem { Name = "movie.nfo", IsDirectory = false, Size = 1024 },
                new FtpsDirectoryItem { Name = "movie.srt", IsDirectory = false, Size = 2048 }
            };

            var rootDir = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "Archive.Movie.2023.1080p.BluRay.x264-GROUP",
                    IsDirectory = true,
                    Size = 0,
                    FullPath = "/movies/Archive.Movie.2023.1080p.BluRay.x264-GROUP"
                }
            };

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"))
                .ReturnsAsync(rootDir);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), 
                "/movies/Archive.Movie.2023.1080p.BluRay.x264-GROUP"))
                .ReturnsAsync(archiveFiles);

            // Act
            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());
            var releases = await _ftpsIndexer.FetchIndexerFeed(indexerRequest);

            // Assert - Should select main RAR file
            releases.Should().HaveCount(1);
            var release = releases.First();
            release.DownloadUrl.Should().Contain("movie.rar");
            release.Size.Should().Be(4294967296); // Main archive size
        }

        [Test]
        public void Settings_ShouldConvertCorrectly()
        {
            // Act
            var clientFtpsSettings = _clientSettings.ToFtpsSettings();
            var indexerFtpsSettings = _indexerSettings.ToFtpsSettings();

            // Assert
            clientFtpsSettings.Host.Should().Be(_clientSettings.Host);
            clientFtpsSettings.Port.Should().Be(_clientSettings.Port);
            clientFtpsSettings.Username.Should().Be(_clientSettings.Username);
            clientFtpsSettings.Password.Should().Be(_clientSettings.Password);

            indexerFtpsSettings.Host.Should().Be(_indexerSettings.Host);
            indexerFtpsSettings.Port.Should().Be(_indexerSettings.Port);
            indexerFtpsSettings.Username.Should().Be(_indexerSettings.Username);
            indexerFtpsSettings.Password.Should().Be(_indexerSettings.Password);
        }

        [Test]
        public void DownloadProtocol_ShouldBeRegistered()
        {
            // Assert
            ((int)DownloadProtocol.Ftps).Should().Be(3);
            Enum.IsDefined(typeof(DownloadProtocol), DownloadProtocol.Ftps).Should().BeTrue();
        }

        [Test]
        public async Task EmptyDirectories_ShouldBeHandledCorrectly()
        {
            // Arrange
            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"))
                .ReturnsAsync(new List<FtpsDirectoryItem>());

            // Act
            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());
            var releases = await _ftpsIndexer.FetchIndexerFeed(indexerRequest);

            // Assert
            releases.Should().BeEmpty();
        }

        [Test]
        public async Task LargeDirectories_ShouldHandleEfficiently()
        {
            // Arrange - Mock large number of releases
            var largeReleaseList = new List<FtpsDirectoryItem>();
            for (int i = 0; i < 1000; i++)
            {
                largeReleaseList.Add(new FtpsDirectoryItem
                {
                    Name = $"Movie.{2000 + i}.1080p.BluRay.x264-GROUP{i:D3}",
                    IsDirectory = true,
                    Size = 0,
                    FullPath = $"/movies/Movie.{2000 + i}.1080p.BluRay.x264-GROUP{i:D3}"
                });
            }

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"))
                .ReturnsAsync(largeReleaseList);

            // Mock individual directory contents
            for (int i = 0; i < 1000; i++)
            {
                var path = $"/movies/Movie.{2000 + i}.1080p.BluRay.x264-GROUP{i:D3}";
                var files = new List<FtpsDirectoryItem>
                {
                    new FtpsDirectoryItem
                    {
                        Name = $"Movie.{2000 + i}.1080p.BluRay.x264-GROUP{i:D3}.mkv",
                        IsDirectory = false,
                        Size = 8589934592 + i, // Unique size
                        FullPath = $"{path}/Movie.{2000 + i}.1080p.BluRay.x264-GROUP{i:D3}.mkv"
                    }
                };

                _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), path))
                    .ReturnsAsync(files);
            }

            // Act
            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());
            var releases = await _ftpsIndexer.FetchIndexerFeed(indexerRequest);

            // Assert
            releases.Should().HaveCount(1000);
            releases.Should().OnlyContain(r => r.Size > 8589934592);
        }
    }
}