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

        [SetUp]
        public void Setup()
        {
            _settings = new FtpsIndexerSettings
            {
                Host = "test.ftps.com",
                Port = 21,
                Username = "testuser",
                Password = "testpass",
                MovieDirectory = "movies",
                SecurityMode = FtpsSecurityMode.Explicit,
                ConnectionMode = FtpsConnectionMode.Passive
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
            name.Should().Be("FTPS Indexer");
        }

        [Test]
        public void should_support_rss()
        {
            // Act
            var supportsRss = Subject.SupportsRss;

            // Assert
            supportsRss.Should().BeTrue();
        }

        [Test]
        public void should_support_search()
        {
            // Act
            var supportsSearch = Subject.SupportsSearch;

            // Assert
            supportsSearch.Should().BeTrue();
        }

        [Test]
        public void should_parse_valid_movie_release_name()
        {
            // Arrange
            var releaseName = "The.Matrix.1999.1080p.BluRay.x264-GROUP";

            // Act
            var parseResult = Subject.ParseReleaseTitle(releaseName);

            // Assert
            parseResult.Should().NotBeNull();
            parseResult.MovieTitle.Should().Be("The Matrix");
            parseResult.Year.Should().Be(1999);
            parseResult.Quality.Quality.Name.Should().Be("Bluray-1080p");
        }

        [Test]
        public void should_return_null_for_invalid_release_name()
        {
            // Arrange
            var releaseName = "invalid-release-name";

            // Act
            var parseResult = Subject.ParseReleaseTitle(releaseName);

            // Assert
            parseResult.Should().BeNull();
        }

        [Test]
        public async Task FetchIndexerFeed_ValidDirectory_ShouldReturnReleases()
        {
            // Arrange
            var directoryItems = new List<FtpsDirectoryItem>
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

            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"))
                .ReturnsAsync(directoryItems);

            // Mock subdirectory listings with video files
            var matrixFiles = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "The.Matrix.1999.1080p.BluRay.x264-GROUP.mkv",
                    IsDirectory = false,
                    Size = 8589934592, // 8GB
                    FullPath = "/movies/The.Matrix.1999.1080p.BluRay.x264-GROUP/The.Matrix.1999.1080p.BluRay.x264-GROUP.mkv"
                }
            };

            var avatarFiles = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem
                {
                    Name = "Avatar.2009.720p.WEB.x264-RELEASE.mp4",
                    IsDirectory = false,
                    Size = 4294967296, // 4GB
                    FullPath = "/movies/Avatar.2009.720p.WEB.x264-RELEASE/Avatar.2009.720p.WEB.x264-RELEASE.mp4"
                }
            };

            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), 
                    "/movies/The.Matrix.1999.1080p.BluRay.x264-GROUP"))
                .ReturnsAsync(matrixFiles);

            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), 
                    "/movies/Avatar.2009.720p.WEB.x264-RELEASE"))
                .ReturnsAsync(avatarFiles);

            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());

            // Act
            var releases = await Subject.FetchIndexerFeed(indexerRequest);

            // Assert
            releases.Should().NotBeEmpty();
            releases.Should().HaveCount(2);
            
            var matrixRelease = releases.First(r => r.Title.Contains("Matrix"));
            matrixRelease.Size.Should().Be(8589934592);
            matrixRelease.DownloadUrl.Should().Contain("The.Matrix.1999.1080p.BluRay.x264-GROUP.mkv");
            
            var avatarRelease = releases.First(r => r.Title.Contains("Avatar"));
            avatarRelease.Size.Should().Be(4294967296);
            avatarRelease.DownloadUrl.Should().Contain("Avatar.2009.720p.WEB.x264-RELEASE.mp4");
        }

        [Test]
        public async Task FetchIndexerFeed_EmptyDirectory_ShouldReturnEmptyList()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"))
                .ReturnsAsync(new List<FtpsDirectoryItem>());

            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());

            // Act
            var releases = await Subject.FetchIndexerFeed(indexerRequest);

            // Assert
            releases.Should().BeEmpty();
        }

        [Test]
        public async Task FetchIndexerFeed_ConnectionError_ShouldThrowException()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                .Setup(x => x.GetDirectoryListingAsync(It.IsAny<FtpsSettings>(), "/movies"))
                .ThrowsAsync(new Exception("Connection failed"));

            var indexerRequest = new IndexerRequest("test", new IndexerPageableRequestChain());

            // Act & Assert
            await Subject.Awaiting(x => x.FetchIndexerFeed(indexerRequest))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Connection failed");
        }

        [Test]
        public void SelectBestFile_VideoFilePresent_ShouldSelectVideo()
        {
            // Arrange
            var files = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem { Name = "movie.nfo", IsDirectory = false, Size = 1024 },
                new FtpsDirectoryItem { Name = "movie.mkv", IsDirectory = false, Size = 8589934592 },
                new FtpsDirectoryItem { Name = "movie.srt", IsDirectory = false, Size = 2048 }
            };

            // Act
            var bestFile = Subject.SelectBestFile(files);

            // Assert
            bestFile.Should().NotBeNull();
            bestFile.Name.Should().Be("movie.mkv");
        }

        [Test]
        public void SelectBestFile_ArchiveFilePresent_ShouldSelectArchive()
        {
            // Arrange
            var files = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem { Name = "movie.nfo", IsDirectory = false, Size = 1024 },
                new FtpsDirectoryItem { Name = "movie.rar", IsDirectory = false, Size = 4294967296 },
                new FtpsDirectoryItem { Name = "movie.r00", IsDirectory = false, Size = 4294967296 },
                new FtpsDirectoryItem { Name = "movie.srt", IsDirectory = false, Size = 2048 }
            };

            // Act
            var bestFile = Subject.SelectBestFile(files);

            // Assert
            bestFile.Should().NotBeNull();
            bestFile.Name.Should().Be("movie.rar");
        }

        [Test]
        public void SelectBestFile_NoVideoOrArchive_ShouldSelectLargestFile()
        {
            // Arrange
            var files = new List<FtpsDirectoryItem>
            {
                new FtpsDirectoryItem { Name = "movie.nfo", IsDirectory = false, Size = 1024 },
                new FtpsDirectoryItem { Name = "movie.srt", IsDirectory = false, Size = 2048 },
                new FtpsDirectoryItem { Name = "movie.jpg", IsDirectory = false, Size = 512000 }
            };

            // Act
            var bestFile = Subject.SelectBestFile(files);

            // Assert
            bestFile.Should().NotBeNull();
            bestFile.Name.Should().Be("movie.jpg");
        }

        [Test]
        public void SelectBestFile_EmptyList_ShouldReturnNull()
        {
            // Arrange
            var files = new List<FtpsDirectoryItem>();

            // Act
            var bestFile = Subject.SelectBestFile(files);

            // Assert
            bestFile.Should().BeNull();
        }

        [Test]
        public void IsVideoFile_VideoExtensions_ShouldReturnTrue()
        {
            // Arrange & Act & Assert
            Subject.IsVideoFile("movie.mkv").Should().BeTrue();
            Subject.IsVideoFile("movie.mp4").Should().BeTrue();
            Subject.IsVideoFile("movie.avi").Should().BeTrue();
            Subject.IsVideoFile("movie.mov").Should().BeTrue();
            Subject.IsVideoFile("movie.wmv").Should().BeTrue();
            Subject.IsVideoFile("movie.flv").Should().BeTrue();
            Subject.IsVideoFile("movie.webm").Should().BeTrue();
            Subject.IsVideoFile("movie.m4v").Should().BeTrue();
            Subject.IsVideoFile("movie.mpg").Should().BeTrue();
            Subject.IsVideoFile("movie.mpeg").Should().BeTrue();
            Subject.IsVideoFile("movie.ts").Should().BeTrue();
            Subject.IsVideoFile("movie.m2ts").Should().BeTrue();
        }

        [Test]
        public void IsVideoFile_NonVideoExtensions_ShouldReturnFalse()
        {
            // Arrange & Act & Assert
            Subject.IsVideoFile("movie.nfo").Should().BeFalse();
            Subject.IsVideoFile("movie.srt").Should().BeFalse();
            Subject.IsVideoFile("movie.jpg").Should().BeFalse();
            Subject.IsVideoFile("movie.txt").Should().BeFalse();
            Subject.IsVideoFile("movie.rar").Should().BeFalse();
            Subject.IsVideoFile("movie.zip").Should().BeFalse();
        }

        [Test]
        public void IsArchiveFile_ArchiveExtensions_ShouldReturnTrue()
        {
            // Arrange & Act & Assert
            Subject.IsArchiveFile("movie.rar").Should().BeTrue();
            Subject.IsArchiveFile("movie.zip").Should().BeTrue();
            Subject.IsArchiveFile("movie.7z").Should().BeTrue();
            Subject.IsArchiveFile("movie.tar").Should().BeTrue();
            Subject.IsArchiveFile("movie.gz").Should().BeTrue();
            Subject.IsArchiveFile("movie.bz2").Should().BeTrue();
            Subject.IsArchiveFile("movie.xz").Should().BeTrue();
            Subject.IsArchiveFile("movie.r00").Should().BeTrue();
            Subject.IsArchiveFile("movie.r01").Should().BeTrue();
            Subject.IsArchiveFile("movie.r99").Should().BeTrue();
        }

        [Test]
        public void IsArchiveFile_NonArchiveExtensions_ShouldReturnFalse()
        {
            // Arrange & Act & Assert
            Subject.IsArchiveFile("movie.mkv").Should().BeFalse();
            Subject.IsArchiveFile("movie.nfo").Should().BeFalse();
            Subject.IsArchiveFile("movie.srt").Should().BeFalse();
            Subject.IsArchiveFile("movie.jpg").Should().BeFalse();
        }

        [Test]
        public void TestConnection_ValidSettings_ShouldReturnSuccess()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                .Setup(s => s.TestConnectionAsync(It.IsAny<FtpsSettings>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = Subject.TestConnection();

            // Assert
            result.IsValid.Should().BeTrue();
            result.Failures.Should().BeEmpty();
        }

        [Test]
        public void TestConnection_InvalidSettings_ShouldReturnFailure()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                .Setup(s => s.TestConnectionAsync(It.IsAny<FtpsSettings>()))
                .Returns(Task.FromResult(false));

            // Act
            var result = Subject.TestConnection();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Failures.Should().NotBeEmpty();
            result.Failures.Should().Contain(f => f.PropertyName == "General");
        }

        [Test]
        public void GetDefaultDefinitions_ShouldReturnFtpsDefinition()
        {
            // Act
            var definitions = Subject.DefaultDefinitions;

            // Assert
            definitions.Should().NotBeEmpty();
            definitions.Should().Contain(d => d.Name == "FTPS Indexer");
            definitions.Should().Contain(d => d.Implementation == "FtpsIndexer");
        }

        [TestCase("The.Matrix.1999.1080p.BluRay.x264-GROUP", "The Matrix", 1999)]
        [TestCase("Avatar.2009.720p.WEB.x264-RELEASE", "Avatar", 2009)]
        [TestCase("Inception.2010.PROPER.1080p.BluRay.x264-SPARKS", "Inception", 2010)]
        [TestCase("The.Dark.Knight.2008.1080p.BluRay.x264-METiS", "The Dark Knight", 2008)]
        public void ParseReleaseTitle_ValidReleases_ShouldParseCorrectly(
            string releaseName, string expectedTitle, int expectedYear)
        {
            // Act
            var parseResult = Subject.ParseReleaseTitle(releaseName);

            // Assert
            parseResult.Should().NotBeNull();
            parseResult.MovieTitle.Should().Be(expectedTitle);
            parseResult.Year.Should().Be(expectedYear);
        }

        [TestCase("file.txt")]
        [TestCase("random-string")]
        [TestCase("")]
        [TestCase(null)]
        public void ParseReleaseTitle_InvalidReleases_ShouldReturnNull(string releaseName)
        {
            // Act
            var parseResult = Subject.ParseReleaseTitle(releaseName);

            // Assert
            parseResult.Should().BeNull();
        }
    }
}