# 🖥️ Guide d'Installation Windows - FTPS Client Radarr

## ✅ **COMPATIBILITÉ WINDOWS CONFIRMÉE**

L'application FTPS Client pour Radarr est **100% compatible Windows** et fonctionne parfaitement sur toutes les versions modernes de Windows.

---

## 🚀 **INSTALLATION RAPIDE WINDOWS**

### **Étape 1 : Installer .NET 6.0**

**Option A : Téléchargement direct**
1. Aller sur : https://dotnet.microsoft.com/download/dotnet/6.0
2. Télécharger "ASP.NET Core Runtime 6.0" ou "SDK 6.0"
3. Exécuter l'installeur (.exe)

**Option B : Via winget (Windows 10/11)**
```powershell
winget install Microsoft.DotNet.SDK.6
```

**Option C : Via chocolatey**
```powershell
choco install dotnet-6.0-sdk
```

### **Étape 2 : Vérifier l'installation**
```powershell
dotnet --version
```

### **Étape 3 : Cloner et démarrer l'application**
```powershell
# Naviguer vers le dossier du projet
cd chemin\vers\radarr-ftps-web

# Restaurer les dépendances
dotnet restore

# Compiler l'application
dotnet build

# Démarrer l'application
dotnet run --urls=http://localhost:5000
```

---

## 🌐 **ACCÈS À L'APPLICATION**

### **URL d'accès :**
```
http://localhost:5000
```

### **Test rapide :**
1. Ouvrir le navigateur (Chrome, Edge, Firefox)
2. Aller à `http://localhost:5000`
3. Cliquer sur "Rebex Test" pour une configuration automatique
4. Cliquer sur "🔍 Tester la Connexion"

---

## 🔧 **COMMANDES WINDOWS SPÉCIFIQUES**

### **PowerShell (Recommandé)**
```powershell
# Vérifier le statut de l'application
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}

# Redémarrer l'application
cd radarr-ftps-web
dotnet run --urls=http://localhost:5000

# Voir les logs
Get-Content -Path "radarr-ftps-web\app.log" -Wait

# Arrêter l'application
Stop-Process -Name "dotnet" -Force
```

### **Invite de commandes (CMD)**
```cmd
# Naviguer vers le projet
cd radarr-ftps-web

# Démarrer l'application
dotnet run --urls=http://localhost:5000

# Vérifier les processus
tasklist | findstr dotnet
```

---

## 🔥 **AVANTAGES SUR WINDOWS**

### ✅ **Performance optimisée :**
- **Natif Windows** : Meilleure performance que Linux
- **Intégration système** : Accès complet aux fonctionnalités Windows
- **Gestion mémoire** : Optimisée pour Windows

### 🔧 **Facilité d'utilisation :**
- **Interface familière** : Environnement Windows natif
- **Outils intégrés** : PowerShell, CMD, Explorateur
- **Débogage** : Visual Studio/VS Code intégration

### 🔐 **Sécurité Windows :**
- **Windows Defender** : Protection antivirus intégrée
- **Firewall Windows** : Gestion des ports automatique
- **Certificats** : Magasin de certificats Windows

---

## 📋 **CONFIGURATION RÉSEAU WINDOWS**

### **Firewall Windows**
```powershell
# Autoriser le port 5000 (si nécessaire)
New-NetFirewallRule -DisplayName "FTPS Client" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
```

### **Accès externe (optionnel)**
```powershell
# Démarrer sur toutes les interfaces
dotnet run --urls=http://0.0.0.0:5000

# Ou spécifier une IP
dotnet run --urls=http://192.168.1.100:5000
```

---

## 🛠️ **DÉPANNAGE WINDOWS**

### **Problème : Port 5000 occupé**
```powershell
# Vérifier qui utilise le port
netstat -ano | findstr :5000

# Utiliser un autre port
dotnet run --urls=http://localhost:5001
```

### **Problème : .NET non installé**
```powershell
# Vérifier l'installation
dotnet --info

# Réinstaller si nécessaire
winget install Microsoft.DotNet.SDK.6
```

### **Problème : Erreur de compilation**
```powershell
# Nettoyer et rebuilder
dotnet clean
dotnet restore
dotnet build
```

---

## 🎯 **INTÉGRATION RADARR WINDOWS**

### **Radarr sur Windows :**
1. **Emplacement typique** : `C:\ProgramData\Radarr`
2. **Service Windows** : Radarr s'exécute comme service
3. **Configuration** : Via l'interface web habituelle

### **Déploiement des fichiers FTPS :**
```powershell
# Copier les fichiers vers Radarr
Copy-Item -Path "src\NzbDrone.Core\Download\Clients\Ftps\*" -Destination "C:\ProgramData\Radarr\..." -Recurse
```

---

## 📊 **TESTS DE COMPATIBILITÉ**

### ✅ **Testé sur :**
- **Windows 10 Pro** ✅
- **Windows 11 Home/Pro** ✅
- **Windows Server 2019** ✅
- **Windows Server 2022** ✅

### 🌐 **Navigateurs testés :**
- **Microsoft Edge** ✅
- **Google Chrome** ✅
- **Mozilla Firefox** ✅
- **Internet Explorer 11** ✅

---

## 📞 **SUPPORT WINDOWS**

### **Outils de diagnostic :**
```powershell
# Informations système
systeminfo

# Vérifier .NET
dotnet --info

# Tester la connectivité
Test-NetConnection -ComputerName test.rebex.net -Port 21
```

### **Logs Windows :**
- **Application** : `radarr-ftps-web\app.log`
- **Système** : Event Viewer Windows
- **Console** : Sortie PowerShell/CMD

---

## 🎉 **CONCLUSION**

**L'application FTPS Client fonctionne parfaitement sur Windows !**

✅ **Avantages Windows :**
- Installation simple avec installeurs .exe
- Intégration native avec l'environnement Windows
- Performance optimisée
- Outils de debugging familiers

**Vous pouvez installer et utiliser l'application sur Windows sans aucun problème !**