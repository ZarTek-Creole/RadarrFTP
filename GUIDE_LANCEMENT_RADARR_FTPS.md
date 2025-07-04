# 🎯 Guide de Lancement Radarr avec Support FTPS

## ✅ **RADARR AVEC FTPS - PRÊT À LANCER**

Votre intégration FTPS est maintenant **complètement intégrée** dans le code source de Radarr ! Ce guide vous explique comment compiler et lancer Radarr avec le support FTPS.

---

## 🚀 **COMPILATION ET LANCEMENT RADARR**

### **Étape 1 : Prérequis**
```bash
# Vérifier .NET 6.0
dotnet --version

# Vérifier Node.js (pour le frontend)
node --version
npm --version
```

### **Étape 2 : Compilation du Backend**
```bash
# Compiler le core de Radarr
cd src
dotnet build NzbDrone.Core/Radarr.Core.csproj --configuration Release

# Compiler l'application principale
dotnet build Radarr.Host/Radarr.Host.csproj --configuration Release
```

### **Étape 3 : Compilation du Frontend (optionnel)**
```bash
# Si vous voulez recompiler le frontend
cd frontend
npm install
npm run build
cd ..
```

### **Étape 4 : Lancement de Radarr**
```bash
# Démarrer Radarr avec le support FTPS
cd src/Radarr.Host
dotnet run --configuration Release --urls=http://localhost:7878
```

---

## 🌐 **ACCÈS À RADARR**

### **URL d'accès :**
```
http://localhost:7878
```

### **Configuration initiale :**
1. **Premier démarrage** : Assistant de configuration
2. **Authentification** : Configurer admin/mot de passe (optionnel)
3. **Dossiers** : Configurer dossiers de films et téléchargements

---

## 📡 **CONFIGURATION DU CLIENT FTPS**

### **Étape 1 : Accéder aux Download Clients**
1. Aller dans **Settings** → **Download Clients**
2. Cliquer sur **+** pour ajouter un nouveau client
3. Sélectionner **FTPS** dans la liste

### **Étape 2 : Configuration FTPS**
```
📋 Paramètres principaux :
- Name: Mon Serveur FTPS
- Enable: ✅ Coché
- Host: test.rebex.net (pour test)
- Port: 21
- Username: demo
- Password: password

🔐 Paramètres SSL/TLS :
- Use SSL: ✅ Coché
- Encryption Mode: Explicit
- Validate Certificate: ❌ Décoché (pour test)

📁 Paramètres de chemin :
- Remote Base Path: /
- Category: movies

⚙️ Paramètres avancés :
- Priority: 1
- Timeout: 30
- Retry Attempts: 3
```

### **Étape 3 : Test de connexion**
1. Cliquer sur **Test** pour valider la configuration
2. Si succès : ✅ "Connection successful"
3. Cliquer sur **Save** pour enregistrer

---

## 🎬 **TEST AVEC UN FILM**

### **Ajouter un film pour test :**
1. Aller dans **Movies** → **Add New**
2. Rechercher un film (ex: "Inception")
3. Sélectionner le film
4. Choisir **Root Folder** et **Quality Profile**
5. **Search on add** : ✅ Coché
6. Cliquer sur **Add Movie**

### **Surveillance du téléchargement :**
1. Aller dans **Activity** → **Queue**
2. Voir le statut du téléchargement FTPS
3. Suivre le progrès en temps réel

---

## 🔧 **SCRIPTS DE LANCEMENT AUTOMATIQUE**

### **Script Linux/macOS :**
```bash
#!/bin/bash
# launch-radarr-ftps.sh

echo "🚀 Lancement Radarr avec support FTPS"
echo "====================================="

cd "$(dirname "$0")"

# Vérifier .NET
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 6.0 requis"
    exit 1
fi

# Compiler si nécessaire
echo "🔨 Compilation..."
cd src
dotnet build --configuration Release

# Lancer Radarr
echo "🎯 Démarrage de Radarr..."
cd Radarr.Host
dotnet run --configuration Release --urls=http://localhost:7878

echo "✅ Radarr accessible sur http://localhost:7878"
```

### **Script Windows PowerShell :**
```powershell
# launch-radarr-ftps.ps1

Write-Host "🚀 Lancement Radarr avec support FTPS" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Vérifier .NET
if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Host "❌ .NET 6.0 requis" -ForegroundColor Red
    exit 1
}

# Compiler si nécessaire
Write-Host "🔨 Compilation..." -ForegroundColor Blue
Set-Location src
dotnet build --configuration Release

# Lancer Radarr
Write-Host "🎯 Démarrage de Radarr..." -ForegroundColor Blue
Set-Location Radarr.Host
dotnet run --configuration Release --urls=http://localhost:7878

Write-Host "✅ Radarr accessible sur http://localhost:7878" -ForegroundColor Green
```

