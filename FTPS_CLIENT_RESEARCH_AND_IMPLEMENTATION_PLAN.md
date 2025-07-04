# Développement d'un Client de Téléchargement FTPS pour Radarr
## Recherche et Plan d'Implémentation Détaillé

### 📋 Résumé Exécutif

Ce document présente l'analyse complète de l'architecture Radarr et le plan d'implémentation pour intégrer un client de téléchargement FTPS (FTP over TLS/SSL) natif. L'objectif est de permettre l'acquisition directe de films depuis des serveurs FTPS privés de la scène warez, en remplacement des méthodes traditionnelles basées sur Usenet et BitTorrent.

### 🏗️ Architecture Existante de Radarr

#### Structure des Clients de Téléchargement

**Composants Principaux Identifiés :**

1. **Interface Core** : `IDownloadClient` (`src/NzbDrone.Core/Download/IDownloadClient.cs`)
   - Définit le contrat pour tous les clients de téléchargement
   - Propriétés requises : `Protocol`, méthodes `Download()`, `GetItems()`, `RemoveItem()`, etc.

2. **Classe de Base** : `DownloadClientBase<TSettings>` (`src/NzbDrone.Core/Download/DownloadClientBase.cs`)
   - Implémentation de base avec gestion des erreurs, logging, validation
   - Gestion des timeouts, reconnexions automatiques via `ResiliencePipeline`

3. **Protocoles Supportés** : `DownloadProtocol` enum (`src/NzbDrone.Core/Indexers/DownloadProtocol.cs`)
   ```csharp
   public enum DownloadProtocol
   {
       Unknown = 0,
       Usenet = 1,
       Torrent = 2
       // FTPS sera ajouté ici (3)
   }
   ```

4. **Factory Pattern** : `DownloadClientFactory` gère l'instanciation et la découverte des clients

#### Clients Existants Analysés

**Structure Type (ex: Sabnzbd)** :
- `Sabnzbd.cs` - Implémentation principale héritant de `UsenetClientBase<SabnzbdSettings>`
- `SabnzbdSettings.cs` - Configuration avec validation FluentValidation
- `SabnzbdProxy.cs` - Couche d'abstraction pour les communications réseau
- Modèles de données spécifiques (`SabnzbdQueue`, `SabnzbdHistoryItem`, etc.)

### 🎯 Plan d'Implémentation FTPS

#### Phase 1 : Fondations Core

**1.1 Extension de l'Enum DownloadProtocol**
```csharp
// src/NzbDrone.Core/Indexers/DownloadProtocol.cs
public enum DownloadProtocol
{
    Unknown = 0,
    Usenet = 1,
    Torrent = 2,
    Ftps = 3  // NOUVEAU
}
```

**1.2 Création de la Structure FTPS**
```
src/NzbDrone.Core/Download/Clients/Ftps/
├── Ftps.cs                      // Client principal
├── FtpsSettings.cs              // Configuration avec validation
├── FtpsProxy.cs                 // Couche FluentFTP
├── FtpsDownloadStatus.cs        // États de téléchargement
├── FtpsItem.cs                  // Modèle d'item FTPS
├── FtpsServerInfo.cs            // Informations serveur
└── FtpsClientException.cs       // Exceptions spécifiques
```

#### Phase 2 : Implémentation Core

