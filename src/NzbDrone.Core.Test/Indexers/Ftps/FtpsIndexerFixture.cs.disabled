using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Ftps;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Indexers.Ftps
{
    [TestFixture]
    public class FtpsIndexerFixture : CoreTest<FtpsIndexer>
    {
        private FtpsIndexerSettings _settings;
        private Mock<IFtpsProxy> _ftpsProxy;
        private List<FtpsDirectoryItem> _mockDirectories;
        private List<FtpsDirectoryItem> _mockFiles;

        [SetUp]
        public void Setup()
        {
            _settings = new FtpsIndexerSettings
            {
                Host = "test.ftps.server",
                Port = 21,
                Username = "testuser",
                Password = "testpass",
                MovieDirectory = "movies",
                SecurityMode = FtpsSecurityMode.Explicit,
                ConnectionMode = FtpsConnectionMode.Passive
            };

            _ftpsProxy = new Mock<IFtpsProxy>();
            Mocker.GetMock<IFtpsProxy>().Setup(s => s.TestConnectionAsync(It.IsAny<FtpsIndexerSettings>()))
                .ReturnsAsync(true);

            // Mock directories
            _mockDirectories = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "The.Matrix.1999.1080p.BluRay.x264-GROUP",
                    IsDirectory = true,
                    LastModified = DateTime.UtcNow.AddDays(-1)
                },
                new FtpsDirectoryItem
                {
                    Name = "Blade.Runner.2049.2017.4K.UHD.BluRay.x265-GROUP",
                    IsDirectory = true,
                    LastModified = DateTime.UtcNow.AddDays(-2)
                }
            };

            // Mock files
            _mockFiles = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "The.Matrix.1999.1080p.BluRay.x264-GROUP.mkv",
                    IsDirectory = false,
                    Size = 8589934592, // 8 GB
                    LastModified = DateTime.UtcNow.AddDays(-1)
                }
            };

            Subject.Definition = new IndexerDefinition
            {
                Id = 1,
                Name = "Test FTPS Indexer",
                Settings = _settings
            };
        }

        [Test]
        public void should_have_correct_protocol()
        {
            Subject.Protocol.Should().Be(DownloadProtocol.Ftps);
        }

        [Test]
        public void should_support_rss_and_search()
        {
            Subject.SupportsRss.Should().BeTrue();
            Subject.SupportsSearch.Should().BeTrue();
        }

        [Test]
        public async Task should_fetch_recent_releases()
        {
            // Arrange
            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies"))
                .ReturnsAsync(_mockDirectories);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies/The.Matrix.1999.1080p.BluRay.x264-GROUP"))
                .ReturnsAsync(_mockFiles);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies/Blade.Runner.2049.2017.4K.UHD.BluRay.x265-GROUP"))
                .ReturnsAsync(new List<FtpsDirectoryItem>());

            // Act
            var releases = await Subject.FetchRecent();

            // Assert
            releases.Should().NotBeNull();
            releases.Should().HaveCount(1);
            
            var release = releases.First();
            release.Title.Should().Contain("Matrix");
            release.Size.Should().Be(8589934592);
            release.DownloadUrl.Should().StartWith("ftps://");
            release.DownloadProtocol.Should().Be(DownloadProtocol.Ftps);
        }

        [Test]
        public async Task should_search_for_movie()
        {
            // Arrange
            var searchCriteria = new MovieSearchCriteria
            {
                Movie = new Movie
                {
                    Title = "Matrix",
                    Year = 1999
                }
            };

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies"))
                .ReturnsAsync(_mockDirectories);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies/The.Matrix.1999.1080p.BluRay.x264-GROUP"))
                .ReturnsAsync(_mockFiles);

            // Act
            var releases = await Subject.Fetch(searchCriteria);

            // Assert
            releases.Should().NotBeNull();
            releases.Should().HaveCount(1);
            
            var release = releases.First();
            release.Title.Should().Contain("Matrix");
            release.DownloadUrl.Should().Contain("Matrix");
        }

        [Test]
        public async Task should_filter_search_results_by_title()
        {
            // Arrange
            var searchCriteria = new MovieSearchCriteria
            {
                Movie = new Movie
                {
                    Title = "Blade Runner",
                    Year = 2017
                }
            };

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies"))
                .ReturnsAsync(_mockDirectories);

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies/Blade.Runner.2049.2017.4K.UHD.BluRay.x265-GROUP"))
                .ReturnsAsync(new List<FtpsDirectoryItem>
                {
                    new FtpsDirectoryItem
                    {
                        Name = "Blade.Runner.2049.2017.4K.UHD.BluRay.x265-GROUP.mkv",
                        IsDirectory = false,
                        Size = 21474836480, // 20 GB
                        LastModified = DateTime.UtcNow.AddDays(-2)
                    }
                });

            // Act
            var releases = await Subject.Fetch(searchCriteria);

            // Assert
            releases.Should().NotBeNull();
            releases.Should().HaveCount(1);
            
            var release = releases.First();
            release.Title.Should().Contain("Blade Runner");
            release.Size.Should().Be(21474836480);
        }

        [Test]
        public async Task should_handle_connection_failure()
        {
            // Arrange
            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Connection failed"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await Subject.FetchRecent());
        }

        [Test]
        public async Task should_only_include_video_files()
        {
            // Arrange
            var allFiles = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "movie.mkv",
                    IsDirectory = false,
                    Size = 1000000,
                    LastModified = DateTime.UtcNow
                },
                new FtpsDirectoryItem
                {
                    Name = "subtitle.srt",
                    IsDirectory = false,
                    Size = 50000,
                    LastModified = DateTime.UtcNow
                },
                new FtpsDirectoryItem
                {
                    Name = "readme.txt",
                    IsDirectory = false,
                    Size = 1000,
                    LastModified = DateTime.UtcNow
                }
            };

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies"))
                .ReturnsAsync(_mockDirectories.Take(1));

            _ftpsProxy.Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsIndexerSettings>(), "movies/The.Matrix.1999.1080p.BluRay.x264-GROUP"))
                .ReturnsAsync(allFiles);

            // Act
            var releases = await Subject.FetchRecent();

            // Assert
            releases.Should().HaveCount(1);
            releases.First().Title.Should().Contain(".mkv");
        }
    }
}