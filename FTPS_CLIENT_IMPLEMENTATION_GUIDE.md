# Guide Ultra-Complet : Implémentation d'un Client FTPS pour Radarr

## Table des Matières
1. [Vue d'ensemble de l'architecture](#vue-densemble-de-larchitecture)
2. [Structure des modifications requises](#structure-des-modifications-requises)
3. [Implémentation détaillée](#implémentation-détaillée)
4. [Configuration de l'interface utilisateur](#configuration-de-linterface-utilisateur)
5. [Tests et validation](#tests-et-validation)
6. [Intégration et déploiement](#intégration-et-déploiement)

## Vue d'ensemble de l'architecture

### Architecture existante des clients de téléchargement
Radarr utilise une architecture modulaire pour les clients de téléchargement :

- **Interface principale** : `IDownloadClient` définit les méthodes obligatoires
- **Classe de base** : `DownloadClientBase<TSettings>` fournit les fonctionnalités communes
- **Classes spécialisées** : `UsenetClientBase` et `TorrentClientBase` pour les protocoles spécifiques
- **Implémentations concrètes** : Chaque client (Sabnzbd, QBittorrent, etc.) hérite de ces bases

### Protocoles supportés actuellement
```csharp
public enum DownloadProtocol
{
    Unknown = 0,
    Usenet = 1,
    Torrent = 2
}
```

**Nouveau protocole requis** : `Ftps = 3`

## Structure des modifications requises

### 1. Core - Modifications du backend (.NET)

#### A. Nouveau protocole FTPS
```
src/NzbDrone.Core/Indexers/DownloadProtocol.cs
```
- Ajouter `Ftps = 3` à l'énumération

#### B. Nouveau client FTPS
```
src/NzbDrone.Core/Download/Clients/Ftps/
├── FtpsClient.cs                    # Classe principale du client
├── FtpsSettings.cs                  # Configuration du client
├── FtpsProxy.cs                     # Proxy pour FluentFTP
├── FtpsDirectoryItem.cs             # Modèle pour les éléments de répertoire
├── FtpsDownloadItem.cs              # Modèle pour les téléchargements
├── FtpsConnectionMode.cs            # Enum pour les modes de connexion
└── FtpsSecurityMode.cs              # Enum pour les modes de sécurité
```

#### C. Classe de base spécialisée
```
src/NzbDrone.Core/Download/FtpsClientBase.cs
```
- Nouvelle classe de base pour les clients FTPS (similaire à `UsenetClientBase`)

### 2. API V3 - Exposition des paramètres

#### A. Ressources API
```
src/Radarr.Api.V3/DownloadClient/DownloadClientResource.cs
```
- Pas de modifications nécessaires (utilise déjà `DownloadProtocol`)

### 3. Frontend - Interface utilisateur

#### A. Composants de configuration
```
frontend/src/Settings/DownloadClients/DownloadClients/
├── FtpsClientSettings.js            # Composant de configuration FTPS
├── FtpsClientSettings.css           # Styles spécifiques
└── FtpsClientSettingsConnector.js   # Connecteur Redux
```

#### B. Modification des composants existants
```
frontend/src/Settings/DownloadClients/DownloadClients/
├── AddDownloadClientModalContent.js # Ajouter FTPS aux options
└── DownloadClient.js                # Gestion du nouveau type
```

### 4. Tests

#### A. Tests unitaires
```
src/NzbDrone.Core.Test/Download/Clients/Ftps/
├── FtpsClientFixture.cs             # Tests du client principal
├── FtpsSettingsFixture.cs           # Tests des paramètres
└── FtpsProxyFixture.cs              # Tests du proxy
```

#### B. Tests d'intégration
```
src/NzbDrone.Integration.Test/Download/
└── FtpsClientIntegrationTest.cs     # Tests bout-en-bout
```

## Implémentation détaillée

### 1. Extension du protocole de téléchargement

#### Fichier : `src/NzbDrone.Core/Indexers/DownloadProtocol.cs`
```csharp
namespace NzbDrone.Core.Indexers
{
    public enum DownloadProtocol
    {
        Unknown = 0,
        Usenet = 1,
        Torrent = 2,
        Ftps = 3  // NOUVEAU
    }
}
```

### 2. Classe de base FTPS

#### Fichier : `src/NzbDrone.Core/Download/FtpsClientBase.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;
using NLog;

namespace NzbDrone.Core.Download
{
    public abstract class FtpsClientBase<TSettings> : DownloadClientBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected FtpsClientBase(IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            Logger logger,
            ILocalizationService localizationService)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
        }

        public override DownloadProtocol Protocol => DownloadProtocol.Ftps;

        protected abstract Task<bool> TestConnectionAsync();
        protected abstract Task<IEnumerable<string>> GetDirectoryListingAsync(string path);
        protected abstract Task<string> DownloadFileAsync(string remotePath, string localPath);
    }
}
```

### 3. Paramètres de configuration FTPS

#### Fichier : `src/NzbDrone.Core/Download/Clients/Ftps/FtpsSettings.cs`
```csharp
using System.ComponentModel;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsSettingsValidator : AbstractValidator<FtpsSettings>
    {
        public FtpsSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
            RuleFor(c => c.BasePath).NotEmpty();
            RuleFor(c => c.MovieDirectory).NotEmpty();
        }
    }

    public class FtpsSettings : DownloadClientSettingsBase<FtpsSettings>
    {
        private static readonly FtpsSettingsValidator Validator = new();

        public FtpsSettings()
        {
            Host = "localhost";
            Port = 21;
            SecurityMode = FtpsSecurityMode.Explicit;
            ConnectionMode = FtpsConnectionMode.Passive;
            ValidateCertificate = true;
            BasePath = "/";
            MovieDirectory = "movies";
            Priority = 1;
            ScanInterval = 60;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Number)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "SSL/TLS Mode", Type = FieldType.Select, SelectOptions = typeof(FtpsSecurityMode), HelpText = "FTPS security mode")]
        public FtpsSecurityMode SecurityMode { get; set; }

        [FieldDefinition(5, Label = "Connection Mode", Type = FieldType.Select, SelectOptions = typeof(FtpsConnectionMode), HelpText = "FTP connection mode")]
        public FtpsConnectionMode ConnectionMode { get; set; }

        [FieldDefinition(6, Label = "Validate Certificate", Type = FieldType.Checkbox, HelpText = "Validate SSL/TLS certificate")]
        public bool ValidateCertificate { get; set; }

        [FieldDefinition(7, Label = "Base Path", Type = FieldType.Textbox, HelpText = "Base directory path on the FTPS server")]
        public string BasePath { get; set; }

        [FieldDefinition(8, Label = "Movie Directory", Type = FieldType.Textbox, HelpText = "Directory containing movies")]
        public string MovieDirectory { get; set; }

        [FieldDefinition(9, Label = "Priority", Type = FieldType.Number, HelpText = "Server priority (1-100)")]
        public int Priority { get; set; }

        [FieldDefinition(10, Label = "Scan Interval", Type = FieldType.Number, HelpText = "Scan interval in minutes")]
        public int ScanInterval { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
```

### 4. Énumérations pour les modes de connexion

#### Fichier : `src/NzbDrone.Core/Download/Clients/Ftps/FtpsSecurityMode.cs`
```csharp
using System.ComponentModel;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public enum FtpsSecurityMode
    {
        [Description("Explicit (AUTH TLS)")]
        Explicit = 0,

        [Description("Implicit (SSL)")]
        Implicit = 1,

        [Description("None (Plain FTP)")]
        None = 2
    }
}
```

#### Fichier : `src/NzbDrone.Core/Download/Clients/Ftps/FtpsConnectionMode.cs`
```csharp
using System.ComponentModel;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public enum FtpsConnectionMode
    {
        [Description("Passive")]
        Passive = 0,

        [Description("Active")]
        Active = 1
    }
}
```

### 5. Proxy FTPS (utilisant FluentFTP)

#### Fichier : `src/NzbDrone.Core/Download/Clients/Ftps/FtpsProxy.cs`
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FluentFTP;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.Clients.Ftps;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public interface IFtpsProxy
    {
        Task<bool> TestConnectionAsync(FtpsSettings settings);
        Task<IEnumerable<FtpsDirectoryItem>> GetDirectoryListingAsync(FtpsSettings settings, string path);
        Task<bool> DownloadFileAsync(FtpsSettings settings, string remotePath, string localPath);
        Task<bool> FileExistsAsync(FtpsSettings settings, string path);
        Task<long> GetFileSizeAsync(FtpsSettings settings, string path);
    }

    public class FtpsProxy : IFtpsProxy
    {
        private readonly Logger _logger;

        public FtpsProxy(Logger logger)
        {
            _logger = logger;
        }

        public async Task<bool> TestConnectionAsync(FtpsSettings settings)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                return client.IsConnected;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test FTPS connection to {0}:{1}", settings.Host, settings.Port);
                return false;
            }
        }

        public async Task<IEnumerable<FtpsDirectoryItem>> GetDirectoryListingAsync(FtpsSettings settings, string path)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                var items = await client.GetListingAsync(path);
                
                return items.Select(item => new FtpsDirectoryItem
                {
                    Name = item.Name,
                    FullPath = item.FullName,
                    Size = item.Size,
                    IsDirectory = item.Type == FtpObjectType.Directory,
                    ModifiedDate = item.Modified
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get directory listing for {0}", path);
                throw;
            }
        }

        public async Task<bool> DownloadFileAsync(FtpsSettings settings, string remotePath, string localPath)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                var result = await client.DownloadFileAsync(localPath, remotePath);
                return result.IsSuccess();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download file from {0} to {1}", remotePath, localPath);
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(FtpsSettings settings, string path)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                return await client.FileExistsAsync(path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to check if file exists: {0}", path);
                return false;
            }
        }

        public async Task<long> GetFileSizeAsync(FtpsSettings settings, string path)
        {
            using var client = CreateClient(settings);
            
            try
            {
                await client.ConnectAsync();
                return await client.GetFileSizeAsync(path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get file size for: {0}", path);
                return 0;
            }
        }

        private FtpClient CreateClient(FtpsSettings settings)
        {
            var client = new FtpClient(settings.Host, settings.Port, settings.Username, settings.Password);
            
            // Configuration SSL/TLS
            switch (settings.SecurityMode)
            {
                case FtpsSecurityMode.Explicit:
                    client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
                    client.Config.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    break;
                    
                case FtpsSecurityMode.Implicit:
                    client.Config.EncryptionMode = FtpEncryptionMode.Implicit;
                    client.Config.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    break;
                    
                case FtpsSecurityMode.None:
                    client.Config.EncryptionMode = FtpEncryptionMode.None;
                    break;
            }
            
            // Configuration du mode de connexion
            client.Config.DataConnectionType = settings.ConnectionMode == FtpsConnectionMode.Active 
                ? FtpDataConnectionType.AutoActive 
                : FtpDataConnectionType.AutoPassive;
            
            // Configuration de la validation des certificats
            if (!settings.ValidateCertificate)
            {
                client.Config.ValidateAnyCertificate = true;
            }
            
            return client;
        }
    }
}
```

### 6. Modèles de données

#### Fichier : `src/NzbDrone.Core/Download/Clients/Ftps/FtpsDirectoryItem.cs`
```csharp
using System;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsDirectoryItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }
        public bool IsDirectory { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
```

### 7. Client FTPS principal

#### Fichier : `src/NzbDrone.Core/Download/Clients/Ftps/FtpsClient.cs`
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Ftps
{
    public class FtpsClient : FtpsClientBase<FtpsSettings>
    {
        private readonly IFtpsProxy _proxy;
        private static readonly Regex MovieReleaseRegex = new Regex(
            @"^(?<title>.+?)\.(?<year>\d{4})\..*?(?<quality>1080p|720p|480p|2160p|4K).*?-(?<group>\w+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FtpsClient(IFtpsProxy proxy,
            IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            Logger logger,
            ILocalizationService localizationService)
            : base(configService, diskProvider, remotePathMappingService, logger, localizationService)
        {
            _proxy = proxy;
        }

        public override string Name => "FTPS Client";

        public override async Task<string> Download(RemoteMovie remoteMovie, IIndexer indexer)
        {
            var title = remoteMovie.Release.Title;
            var downloadId = Guid.NewGuid().ToString();
            
            try
            {
                // Recherche du fichier sur le serveur FTPS
                var movieFiles = await FindMovieFilesAsync(title);
                
                if (!movieFiles.Any())
                {
                    throw new ReleaseDownloadException(remoteMovie.Release, "Movie not found on FTPS server");
                }

                // Sélection du meilleur fichier
                var selectedFile = SelectBestFile(movieFiles, remoteMovie);
                
                // Téléchargement
                var localPath = GetDownloadPath(selectedFile.Name);
                var success = await _proxy.DownloadFileAsync(Settings, selectedFile.FullPath, localPath);
                
                if (!success)
                {
                    throw new ReleaseDownloadException(remoteMovie.Release, "Failed to download movie file");
                }

                _logger.Info("Successfully downloaded {0} to {1}", selectedFile.Name, localPath);
                return downloadId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download movie: {0}", title);
                throw;
            }
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            // Implémentation pour récupérer les éléments en cours de téléchargement
            // Pour FTPS, cela pourrait inclure un système de cache ou de base de données locale
            return new List<DownloadClientItem>();
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            // Implémentation pour supprimer un élément
            if (deleteData)
            {
                DeleteItemData(item);
            }
        }

        public override DownloadClientInfo GetStatus()
        {
            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { new OsPath(GetDownloadPath("")) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            failures.AddIfNotNull(TestBasePath());
        }

        protected override async Task<bool> TestConnectionAsync()
        {
            return await _proxy.TestConnectionAsync(Settings);
        }

        protected override async Task<IEnumerable<string>> GetDirectoryListingAsync(string path)
        {
            var items = await _proxy.GetDirectoryListingAsync(Settings, path);
            return items.Select(i => i.FullPath);
        }

        protected override async Task<string> DownloadFileAsync(string remotePath, string localPath)
        {
            var success = await _proxy.DownloadFileAsync(Settings, remotePath, localPath);
            return success ? localPath : null;
        }

        private async Task<List<FtpsDirectoryItem>> FindMovieFilesAsync(string title)
        {
            var moviePath = Path.Combine(Settings.BasePath, Settings.MovieDirectory);
            var allFiles = await _proxy.GetDirectoryListingAsync(Settings, moviePath);
            
            return allFiles
                .Where(f => !f.IsDirectory && IsMovieFile(f.Name, title))
                .ToList();
        }

        private bool IsMovieFile(string fileName, string searchTitle)
        {
            var match = MovieReleaseRegex.Match(fileName);
            if (!match.Success) return false;
            
            var fileTitle = match.Groups["title"].Value.Replace(".", " ");
            return fileTitle.ContainsIgnoreCase(searchTitle);
        }

        private FtpsDirectoryItem SelectBestFile(List<FtpsDirectoryItem> files, RemoteMovie remoteMovie)
        {
            // Logique de sélection du meilleur fichier basée sur la qualité
            return files.OrderByDescending(f => f.Size).First();
        }

        private string GetDownloadPath(string fileName)
        {
            var downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RadarrDownloads");
            Directory.CreateDirectory(downloadDir);
            return Path.Combine(downloadDir, fileName);
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var result = _proxy.TestConnectionAsync(Settings).Result;
                return result ? null : new ValidationFailure("Host", "Unable to connect to FTPS server");
            }
            catch (Exception ex)
            {
                return new ValidationFailure("Host", $"Connection test failed: {ex.Message}");
            }
        }

        private ValidationFailure TestBasePath()
        {
            try
            {
                var items = _proxy.GetDirectoryListingAsync(Settings, Settings.BasePath).Result;
                return null;
            }
            catch (Exception ex)
            {
                return new ValidationFailure("BasePath", $"Invalid base path: {ex.Message}");
            }
        }
    }
}
```

## Configuration de l'interface utilisateur

### 1. Composant de paramètres FTPS

#### Fichier : `frontend/src/Settings/DownloadClients/DownloadClients/FtpsClientSettings.js`
```javascript
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import translate from 'Utilities/String/translate';

const securityModeOptions = [
  { key: 0, value: 'Explicit (AUTH TLS)' },
  { key: 1, value: 'Implicit (SSL)' },
  { key: 2, value: 'None (Plain FTP)' }
];

const connectionModeOptions = [
  { key: 0, value: 'Passive' },
  { key: 1, value: 'Active' }
];

class FtpsClientSettings extends Component {
  render() {
    const {
      settings,
      onInputChange
    } = this.props;

    const {
      host,
      port,
      username,
      password,
      securityMode,
      connectionMode,
      validateCertificate,
      basePath,
      movieDirectory,
      priority,
      scanInterval
    } = settings;

    return (
      <div>
        <FormGroup>
          <FormLabel>Host</FormLabel>
          <FormInputGroup
            type={inputTypes.TEXT}
            name="host"
            value={host}
            onChange={onInputChange}
            helpText="FTPS server hostname or IP address"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Port</FormLabel>
          <FormInputGroup
            type={inputTypes.NUMBER}
            name="port"
            value={port}
            onChange={onInputChange}
            helpText="FTPS server port (default: 21)"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Username</FormLabel>
          <FormInputGroup
            type={inputTypes.TEXT}
            name="username"
            value={username}
            onChange={onInputChange}
            helpText="FTPS username"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Password</FormLabel>
          <FormInputGroup
            type={inputTypes.PASSWORD}
            name="password"
            value={password}
            onChange={onInputChange}
            helpText="FTPS password"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Security Mode</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="securityMode"
            value={securityMode}
            values={securityModeOptions}
            onChange={onInputChange}
            helpText="FTPS security mode"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Connection Mode</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="connectionMode"
            value={connectionMode}
            values={connectionModeOptions}
            onChange={onInputChange}
            helpText="FTP connection mode"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Validate Certificate</FormLabel>
          <FormInputGroup
            type={inputTypes.CHECK}
            name="validateCertificate"
            value={validateCertificate}
            onChange={onInputChange}
            helpText="Validate SSL/TLS certificate"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Base Path</FormLabel>
          <FormInputGroup
            type={inputTypes.TEXT}
            name="basePath"
            value={basePath}
            onChange={onInputChange}
            helpText="Base directory path on the FTPS server"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Movie Directory</FormLabel>
          <FormInputGroup
            type={inputTypes.TEXT}
            name="movieDirectory"
            value={movieDirectory}
            onChange={onInputChange}
            helpText="Directory containing movies"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Priority</FormLabel>
          <FormInputGroup
            type={inputTypes.NUMBER}
            name="priority"
            value={priority}
            onChange={onInputChange}
            helpText="Server priority (1-100)"
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Scan Interval</FormLabel>
          <FormInputGroup
            type={inputTypes.NUMBER}
            name="scanInterval"
            value={scanInterval}
            onChange={onInputChange}
            helpText="Scan interval in minutes"
          />
        </FormGroup>
      </div>
    );
  }
}

FtpsClientSettings.propTypes = {
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default FtpsClientSettings;
```

### 2. Intégration dans AddDownloadClientModalContent

#### Modification du fichier : `frontend/src/Settings/DownloadClients/DownloadClients/AddDownloadClientModalContent.js`
```javascript
// Ajouter l'import
import FtpsClientSettings from './FtpsClientSettings';

// Dans la méthode render, ajouter après les autres types :
{
  implementationName === 'FtpsClient' &&
  <FtpsClientSettings
    settings={settings}
    onInputChange={onInputChange}
  />
}
```

## Tests et validation

### 1. Tests unitaires

#### Fichier : `src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsClientFixture.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
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

        // Ajoutez d'autres tests selon vos besoins
    }
}
```

### 2. Tests d'intégration

#### Fichier : `src/NzbDrone.Integration.Test/Download/FtpsClientIntegrationTest.cs`
```csharp
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Ftps;
using NzbDrone.Integration.Test.Framework;