**2.1 FtpsSettings - Configuration Serveur**
```csharp
public class FtpsSettings : DownloadClientSettingsBase<FtpsSettings>
{
    [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
    public string Host { get; set; } = "localhost";

    [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
    public int Port { get; set; } = 21;

    [FieldDefinition(2, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
    public string Username { get; set; }

    [FieldDefinition(3, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
    public string Password { get; set; }

    [FieldDefinition(4, Label = "SSL/TLS Mode", Type = FieldType.Select, SelectOptions = typeof(FtpEncryptionMode))]
    public int EncryptionMode { get; set; } = (int)FtpEncryptionMode.Explicit;

    [FieldDefinition(5, Label = "Validate Certificate", Type = FieldType.Checkbox)]
    public bool ValidateCertificate { get; set; } = true;

    [FieldDefinition(6, Label = "Transfer Mode", Type = FieldType.Select, SelectOptions = typeof(FtpDataConnectionType))]
    public int DataConnectionType { get; set; } = (int)FtpDataConnectionType.AutoPassive;

    [FieldDefinition(7, Label = "Remote Base Path", Type = FieldType.Textbox)]
    public string RemoteBasePath { get; set; } = "/releases/movies/";

    [FieldDefinition(8, Label = "Priority", Type = FieldType.Textbox)]
    public int Priority { get; set; } = 1;

    [FieldDefinition(9, Label = "Connection Timeout (seconds)", Type = FieldType.Textbox)]
    public int ConnectionTimeout { get; set; } = 30;
}
```

**2.2 FtpsProxy - Abstraction FluentFTP**
```csharp
public interface IFtpsProxy
{
    Task<bool> TestConnectionAsync(FtpsSettings settings);
    Task<IEnumerable<FtpListItem>> GetDirectoryListingAsync(string path, FtpsSettings settings);
    Task<bool> DownloadFileAsync(string remotePath, string localPath, FtpsSettings settings, 
                                 IProgress<FtpProgress> progress = null);
    Task<bool> FileExistsAsync(string remotePath, FtpsSettings settings);
    Task<long> GetFileSizeAsync(string remotePath, FtpsSettings settings);
    Task<DateTime> GetModifiedTimeAsync(string remotePath, FtpsSettings settings);
}

public class FtpsProxy : IFtpsProxy
{
    private readonly Logger _logger;

    private async Task<AsyncFtpClient> CreateClientAsync(FtpsSettings settings)
    {
        var client = new AsyncFtpClient(settings.Host, settings.Port, 
                                       settings.Username, settings.Password);
        
        client.Config.EncryptionMode = (FtpEncryptionMode)settings.EncryptionMode;
        client.Config.DataConnectionType = (FtpDataConnectionType)settings.DataConnectionType;
        client.Config.ValidateAnyCertificate = !settings.ValidateCertificate;
        client.Config.ConnectTimeout = TimeSpan.FromSeconds(settings.ConnectionTimeout);
        
        await client.AutoConnect();
        return client;
    }
    
    // Implémentations des méthodes...
}
```

**2.3 Ftps - Client Principal**
```csharp
public class Ftps : DownloadClientBase<FtpsSettings>
{
    private readonly IFtpsProxy _proxy;
    private readonly IMovieService _movieService;

    public override string Name => "FTPS";
    public override DownloadProtocol Protocol => DownloadProtocol.Ftps;

    public override async Task<string> Download(RemoteMovie remoteMovie, IIndexer indexer)
    {
        var releases = await ScanForMovieReleases(remoteMovie);
        var bestRelease = SelectBestRelease(releases, remoteMovie);
        
        if (bestRelease == null)
            throw new DownloadClientException("No suitable release found on FTPS server");

        var downloadId = Guid.NewGuid().ToString();
        await InitiateDownload(bestRelease, downloadId);
        
        return downloadId;
    }

    private async Task<IEnumerable<FtpsReleaseItem>> ScanForMovieReleases(RemoteMovie remoteMovie)
    {
        var releases = new List<FtpsReleaseItem>();
        var searchPaths = GenerateSearchPaths(remoteMovie);

        foreach (var path in searchPaths)
        {
            try
            {
                var items = await _proxy.GetDirectoryListingAsync(path, Settings);
                var movieReleases = items.Where(item => IsMovieRelease(item, remoteMovie))
                                         .Select(item => new FtpsReleaseItem(item, path));
                releases.AddRange(movieReleases);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to scan path: {0}", path);
            }
        }

        return releases;
    }

    private FtpsReleaseItem SelectBestRelease(IEnumerable<FtpsReleaseItem> releases, RemoteMovie remoteMovie)
    {
        // Logique de sélection basée sur :
        // - Profil de qualité de Radarr
        // - Formats personnalisés
        // - Taille du fichier
        // - Conventions de nommage de la scène
        return releases.OrderByDescending(r => CalculateReleaseScore(r, remoteMovie))
                      .FirstOrDefault();
    }
}
```

