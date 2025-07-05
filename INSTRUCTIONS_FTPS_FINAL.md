# 🎬 RADARR V4 AVEC INTÉGRATION FTPS - PACKAGE FINAL

## 🚨 IMPORTANT : NOUVEAU BUILD COMPLET AVEC TOUTES LES CORRECTIONS

Ce package contient un **build complètement nouveau** avec toutes les modifications FTPS intégrées et les corrections de l'interface utilisateur.

---

## 📦 PACKAGE FINAL

**Nom du fichier** : `Radarr_v4_FTPS_Windows_FINAL.zip` (73 MB)

**Contenu** :
- ✅ **Application Radarr complète** avec intégration FTPS native
- ✅ **FluentFTP v48.0.2** intégré
- ✅ **Interface utilisateur complète** (React/TypeScript)
- ✅ **Runtime .NET 6.0** self-contained
- ✅ **Script de lancement** Radarr.bat

---

## 🚀 INSTALLATION ET UTILISATION

### 1. **Extraction**
```bash
# Extraire le ZIP complet
# Dossier recommandé: C:\Radarr_FTPS\
```

### 2. **Lancement**
```bash
# Double-cliquer sur: Radarr.bat
# OU manuellement: dotnet Radarr.Host.dll
```

### 3. **Accès à l'interface**
- **URL** : http://localhost:7878
- **Interface complète** avec FTPS intégré

---

## 🎯 OÙ TROUVER LES OPTIONS FTPS

### **INDEXER FTPS** (Découverte des films)
```
http://localhost:7878
→ Settings (menu gauche)
→ Indexers
→ Add Indexer (bouton +)
→ Chercher: "FTPS Indexer"
```

### **CLIENT FTPS** (Téléchargement des films)
```
http://localhost:7878
→ Settings (menu gauche)
→ Download Clients
→ Add Download Client (bouton +)
→ Chercher: "FTPS Client"
```

---

## 🔧 CONFIGURATION FTPS

### **Configuration de l'Indexer FTPS**
- **Host** : votre-serveur-ftps.com
- **Port** : 21 (ou 990 pour FTPS implicite)
- **Username** : votre nom d'utilisateur
- **Password** : votre mot de passe
- **Movie Directory** : dossier contenant les films (ex: /movies)
- **Security Mode** : Explicit/Implicit/None
- **Connection Mode** : Passive/Active

### **Configuration du Client FTPS**
- **Host** : même serveur que l'indexer
- **Port** : même port que l'indexer
- **Username** : même utilisateur
- **Password** : même mot de passe
- **Movie Category** : catégorie pour les films
- **Security Mode** : même mode que l'indexer
- **Connection Mode** : même mode que l'indexer

---

## ✅ VERIFICATION APRÈS INSTALLATION

1. **Ouvrir** http://localhost:7878
2. **Aller dans** Settings → Indexers
3. **Cliquer** Add Indexer (+)
4. **Vérifier** que "FTPS Indexer" apparaît dans la liste
5. **Aller dans** Settings → Download Clients
6. **Cliquer** Add Download Client (+)
7. **Vérifier** que "FTPS Client" apparaît dans la liste

---

## 🆘 DÉPANNAGE

### Si FTPS n'apparaît pas dans l'interface :

1. **Fermer complètement** Radarr (CTRL+C dans la console)
2. **Relancer** avec Radarr.bat
3. **Attendre** 30 secondes pour le démarrage complet
4. **Recharger** le navigateur avec CTRL+F5
5. **Vérifier** que vous utilisez le **bon package** (73 MB)

### Si problème de dépendances :

- **Installer .NET 6.0 Runtime** : https://dotnet.microsoft.com/download/dotnet/6.0
- **Utiliser le script** Radarr.bat (contient dotnet)

---

## 🎉 FONCTIONNALITÉS FTPS INTÉGRÉES

### **Indexer FTPS**
- ✅ Scan automatique des serveurs FTPS
- ✅ Découverte intelligente des films
- ✅ Parsing des noms de films et qualité
- ✅ Support SSL/TLS (Explicit/Implicit)
- ✅ Modes Passif/Actif

### **Client FTPS**
- ✅ Téléchargement via FTPS sécurisé
- ✅ Gestion des archives (RAR, ZIP, 7Z)
- ✅ Sélection intelligente des fichiers
- ✅ Retry automatique en cas d'erreur
- ✅ Validation des certificats (optionnelle)

---

## 📋 RÉSUMÉ DES CORRECTIONS

**Build final avec toutes les corrections :**
1. ✅ **FtpsIndexer** - Apparaît maintenant dans Add Indexer
2. ✅ **FtpsClient** - Apparaît maintenant dans Add Download Client
3. ✅ **Interface UI** - Complète avec tous les fichiers statiques
4. ✅ **Auto-discovery** - Services automatiquement enregistrés
5. ✅ **FluentFTP** - Version 48.0.2 intégrée
6. ✅ **Dépendances** - Toutes les DLLs incluses
7. ✅ **Script de lancement** - Radarr.bat pour simplicité

**Ce package final résout tous les problèmes précédents !**