---

## 📋 **FONCTIONNALITÉS FTPS DISPONIBLES**

### ✅ **Fonctionnalités implémentées :**
- **✅ Connexion sécurisée** : SSL/TLS Explicit/Implicit
- **✅ Authentification** : Username/Password
- **✅ Détection automatique** : Scan des serveurs FTPS
- **✅ Parsing scene** : Reconnaissance formats warez
- **✅ Scoring intelligent** : Sélection de la meilleure release
- **✅ Téléchargement** : Avec progress et resume
- **✅ Monitoring** : Surveillance en temps réel
- **✅ Intégration complète** : Interface Radarr native

### 🎯 **Interface Radarr :**
- **Download Clients** : Configuration FTPS dans l'interface
- **Activity Queue** : Suivi des téléchargements FTPS
- **History** : Historique des téléchargements
- **Health Check** : Vérification de l'état des serveurs FTPS

---

## 🛠️ **DÉPANNAGE**

### **Problème : "FTPS not found in Download Clients"**
```bash
# Vérifier que Ftps.cs est compilé
ls src/NzbDrone.Core/Download/Clients/Ftps/

# Recompiler le core
cd src
dotnet clean
dotnet build NzbDrone.Core/Radarr.Core.csproj --configuration Release
```

### **Problème : Erreur de compilation**
```bash
# Vérifier les dépendances
dotnet restore

# Vérifier FluentFTP
grep -r "FluentFTP" src/NzbDrone.Core/Radarr.Core.csproj
```

### **Problème : Interface ne se charge pas**
```bash
# Vérifier le frontend
cd frontend
npm install
npm run build
```

---

## 🔍 **VÉRIFICATION DE L'INTÉGRATION**

### **Logs à surveiller :**
```bash
# Démarrage de Radarr
tail -f ~/.config/Radarr/logs/radarr.txt | grep -i ftps

# Rechercher "FTPS" dans les logs
grep -i "ftps" ~/.config/Radarr/logs/radarr.txt
```

### **Tests de validation :**
1. **✅ Interface** : Client FTPS visible dans Download Clients
2. **✅ Configuration** : Formulaire de configuration complet
3. **✅ Test** : Bouton "Test" fonctionne
4. **✅ Connexion** : Test avec Rebex réussi
5. **✅ Téléchargement** : Film ajouté déclenche recherche FTPS

---

## 📊 **MONITORING FTPS**

### **Dans l'interface Radarr :**
- **System → Status** : État des download clients
- **Activity → Queue** : Files d'attente FTPS
- **Activity → History** : Historique téléchargements
- **Settings → Download Clients** : Configuration FTPS

### **Métriques disponibles :**
- **Connexions actives** : Nombre de serveurs connectés
- **Vitesse téléchargement** : Débit en temps réel
- **Files d'attente** : Téléchargements en cours et en attente
- **Taux de succès** : Statistiques de réussite

---

## 🎉 **PROCHAINES ÉTAPES**

### **Une fois Radarr lancé :**
1. **Configuration** : Configurer votre serveur FTPS privé
2. **Test** : Ajouter un film pour tester le téléchargement
3. **Optimisation** : Ajuster les paramètres selon vos besoins
4. **Monitoring** : Surveiller les performances

### **Intégration avancée :**
- **Indexers** : Configurer des indexers compatibles
- **Quality Profiles** : Définir vos profils de qualité
- **Notifications** : Configurer les notifications
- **Auto-organisation** : Paramétrer le post-processing

---

## 📞 **SUPPORT**

### **En cas de problème :**
1. **Logs** : Consulter `~/.config/Radarr/logs/radarr.txt`
2. **Interface** : Vérifier System → Status
3. **Réseau** : Tester la connectivité FTPS
4. **Configuration** : Valider les paramètres serveur

### **Documentation :**
- **Configuration** : Interface Settings → Download Clients
- **Monitoring** : Interface Activity
- **Logs** : System → Logs

---

## 🎯 **RÉSUMÉ FINAL**

**✅ Radarr avec FTPS est prêt !**

Votre intégration FTPS est maintenant **complètement fonctionnelle** dans Radarr :
- **Compilation** : Code source intégré
- **Interface** : Configuration native
- **Fonctionnalités** : Détection et téléchargement automatiques
- **Monitoring** : Suivi en temps réel

**🚀 Lancez Radarr et configurez votre premier serveur FTPS !**

**URL d'accès : http://localhost:7878**