#### Phase 3 : Intégration Frontend

**3.1 API Extensions**
```csharp
// src/Radarr.Api.V3/DownloadClient/DownloadClientResource.cs
// Ajout des propriétés FTPS dans le mapping des ressources

// src/Radarr.Api.V3/DownloadClient/DownloadClientController.cs
// Extensions pour gérer les requêtes de test de connexion FTPS
```

**3.2 Frontend React/Angular (Structure supposée)**
```
frontend/src/Settings/DownloadClients/Ftps/
├── FtpsSettings.js
├── FtpsSettings.css
└── index.js
```

#### Phase 4 : Fonctionnalités Avancées

**4.1 Détection Intelligente de Releases**
- Pattern matching pour les conventions de nommage de la scène
- Intégration avec les capacités de parsing existantes de Radarr
- Support des formats multi-fichiers (archives, sous-titres, etc.)

**4.2 Gestion des Téléchargements**
- Queue de téléchargement avec priorités
- Reprise automatique des téléchargements interrompus
- Vérification d'intégrité (SFV, hashes)

**4.3 Monitoring et Surveillance**
- Surveillance périodique des serveurs FTPS
- Détection automatique de nouveaux contenus
- Notifications en cas d'indisponibilité

### 🛠️ Intégration FluentFTP

#### Avantages de FluentFTP pour ce Projet

1. **Support FTPS Complet** : TLS 1.3, certificats clients, modes explicite/implicite
2. **Performance Optimisée** : Transferts asynchrones, reprise de téléchargement
3. **Robustesse** : Gestion automatique des reconnexions, retry logic
4. **Compatibilité Serveur** : Support de 30+ types de serveurs FTP
5. **API Moderne** : async/await, cancellation tokens, progress tracking

#### Configuration FluentFTP Recommandée

```csharp
var client = new AsyncFtpClient(host, port, username, password);

// Sécurité renforcée
client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
client.Config.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
client.Config.DataConnectionEncryption = true;

// Performance optimisée
client.Config.TransferChunkSize = 1024 * 1024; // 1MB chunks
client.Config.RetryAttempts = 3;
client.Config.ConnectTimeout = TimeSpan.FromSeconds(30);

// Compatibilité serveur
client.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;
```

### 🔒 Considérations de Sécurité

#### Stockage des Identifiants
- Chiffrement des mots de passe dans la base de données Radarr
- Support des variables d'environnement pour les déploiements Docker
- Rotation automatique des connexions

#### Validation des Certificats
- Option stricte par défaut avec validation complète
- Mode "développement" avec avertissements clairs
- Support des certificats auto-signés pour les environnements de test

#### Protection contre les Attaques
- Validation des chemins contre les traversées de répertoire
- Limitation du taux de téléchargement
- Timeout configurable pour éviter les connexions bloquées

### 📊 Tests et Validation

#### Tests Unitaires
```csharp
// src/NzbDrone.Core.Test/Download/Clients/Ftps/
├── FtpsFixture.cs
├── FtpsProxyFixture.cs
└── FtpsSettingsFixture.cs
```

#### Tests d'Intégration
- Configuration de serveurs FTPS de test avec Docker
- Validation contre différents types de serveurs
- Tests de performance avec fichiers volumineux

#### Serveurs de Test Recommandés
- **ProFTPD** avec mod_tls
- **Pure-FTPd** avec TLS
- **vsftpd** avec SSL
- **FileZilla Server** pour Windows

### 🚀 Plan de Déploiement

