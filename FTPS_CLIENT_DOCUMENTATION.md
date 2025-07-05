# Client FTPS pour Radarr

## Vue d'ensemble

Le client FTPS permet à Radarr de télécharger directement des films depuis des serveurs FTPS (FTP sur SSL/TLS) sécurisés. Cette fonctionnalité est particulièrement utile pour accéder à des serveurs privés de la scène warez.

## Configuration

### Paramètres de connexion

#### Serveur
- **Host** : Nom d'hôte ou adresse IP du serveur FTPS
- **Port** : Port du serveur FTPS (par défaut : 21)
- **Username** : Nom d'utilisateur pour l'authentification
- **Password** : Mot de passe pour l'authentification

#### Sécurité
- **SSL/TLS Mode** : 
  - **Explicit (AUTH TLS)** : Connexion FTP standard qui passe en mode sécurisé (recommandé)
  - **Implicit (SSL)** : Connexion SSL depuis le début (port 990 généralement)
  - **None (Plain FTP)** : FTP non chiffré (non recommandé)
  
- **Connection Mode** :
  - **Passive** : Mode passif (recommandé pour la plupart des configurations avec pare-feu)
  - **Active** : Mode actif (pour des configurations réseau spécifiques)
  
- **Validate Certificate** : Valider le certificat SSL/TLS du serveur (recommandé)

#### Chemins
- **Base Path** : Chemin de base sur le serveur FTPS (ex: `/releases/`)
- **Movie Directory** : Sous-répertoire contenant les films (ex: `movies/`)

#### Avancé
- **Priority** : Priorité du serveur (1-100, 1 = priorité la plus haute)
- **Scan Interval** : Intervalle de scan en minutes pour détecter les nouveaux contenus

## Utilisation

### Ajout d'un client FTPS

1. Allez dans **Paramètres** > **Clients de Téléchargement**
2. Cliquez sur **Ajouter** (+)
3. Sélectionnez **FTPS Client** dans la section **FTPS**
4. Configurez les paramètres de connexion
5. Testez la connexion
6. Sauvegardez

### Détection automatique

Le client FTPS recherche automatiquement les films en utilisant :
- Les conventions de nommage de la scène (ex: `Film.2023.1080p.BluRay.x264-GROUP`)
- Les patterns de qualité (1080p, 720p, 4K, etc.)
- La correspondance des titres de films

### Téléchargement

Lorsqu'un film est trouvé :
1. Le système sélectionne automatiquement le meilleur fichier (basé sur la taille et la qualité)
2. Le fichier est téléchargé dans le répertoire de téléchargement configuré
3. Le post-traitement standard de Radarr s'applique (renommage, déplacement, etc.)

## Sécurité

### Recommandations

- **Utilisez toujours SSL/TLS** : Préférez le mode "Explicit (AUTH TLS)"
- **Validez les certificats** : Activez la validation des certificats SSL/TLS
- **Connexions privées** : Utilisez ce client uniquement avec des serveurs de confiance
- **Mots de passe forts** : Utilisez des mots de passe complexes

### Avertissements

⚠️ **Attention** : 
- Désactiver la validation des certificats expose aux attaques de l'homme du milieu
- L'utilisation de FTP non chiffré (mode "None") transmet les identifiants en clair
- Respectez les lois locales concernant le téléchargement de contenu

## Dépannage

### Problèmes de connexion

**Erreur : "Unable to connect to FTPS server"**
- Vérifiez l'adresse et le port du serveur
- Testez la connectivité réseau
- Vérifiez les paramètres de pare-feu

**Erreur : "Authentication failed"**
- Vérifiez le nom d'utilisateur et le mot de passe
- Vérifiez que le compte n'est pas verrouillé

### Problèmes SSL/TLS

**Erreur de certificat**
- Vérifiez la validité du certificat du serveur
- Essayez de désactiver temporairement la validation (non recommandé en production)

### Problèmes de téléchargement

**Films non trouvés**
- Vérifiez le chemin de base et le répertoire de films
- Vérifiez les conventions de nommage des fichiers sur le serveur
- Consultez les logs de Radarr pour plus de détails

**Téléchargements lents**
- Essayez le mode de connexion passif/actif alternatif
- Vérifiez la bande passante réseau

## Logs et debugging

### Activation des logs détaillés

1. Allez dans **Paramètres** > **Général**
2. Changez le **Niveau de log** à **Debug** ou **Trace**
3. Les logs FTPS apparaîtront dans **Système** > **Logs**

### Logs utiles

Les logs incluront :
- Tentatives de connexion FTPS
- Erreurs d'authentification
- Progression des téléchargements
- Erreurs de parsing des noms de fichiers

## Limitations

- **Performance** : FTPS peut être plus lent que BitTorrent/Usenet pour de gros fichiers
- **Compatibilité** : Tous les serveurs FTPS n'implémentent pas les mêmes fonctionnalités
- **Dépendance réseau** : Sensible aux coupures de connexion réseau
- **Gestion de reprises** : La reprise de téléchargement dépend du serveur

## Support technique

### Informations requises pour le support

En cas de problème, fournissez :
- Version de Radarr
- Configuration du client FTPS (sans les mots de passe)
- Logs détaillés de l'erreur
- Type et version du serveur FTPS

### Ressources

- [Documentation officielle de Radarr](https://wiki.servarr.com/radarr)
- [Forums de la communauté Radarr](https://forums.radarr.video/)
- [Documentation FluentFTP](https://github.com/robinrodricks/FluentFTP/wiki)