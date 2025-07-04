using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

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
        public async Task should_test_connection_successfully()
        {
            // Act
            var result = await Subject.TestConnectionAsync();

            // Assert
            result.Should().BeTrue();
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
    }
}