#### Phase 1 : Prototype (4-6 semaines)
- [ ] Implémentation basique du client FTPS
- [ ] Interface de configuration minimale
- [ ] Tests avec serveur FTPS local

#### Phase 2 : Intégration (6-8 semaines)
- [ ] Intégration complète avec l'architecture Radarr
- [ ] Interface utilisateur complète
- [ ] Tests d'intégration extensifs

#### Phase 3 : Optimisation (4-6 semaines)
- [ ] Optimisations de performance
- [ ] Fonctionnalités avancées (surveillance, retry logic)
- [ ] Documentation utilisateur

#### Phase 4 : Production (2-4 semaines)
- [ ] Tests bêta avec utilisateurs pilotes
- [ ] Corrections de bugs et polissage
- [ ] Release officielle

### 📝 Fichiers à Créer/Modifier

#### Nouveaux Fichiers
```
src/NzbDrone.Core/Download/Clients/Ftps/
├── Ftps.cs
├── FtpsSettings.cs
├── FtpsProxy.cs
├── FtpsDownloadStatus.cs
├── FtpsItem.cs
├── FtpsServerInfo.cs
├── FtpsClientException.cs
└── FtpsReleaseItem.cs

src/NzbDrone.Core.Test/Download/Clients/Ftps/
├── FtpsFixture.cs
├── FtpsProxyFixture.cs
└── FtpsSettingsFixture.cs

frontend/src/Settings/DownloadClients/Ftps/
├── FtpsSettings.js
├── FtpsSettings.css
└── index.js
```

#### Fichiers à Modifier
```
src/NzbDrone.Core/Indexers/DownloadProtocol.cs
src/Radarr.Api.V3/DownloadClient/DownloadClientResource.cs
src/NzbDrone.Core/Download/DownloadClientFactory.cs
src/NzbDrone.Core/Download/DownloadClientProvider.cs
```

### 🔧 Dépendances Requises

#### NuGet Packages
```xml
<PackageReference Include="FluentFTP" Version="48.0.2" />
<PackageReference Include="FluentValidation" Version="11.0.0" />
```

#### Injection de Dépendances
```csharp
// Registration dans le conteneur IoC de Radarr
services.AddTransient<IFtpsProxy, FtpsProxy>();
services.AddTransient<Ftps>();
```

### 📈 Métriques de Succès

#### Critères d'Acceptation
- [ ] Connexion réussie à différents types de serveurs FTPS
- [ ] Détection automatique de releases de films
- [ ] Téléchargement complet sans corruption de données
- [ ] Intégration transparente avec le workflow Radarr existant
- [ ] Interface utilisateur intuitive et sans erreurs

#### KPIs de Performance
- Temps de connexion FTPS < 5 secondes
- Débit de téléchargement > 80% de la bande passante disponible
- Taux de réussite des téléchargements > 95%
- Détection de nouveaux contenus < 30 minutes

### 🎯 Prochaines Étapes Immédiates

1. **Validation du Concept** : Créer un prototype minimal avec FluentFTP
2. **Configuration de l'Environnement** : Mettre en place serveurs FTPS de test
3. **Développement Itératif** : Implémenter fonctionnalité par fonctionnalité
4. **Tests Continus** : Validation à chaque étape du développement

### 💡 Notes d'Implémentation

#### Conventions de Nommage de la Scène
Le client FTPS devra reconnaître et analyser les patterns typiques :
- `Movie.Title.Year.Quality.Source.Codec-Group`
- `Movie.Title.Year.COMPLETE.BluRay-Group`
- `Movie.Title.Year.UHD.BluRay.2160p.CODEC-Group`

#### Gestion Multi-Serveurs
- Support de plusieurs serveurs FTPS avec priorités
- Load balancing et failover automatique
- Synchronisation des statuts entre serveurs

Ce plan d'implémentation fournit une feuille de route complète pour intégrer un client FTPS robuste et sécurisé dans Radarr, permettant une acquisition directe et efficace de contenu depuis des serveurs FTPS privés de la scène warez.