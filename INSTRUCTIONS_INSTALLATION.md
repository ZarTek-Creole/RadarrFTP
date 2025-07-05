# 🎬 RADARR V4 AVEC SUPPORT FTPS - INSTRUCTIONS D'INSTALLATION

## 📦 CONTENU DU PACKAGE

Ce package contient une version complète de Radarr v4 avec intégration FTPS native :

- **Radarr.exe** - Application principale (Windows 64-bit)
- **FluentFTP.dll** - Client FTPS haute performance (v48.0.2)
- **Tous les runtimes .NET 6.0** - Fonctionnement autonome
- **Interface utilisateur complète** - Interface web intégrée
- **Scripts de build** - Pour recompiler si nécessaire

## 🚀 INSTALLATION RAPIDE

### 1. Extraction
```bash
# Extraire le contenu du ZIP
cd C:\Radarr_FTPS
# Décompresser Radarr_v4_FTPS_Windows.zip ici
```

### 2. Premier Lancement
```bash
# Double-cliquer sur Radarr.exe
# OU en ligne de commande :
.\Radarr.exe
```

### 3. Accès Interface Web
- **URL** : http://localhost:7878
- **Authentification** : Configurée au premier lancement
- **Clé API** : Générée automatiquement

## 🔧 CONFIGURATION FTPS

### 1. Ajouter un Indexer FTPS
```
Settings → Indexers → Add Indexer → "FTPS Indexer"
```

**Paramètres requis :**
- **Host** : adresse.serveur.ftps
- **Port** : 21 (standard) ou 990 (implicite)
- **Username** : votre_nom_utilisateur
- **Password** : votre_mot_de_passe
- **SSL Mode** : Explicit/Implicit selon votre serveur
- **Connection Mode** : Passive (recommandé)

### 2. Ajouter un Client FTPS
```
Settings → Download Clients → Add → "FTPS Client"
```

**Paramètres requis :**
- **Host** : même que l'indexer
- **Port** : même que l'indexer
- **Username/Password** : même que l'indexer
- **Remote Path** : /downloads/ (ou votre dossier)
- **SSL Mode** : même que l'indexer

## 📋 WORKFLOW COMPLET

### 1. Découverte Automatique
```
FTPS Indexer → Scan serveur → Détecte films disponibles
```

### 2. Sélection Intelligente
```
Radarr → Compare qualité → Sélectionne meilleure release
```

### 3. Téléchargement Sécurisé
```
FTPS Client → Télécharge via SSL/TLS → Vérifie intégrité
```

## 🔒 TYPES DE SERVEURS FTPS SUPPORTÉS

### ✅ Serveurs Compatibles
- **FileZilla Server** (Windows/Linux)
- **ProFTPD** avec mod_tls (Linux)
- **vsftpd** avec SSL (Linux)
- **Microsoft IIS** avec FTP SSL (Windows)
- **Pure-FTPd** avec TLS (Linux)

### 🛡️ Modes SSL/TLS
- **Explicit (Port 21)** - Démarrage non-sécurisé puis upgrade
- **Implicit (Port 990)** - Connexion SSL dès le départ
- **None** - FTP standard (non recommandé)

## 📁 GESTION DES FICHIERS

### Types de Fichiers Supportés
1. **Vidéos directes** : .mkv, .mp4, .avi, .mov, .wmv
2. **Archives** : .rar, .zip, .7z, .tar, .gz
3. **Multi-parts** : .r00, .r01, .r02, etc.

### Logique de Sélection
```
1. Priorité aux fichiers vidéo directs
2. Priorité aux archives (RAR principal pour multi-parts)
3. Sinon le plus gros fichier
```

## 🌐 PORTS ET FIREWALL

### Ports Requis
- **7878** : Interface web Radarr
- **21** : FTP standard (Explicit SSL)
- **990** : FTPS Implicit
- **Ports passifs** : 1024-65535 (selon config serveur)

### Configuration Firewall
```bash
# Windows Firewall
netsh advfirewall firewall add rule name="Radarr" dir=in action=allow protocol=TCP localport=7878
netsh advfirewall firewall add rule name="FTP" dir=in action=allow protocol=TCP localport=21
netsh advfirewall firewall add rule name="FTPS" dir=in action=allow protocol=TCP localport=990
```

## 🛠️ DÉPANNAGE

### Problèmes Fréquents

#### 1. Connexion FTPS Impossible
```
✅ Vérifier host/port/credentials
✅ Tester mode SSL (Explicit → Implicit)
✅ Vérifier firewall/proxy
✅ Tester avec client FTP standard
```

#### 2. Certificats SSL
```
✅ Désactiver validation certificats (test)
✅ Importer certificat serveur
✅ Vérifier date/heure système
```

#### 3. Mode Passif/Actif
```
✅ Essayer mode Passive si Active échoue
✅ Configurer range ports passifs serveur
✅ Vérifier NAT/routeur
```

## 🔍 LOGS ET DIAGNOSTIC

### Logs Radarr
```
%APPDATA%\Radarr\logs\
```

### Logs FTPS Détaillés
```
Settings → General → Log Level → Debug
```

### Test Connexion
```
Settings → Indexers → Test Connection
Settings → Download Clients → Test Connection
```

## 🎯 OPTIMISATIONS

### Performance
- **Threads** : 4-8 connexions simultanées
- **Timeout** : 30-60 secondes
- **Retry** : 3 tentatives automatiques

### Sécurité
- **SSL/TLS** : Toujours activé en production
- **Certificats** : Validation recommandée
- **Credentials** : Compte dédié avec permissions limitées

## 📞 SUPPORT

### En cas de problème :
1. **Vérifier logs** Radarr (niveau Debug)
2. **Tester connexion** avec client FTP classique
3. **Vérifier configuration** serveur FTPS
4. **Consulter documentation** serveur FTP

### Fichiers de configuration :
- **config.xml** : Configuration principale
- **logs/** : Fichiers de logs
- **backups/** : Sauvegardes automatiques

---

## 🏆 FONCTIONNALITÉS AVANCÉES

### 1. Indexation Intelligente
- Scan automatique toutes les heures
- Détection nouveaux films
- Parsing intelligent des noms

### 2. Gestion des Duplicatas
- Évite les téléchargements en double
- Compare qualité/taille
- Mise à jour automatique

### 3. Intégration Complète
- API REST complète
- Interface web moderne
- Compatible avec applications mobiles

---

**Version** : Radarr v4 with FTPS Integration
**Build** : Windows x64 Self-Contained
**Taille** : 98 MB compressé / 228 MB décompressé
**Compatibilité** : Windows 7/8/10/11 (64-bit)