namespace NzbDrone.Integration.Test.Download
{
    [TestFixture]
    public class FtpsClientIntegrationTest : IntegrationTest
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
                MovieDirectory = ""
            };
        }

        [Test]
        public async Task should_connect_to_test_server()
        {
            // Arrange
            var proxy = new FtpsProxy(new LoggerMock());

            // Act
            var result = await proxy.TestConnectionAsync(_settings);

            // Assert
            result.Should().BeTrue();
        }

        // Ajoutez d'autres tests d'intégration
    }
}
```

## Intégration et déploiement

### 1. Enregistrement du service

#### Modification du fichier : `src/NzbDrone.Core/Datastore/TableMapping.cs` (si nécessaire)
```csharp
// Ajouter si des tables spécifiques sont nécessaires pour FTPS
```

### 2. Configuration de l'injection de dépendances

#### Modification du fichier : `src/NzbDrone.Core/Lifecycle/ApplicationStartup.cs` (ou équivalent)
```csharp
// Enregistrer les services FTPS
container.RegisterType<IFtpsProxy, FtpsProxy>();
```

### 3. Dépendances NuGet

#### Modification du fichier : `src/NzbDrone.Core/Radarr.Core.csproj`
```xml
<PackageReference Include="FluentFTP" Version="48.0.2" />
```

### 4. Configuration du build

#### Modification du fichier : `src/Directory.Build.props`
```xml
<!-- Ajouter si nécessaire des configurations spécifiques -->
```

## Checklist de déploiement

### Phase 1 : Développement de base
- [ ] Créer l'enum `DownloadProtocol.Ftps`
- [ ] Implémenter `FtpsClientBase`
- [ ] Créer `FtpsSettings` avec validation
- [ ] Développer `FtpsProxy` utilisant FluentFTP
- [ ] Implémenter `FtpsClient` principal
- [ ] Tester les connexions FTPS de base

### Phase 2 : Intégration API
- [ ] Vérifier l'exposition API des nouveaux paramètres
- [ ] Tester les endpoints de configuration
- [ ] Valider la sérialisation/désérialisation

### Phase 3 : Interface utilisateur
- [ ] Créer les composants de configuration React
- [ ] Intégrer dans l'interface existante
- [ ] Tester l'interface utilisateur
- [ ] Valider la persistance des paramètres

### Phase 4 : Tests
- [ ] Développer les tests unitaires
- [ ] Créer les tests d'intégration
- [ ] Tester avec différents serveurs FTPS
- [ ] Valider les scénarios d'erreur

### Phase 5 : Documentation
- [ ] Documenter l'API
- [ ] Créer la documentation utilisateur
- [ ] Mettre à jour les guides de configuration
- [ ] Documenter les limitations et prérequis

### Phase 6 : Déploiement
- [ ] Configurer le build CI/CD
- [ ] Tester en environnement de staging
- [ ] Valider les migrations de base de données
- [ ] Déployer en production

## Considérations de sécurité

1. **Chiffrement des identifiants** : Les mots de passe FTPS doivent être chiffrés dans la base de données
2. **Validation des certificats** : Implémenter une validation correcte des certificats SSL/TLS
3. **Sécurité des connexions** : Utiliser uniquement des connexions sécurisées par défaut
4. **Gestion des erreurs** : Ne pas exposer d'informations sensibles dans les logs
5. **Permissions** : Valider les permissions d'accès aux répertoires

## Limitations et contraintes

1. **Dépendance externe** : Nécessite FluentFTP comme dépendance
2. **Performance** : Les opérations FTPS peuvent être plus lentes que les protocoles existants
3. **Compatibilité** : Tous les serveurs FTPS ne sont pas identiques
4. **Gestion des erreurs** : Nécessite une gestion robuste des déconnexions réseau
5. **Sécurité** : Dépend de la configuration correcte des certificats SSL/TLS

Ce guide fournit une base complète pour l'implémentation d'un client FTPS dans Radarr, en respectant l'architecture existante et en ajoutant les fonctionnalités nécessaires de manière modulaire et testable.