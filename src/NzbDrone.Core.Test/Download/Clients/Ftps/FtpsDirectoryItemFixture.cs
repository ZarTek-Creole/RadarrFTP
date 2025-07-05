using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.Clients.Ftps
{
    [TestFixture]
    public class FtpsDirectoryItemFixture : TestBase
    {
        [Test]
        public void should_initialize_with_default_values()
        {
            // Act
            var item = new FtpsDirectoryItem();

            // Assert
            item.Name.Should().BeNull();
            item.FullPath.Should().BeNull();
            item.Size.Should().Be(0);
            item.IsDirectory.Should().BeFalse();
        }

        [Test]
        public void should_set_properties_correctly()
        {
            // Arrange
            var name = "Movie.2023.1080p.BluRay.x264-GROUP.mkv";
            var fullPath = "/movies/Movie.2023.1080p.BluRay.x264-GROUP/Movie.2023.1080p.BluRay.x264-GROUP.mkv";
            var size = 8589934592L; // 8GB
            var isDirectory = false;

            // Act
            var item = new FtpsDirectoryItem
            {
                Name = name,
                FullPath = fullPath,
                Size = size,
                IsDirectory = isDirectory
            };

            // Assert
            item.Name.Should().Be(name);
            item.FullPath.Should().Be(fullPath);
            item.Size.Should().Be(size);
            item.IsDirectory.Should().Be(isDirectory);
        }

        [Test]
        public void should_handle_directory_item()
        {
            // Arrange & Act
            var item = new FtpsDirectoryItem
            {
                Name = "Movie.2023.1080p.BluRay.x264-GROUP",
                FullPath = "/movies/Movie.2023.1080p.BluRay.x264-GROUP",
                Size = 0,
                IsDirectory = true
            };

            // Assert
            item.Name.Should().Be("Movie.2023.1080p.BluRay.x264-GROUP");
            item.FullPath.Should().Be("/movies/Movie.2023.1080p.BluRay.x264-GROUP");
            item.Size.Should().Be(0);
            item.IsDirectory.Should().BeTrue();
        }

        [Test]
        public void should_handle_file_item()
        {
            // Arrange & Act
            var item = new FtpsDirectoryItem
            {
                Name = "movie.mkv",
                FullPath = "/movies/release/movie.mkv",
                Size = 4294967296L, // 4GB
                IsDirectory = false
            };

            // Assert
            item.Name.Should().Be("movie.mkv");
            item.FullPath.Should().Be("/movies/release/movie.mkv");
            item.Size.Should().Be(4294967296L);
            item.IsDirectory.Should().BeFalse();
        }

        [Test]
        public void should_handle_empty_strings()
        {
            // Arrange & Act
            var item = new FtpsDirectoryItem
            {
                Name = "",
                FullPath = "",
                Size = 0,
                IsDirectory = false
            };

            // Assert
            item.Name.Should().Be("");
            item.FullPath.Should().Be("");
            item.Size.Should().Be(0);
            item.IsDirectory.Should().BeFalse();
        }

        [Test]
        public void should_handle_null_values()
        {
            // Arrange & Act
            var item = new FtpsDirectoryItem
            {
                Name = null,
                FullPath = null,
                Size = 0,
                IsDirectory = false
            };

            // Assert
            item.Name.Should().BeNull();
            item.FullPath.Should().BeNull();
            item.Size.Should().Be(0);
            item.IsDirectory.Should().BeFalse();
        }

        [Test]
        public void should_handle_large_file_sizes()
        {
            // Arrange
            var largeSize = 21474836480L; // 20GB

            // Act
            var item = new FtpsDirectoryItem
            {
                Name = "large-movie.mkv",
                FullPath = "/movies/large-movie.mkv",
                Size = largeSize,
                IsDirectory = false
            };

            // Assert
            item.Size.Should().Be(largeSize);
        }

        [Test]
        public void should_handle_zero_size()
        {
            // Arrange & Act
            var item = new FtpsDirectoryItem
            {
                Name = "empty-file.txt",
                FullPath = "/movies/empty-file.txt",
                Size = 0,
                IsDirectory = false
            };

            // Assert
            item.Size.Should().Be(0);
        }

        [Test]
        public void should_handle_special_characters_in_names()
        {
            // Arrange
            var specialName = "Movie [2023] (1080p) {BluRay} - Group.mkv";
            var specialPath = "/movies/Movie [2023] (1080p) {BluRay} - Group/Movie [2023] (1080p) {BluRay} - Group.mkv";

            // Act
            var item = new FtpsDirectoryItem
            {
                Name = specialName,
                FullPath = specialPath,
                Size = 1024,
                IsDirectory = false
            };

            // Assert
            item.Name.Should().Be(specialName);
            item.FullPath.Should().Be(specialPath);
        }

        [Test]
        public void should_handle_unicode_characters()
        {
            // Arrange
            var unicodeName = "Amélie.2001.1080p.BluRay.x264-CAFÉ.mkv";
            var unicodePath = "/movies/Amélie.2001.1080p.BluRay.x264-CAFÉ/Amélie.2001.1080p.BluRay.x264-CAFÉ.mkv";

            // Act
            var item = new FtpsDirectoryItem
            {
                Name = unicodeName,
                FullPath = unicodePath,
                Size = 2048,
                IsDirectory = false
            };

            // Assert
            item.Name.Should().Be(unicodeName);
            item.FullPath.Should().Be(unicodePath);
        }

        [Test]
        public void should_handle_deeply_nested_paths()
        {
            // Arrange
            var deepPath = "/movies/2023/action/sci-fi/matrix/reloaded/extras/behind-the-scenes/making-of.mkv";

            // Act
            var item = new FtpsDirectoryItem
            {
                Name = "making-of.mkv",
                FullPath = deepPath,
                Size = 1024 * 1024 * 100, // 100MB
                IsDirectory = false
            };

            // Assert
            item.FullPath.Should().Be(deepPath);
        }

        [Test]
        public void should_distinguish_between_files_and_directories()
        {
            // Arrange
            var fileItem = new FtpsDirectoryItem
            {
                Name = "movie.mkv",
                IsDirectory = false
            };

            var directoryItem = new FtpsDirectoryItem
            {
                Name = "movie-folder",
                IsDirectory = true
            };

            // Assert
            fileItem.IsDirectory.Should().BeFalse();
            directoryItem.IsDirectory.Should().BeTrue();
        }

        [Test]
        public void should_handle_common_video_extensions()
        {
            // Arrange
            var videoExtensions = new[] { ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" };

            foreach (var extension in videoExtensions)
            {
                // Act
                var item = new FtpsDirectoryItem
                {
                    Name = $"movie{extension}",
                    FullPath = $"/movies/movie{extension}",
                    Size = 1024 * 1024 * 700, // 700MB
                    IsDirectory = false
                };

                // Assert
                item.Name.Should().EndWith(extension);
                item.Size.Should().BeGreaterThan(0);
                item.IsDirectory.Should().BeFalse();
            }
        }

        [Test]
        public void should_handle_common_archive_extensions()
        {
            // Arrange
            var archiveExtensions = new[] { ".rar", ".zip", ".7z", ".tar", ".gz", ".r00", ".r01" };

            foreach (var extension in archiveExtensions)
            {
                // Act
                var item = new FtpsDirectoryItem
                {
                    Name = $"movie{extension}",
                    FullPath = $"/movies/movie{extension}",
                    Size = 1024 * 1024 * 500, // 500MB
                    IsDirectory = false
                };

                // Assert
                item.Name.Should().EndWith(extension);
                item.Size.Should().BeGreaterThan(0);
                item.IsDirectory.Should().BeFalse();
            }
        }
    }
}