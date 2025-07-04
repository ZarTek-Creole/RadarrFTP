using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.Clients.Ftps
{
    [TestFixture]
    public class FtpsFixture : CoreTest<Ftps>
    {
        private FtpsSettings _settings;
        private Movie _movie;
        private RemoteMovie _remoteMovie;
        private List<FtpsReleaseItem> _mockReleases;

        [SetUp]
        public void Setup()
        {
            _settings = new FtpsSettings
            {
                Host = "test.ftps.server",
                Port = 21,
                Username = "testuser",
                Password = "testpass",
                RemoteBasePath = "/movies",
                UseSsl = true,
                ValidateCertificate = false,
                Category = "movies"
            };

            Subject.Definition = new DownloadClientDefinition { Settings = _settings };

            _movie = new Movie
            {
                Title = "Test Movie",
                Year = 2023,
                ImdbId = "tt1234567"
            };

            _remoteMovie = new RemoteMovie
            {
                Movie = _movie,
                Release = new ReleaseInfo
                {
                    Title = "Test.Movie.2023.1080p.BluRay.x264-GROUP"
                }
            };

            _mockReleases = new List<FtpsReleaseItem>
            {
                new FtpsReleaseItem
                {
                    Name = "Test.Movie.2023.1080p.BluRay.x264-GROUP",
                    FullPath = "/movies/Test.Movie.2023.1080p.BluRay.x264-GROUP.mkv",
                    Size = 2147483648, // 2GB
                    ModifiedTime = DateTime.UtcNow.AddHours(-1),
                    IsDirectory = false,
                    Extension = ".mkv"
                },
                new FtpsReleaseItem
                {
                    Name = "Test.Movie.2023.720p.WEB-DL.x264-ANOTHER",
                    FullPath = "/movies/Test.Movie.2023.720p.WEB-DL.x264-ANOTHER.mkv",
                    Size = 1073741824, // 1GB
                    ModifiedTime = DateTime.UtcNow.AddHours(-2),
                    IsDirectory = false,
                    Extension = ".mkv"
                }
            };

            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.TestConnectionAsync(_settings, default))
                  .ReturnsAsync(true);

            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.DirectoryExistsAsync(_settings.RemoteBasePath, _settings, default))
                  .ReturnsAsync(true);
        }

        [Test]
        public void Validate_should_return_success_when_settings_are_valid()
        {
            // Act
            var result = Subject.Test();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_should_return_failure_when_host_is_invalid()
        {
            // Arrange
            _settings.Host = "";

            // Act
            var result = Subject.Test();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "Host");
        }

        [Test]
        public void Validate_should_return_failure_when_connection_fails()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.TestConnectionAsync(_settings, default))
                  .ReturnsAsync(false);

            // Act
            var result = Subject.Test();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "Host");
        }

        [Test]
        public void Validate_should_return_failure_when_base_path_does_not_exist()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.DirectoryExistsAsync(_settings.RemoteBasePath, _settings, default))
                  .ReturnsAsync(false);

            // Act
            var result = Subject.Test();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(v => v.PropertyName == "RemoteBasePath");
        }

        [Test]
        public async Task Download_should_return_downloadId_when_release_found()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.GetDirectoryListingAsync(It.IsAny<string>(), _settings, default))
                  .ReturnsAsync(_mockReleases);

            // Act
            var result = await Subject.Download(_remoteMovie, null);

            // Assert
            result.Should().NotBeNullOrEmpty();
            Guid.TryParse(result, out _).Should().BeTrue();
        }

        [Test]
        public void Download_should_throw_when_no_releases_found()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.GetDirectoryListingAsync(It.IsAny<string>(), _settings, default))
                  .ReturnsAsync(new List<FtpsReleaseItem>());

            // Act & Assert
            Subject.Awaiting(s => s.Download(_remoteMovie, null))
                   .Should().ThrowAsync<DownloadClientRejectedReleaseException>();
        }

        [Test]
        public async Task Download_should_select_best_quality_release()
        {
            // Arrange
            var releases = new List<FtpsReleaseItem>
            {
                new FtpsReleaseItem
                {
                    Name = "Test.Movie.2023.480p.DVDRip.x264-LOW",
                    FullPath = "/movies/480p.mkv",
                    Size = 524288000, // 500MB
                    ModifiedTime = DateTime.UtcNow,
                    IsDirectory = false,
                    Extension = ".mkv"
                },
                new FtpsReleaseItem
                {
                    Name = "Test.Movie.2023.1080p.BluRay.x264-HIGH",
                    FullPath = "/movies/1080p.mkv",
                    Size = 2147483648, // 2GB
                    ModifiedTime = DateTime.UtcNow,
                    IsDirectory = false,
                    Extension = ".mkv"
                }
            };

            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.GetDirectoryListingAsync(It.IsAny<string>(), _settings, default))
                  .ReturnsAsync(releases);

            // Act
            var downloadId = await Subject.Download(_remoteMovie, null);

            // Assert
            downloadId.Should().NotBeNullOrEmpty();
            
            // Verify that the 1080p release was selected (higher score)
            var items = Subject.GetItems();
            items.Should().HaveCount(1);
            items.First().Title.Should().Contain("1080p");
        }

        [Test]
        public void GetItems_should_return_download_items()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.GetDirectoryListingAsync(It.IsAny<string>(), _settings, default))
                  .ReturnsAsync(_mockReleases);

            // Act
            Subject.Download(_remoteMovie, null).Wait();
            var items = Subject.GetItems();

            // Assert
            items.Should().HaveCount(1);
            items.First().Title.Should().Be(_mockReleases.First().Name);
            items.First().Category.Should().Be(_settings.Category);
        }

        [Test]
        public void RemoveItem_should_remove_download_item()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.GetDirectoryListingAsync(It.IsAny<string>(), _settings, default))
                  .ReturnsAsync(_mockReleases);

            Subject.Download(_remoteMovie, null).Wait();
            var items = Subject.GetItems();
            var itemToRemove = items.First();

            // Act
            Subject.RemoveItem(itemToRemove, false);

            // Assert
            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void GetStatus_should_return_client_info()
        {
            // Act
            var status = Subject.GetStatus();

            // Assert
            status.Should().NotBeNull();
            status.IsLocalhost.Should().BeFalse();
            status.OutputRootFolders.Should().NotBeEmpty();
        }

        [Test]
        public void MarkItemAsImported_should_mark_item_as_completed()
        {
            // Arrange
            Mocker.GetMock<IFtpsProxy>()
                  .Setup(s => s.GetDirectoryListingAsync(It.IsAny<string>(), _settings, default))
                  .ReturnsAsync(_mockReleases);

            Subject.Download(_remoteMovie, null).Wait();
            var items = Subject.GetItems();
            var item = items.First();

            // Act
            Subject.MarkItemAsImported(item);

            // Assert
            var updatedItems = Subject.GetItems();
            updatedItems.First().Status.Should().Be(DownloadItemStatus.Completed);
        }

        [TestCase("Test.Movie.2023.1080p.BluRay.x264-GROUP", "Test Movie", 2023, true)]
        [TestCase("Another.Movie.2022.720p.WEB-DL.x264-TEAM", "Test Movie", 2023, false)]
        [TestCase("Test.Movie.2023.CAM.x264-BAD", "Test Movie", 2023, true)]
        [TestCase("Random.File.2023.txt", "Test Movie", 2023, false)]
        public void IsMovieRelease_should_correctly_identify_matching_releases(string fileName, string movieTitle, int year, bool expected)
        {
            // Arrange
            var release = new FtpsReleaseItem
            {
                Name = fileName,
                IsDirectory = false,
                Extension = Path.GetExtension(fileName)
            };

            var movie = new Movie { Title = movieTitle, Year = year };
            var remoteMovie = new RemoteMovie { Movie = movie };

            // Act
            var result = CallPrivateMethod<bool>("IsMovieRelease", release, remoteMovie);

            // Assert
            result.Should().Be(expected);
        }

        [TestCase("Test.Movie.2023.1080p.BluRay.x264-GROUP", "1080p", "BluRay", "x264", "GROUP")]
        [TestCase("Another.Film.2022.720p.WEB-DL.H.264-TEAM", "720p", "WEB-DL", "H.264", "TEAM")]
        [TestCase("Old.Movie.1999.480p.DVDRip.XviD-CLASSIC", "480p", "DVDRip", "XviD", "CLASSIC")]
        public void ParseReleaseInfo_should_extract_correct_metadata(string fileName, string expectedQuality, 
            string expectedSource, string expectedCodec, string expectedGroup)
        {
            // Arrange
            var release = new FtpsReleaseItem { Name = fileName };

            // Act
            CallPrivateMethod("ParseReleaseInfo", release);

            // Assert
            release.Quality.Should().Be(expectedQuality);
            release.Source.Should().Be(expectedSource);
            release.Codec.Should().Contain(expectedCodec);
            release.ReleaseGroup.Should().Be(expectedGroup);
        }

        [Test]
        public void CalculateReleaseScore_should_prefer_higher_quality()
        {
            // Arrange
            var release1080p = new FtpsReleaseItem
            {
                Quality = "1080p",
                Source = "BluRay",
                Size = 2147483648,
                ModifiedTime = DateTime.UtcNow
            };

            var release720p = new FtpsReleaseItem
            {
                Quality = "720p",
                Source = "BluRay",
                Size = 1073741824,
                ModifiedTime = DateTime.UtcNow
            };

            // Act
            var score1080p = CallPrivateMethod<int>("CalculateReleaseScore", release1080p, _remoteMovie);
            var score720p = CallPrivateMethod<int>("CalculateReleaseScore", release720p, _remoteMovie);

            // Assert
            score1080p.Should().BeGreaterThan(score720p);
        }

        [Test]
        public void CalculateReleaseScore_should_prefer_bluray_over_cam()
        {
            // Arrange
            var blurayRelease = new FtpsReleaseItem
            {
                Quality = "1080p",
                Source = "BluRay",
                Size = 2147483648,
                ModifiedTime = DateTime.UtcNow
            };

            var camRelease = new FtpsReleaseItem
            {
                Quality = "1080p",
                Source = "CAM",
                Size = 2147483648,
                ModifiedTime = DateTime.UtcNow
            };

            // Act
            var blurayScore = CallPrivateMethod<int>("CalculateReleaseScore", blurayRelease, _remoteMovie);
            var camScore = CallPrivateMethod<int>("CalculateReleaseScore", camRelease, _remoteMovie);

            // Assert
            blurayScore.Should().BeGreaterThan(camScore);
        }

        [Test]
        public void GenerateSearchPaths_should_include_year_based_paths()
        {
            // Act
            var paths = CallPrivateMethod<List<string>>("GenerateSearchPaths", _remoteMovie);

            // Assert
            paths.Should().Contain("/movies");
            paths.Should().Contain("/movies/2023");
            paths.Should().Contain("/movies/Movies/2023");
            paths.Should().Contain("/movies/T"); // First letter of "Test Movie"
        }

        [Test]
        public void GenerateSearchPaths_should_include_category_paths()
        {
            // Act
            var paths = CallPrivateMethod<List<string>>("GenerateSearchPaths", _remoteMovie);

            // Assert
            paths.Should().Contain("/movies/movies"); // Category path
            paths.Should().Contain("/movies/movies/2023"); // Category + year path
        }

        private T CallPrivateMethod<T>(string methodName, params object[] parameters)
        {
            var method = typeof(Ftps).GetMethod(methodName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)method.Invoke(Subject, parameters);
        }

        private void CallPrivateMethod(string methodName, params object[] parameters)
        {
            var method = typeof(Ftps).GetMethod(methodName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(Subject, parameters);
        }
    }
}