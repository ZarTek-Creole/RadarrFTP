# 🚀 Scripts de Build pour RadarrFTP

Ce dossier contient des scripts batch pour automatiser la compilation et la publication de RadarrFTP avec support FTPS.

## 📁 Scripts Disponibles

### 1. `build-radarr.bat` - Build Complet Automatique
**Usage :** Double-clic ou `build-radarr.bat`

**Ce qu'il fait :**
- ✅ Nettoie les builds précédents
- ✅ Restaure les packages NuGet
- ✅ Installe les dépendances frontend (yarn/npm)
- ✅ Compile le projet en mode Release
- ✅ Crée les exécutables pour toutes les plateformes :
  - Windows 64-bit (avec dépendances)
  - Linux 64-bit (avec dépendances)
  - macOS 64-bit (avec dépendances)
  - Windows single-file (fichier unique)

**Durée :** ~5-10 minutes selon votre machine

### 2. `dev-commands.bat` - Menu Interactif de Développement
**Usage :** Double-clic ou `dev-commands.bat`

**Menu disponible :**
```
[1] Clean - Nettoyer les fichiers de build
[2] Restore - Restaurer les packages NuGet
[3] Build - Compiler le projet (Debug)
[4] Build Release - Compiler le projet (Release)
[5] Test - Lancer les tests
[6] Publish Windows - Créer l'exécutable Windows
[7] Publish All - Créer tous les exécutables
[8] Full Build - Clean + Restore + Build + Publish
[9] Quick Test - Build rapide pour tester
[0] Quitter
```

**Idéal pour :** Développement et tests rapides

### 3. `quick-build.bat` - Build Rapide Windows
**Usage :** Double-clic ou `quick-build.bat`

**Ce qu'il fait :**
- ✅ Nettoyage rapide
- ✅ Restauration des packages
- ✅ Compilation Release
- ✅ Création de l'exécutable Windows uniquement

**Durée :** ~2-3 minutes

## 🎯 Quel Script Utiliser ?

| Situation | Script Recommandé | Durée |
|-----------|-------------------|-------|
| **Premier build** | `build-radarr.bat` | ~10 min |
| **Build pour distribution** | `build-radarr.bat` | ~10 min |
| **Test rapide** | `quick-build.bat` | ~3 min |
| **Développement** | `dev-commands.bat` | Variable |
| **Debug/Fix** | `dev-commands.bat` → option 3 | ~1 min |

## 📋 Prérequis

Avant d'utiliser ces scripts, assurez-vous d'avoir :

1. **.NET 6.0 SDK** installé
2. **Node.js et Yarn** (pour le frontend)
3. **Git** (optionnel, pour les métadonnées)

Vérification rapide :
```cmd
dotnet --version
yarn --version
```

## 📁 Structure des Résultats

Après un build complet, vous aurez :

```
RadarrFTP-develop/
├── RadarrFTP-Windows/     # Windows 64-bit + dépendances
│   └── Radarr.exe
├── RadarrFTP-Linux/       # Linux 64-bit + dépendances
│   └── Radarr
├── RadarrFTP-macOS/       # macOS 64-bit + dépendances
│   └── Radarr
└── RadarrFTP-Single/      # Windows fichier unique
    └── Radarr.exe
```

## 🚀 Démarrage Rapide

1. **Téléchargez** le projet RadarrFTP
2. **Placez** les scripts `.bat` dans le dossier racine
3. **Double-cliquez** sur `quick-build.bat` pour un premier test
4. **Lancez** `RadarrFTP-Windows\Radarr.exe`
5. **Ouvrez** http://localhost:7878 dans votre navigateur

## ⚙️ Configuration FTPS

Une fois Radarr démarré :

1. Allez dans **Settings** → **Download Clients**
2. Cliquez **Add** → **FTPS**
3. Configurez vos paramètres FTPS :
   - Host : votre serveur FTPS
   - Port : 21 (ou votre port)
   - Username/Password
   - SSL/TLS : Activé
   - Directory : dossier de téléchargement

## 🔧 Dépannage

### Erreur "dotnet command not found"
```cmd
# Installez .NET 6.0 SDK depuis :
# https://dotnet.microsoft.com/download/dotnet/6.0
```

### Erreur "yarn command not found"
```cmd
# Installez Node.js puis :
npm install -g yarn
```

### Erreur de compilation
```cmd
# Utilisez le menu de développement :
dev-commands.bat → option 1 (Clean) → option 2 (Restore) → option 4 (Build Release)
```

## 📞 Support

- **Erreurs de build :** Vérifiez les prérequis et utilisez `dev-commands.bat`
- **Problèmes FTPS :** Consultez la documentation Radarr
- **Performance :** Utilisez `quick-build.bat` pour les tests rapides

---

**🎉 Votre Radarr avec support FTPS est prêt !**

