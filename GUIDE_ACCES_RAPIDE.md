# 🚀 GUIDE D'ACCÈS RAPIDE - Application FTPS Client Test

## ✅ **APPLICATION PRÊTE À L'UTILISATION !**

Votre application web de test FTPS Client pour Radarr est maintenant **déployée et accessible** !

---

## 🌐 **ACCÈS À L'APPLICATION**

### 🔗 **URL d'accès :**
```
http://localhost:5000
```

### 📱 **Interface disponible :**
- **Page d'accueil** : Configuration et test des connexions FTPS
- **Page "À propos"** : Informations détaillées sur l'application

---

## 🚀 **DÉMARRAGE RAPIDE**

### 1. **Accès à l'application**
Ouvrez votre navigateur et allez à : `http://localhost:5000`

### 2. **Test avec serveur public**
Pour un test rapide, cliquez sur le bouton **"Rebex Test"** qui configure automatiquement :
- **Serveur** : test.rebex.net
- **Port** : 21
- **Utilisateur** : demo
- **Mot de passe** : password
- **SSL** : Activé (Explicit FTPS)

### 3. **Lancer le test**
Cliquez sur **"🔍 Tester la Connexion"** pour valider la configuration.

---

## 🔧 **FONCTIONNALITÉS TESTÉES**

### ✅ **Tests de connexion :**
- ✅ Connexion FTPS SSL/TLS
- ✅ Authentification sécurisée
- ✅ Listing des fichiers distants
- ✅ Détection des releases scene
- ✅ Scoring automatique des releases

### 📊 **Résultats affichés :**
- **Statut de connexion** : Succès/Échec avec temps de réponse
- **Détails techniques** : Type de serveur, mode chiffrement, etc.
- **Fichiers trouvés** : Liste des fichiers sur le serveur
- **Releases détectées** : Films avec scoring automatique

---

## 🎯 **SERVEURS DE TEST DISPONIBLES**

### 🟢 **Serveur Rebex (Recommandé)**
- **Host** : test.rebex.net
- **Port** : 21
- **User** : demo
- **Pass** : password
- **SSL** : Explicit FTPS

### 🟡 **Serveur FileZilla**
- **Host** : demo.filezilla-project.org
- **Port** : 21
- **User** : demo
- **Pass** : demo
- **SSL** : Explicit FTPS

---

## 🔧 **COMMANDES UTILES**

### **Vérifier le statut de l'application**
```bash
ps aux | grep dotnet
```

### **Redémarrer l'application**
```bash
cd radarr-ftps-web
dotnet run --urls=http://0.0.0.0:5000
```

### **Voir les logs**
```bash
cd radarr-ftps-web
tail -f app.log
```

### **Arrêter l'application**
```bash
pkill -f "dotnet.*RadarrFtpsWeb"
```

---

## 📋 **SCÉNARIOS DE TEST**

### 🧪 **Test Basique**
1. Utilisez le preset "Rebex Test"
2. Cliquez sur "Tester la Connexion"
3. Vérifiez la connexion réussie

### 🔍 **Test Avancé**
1. Configurez manuellement un serveur FTPS
2. Testez différents modes de chiffrement
3. Vérifiez la détection des releases

### 🚨 **Test d'Erreur**
1. Utilisez des identifiants incorrects
2. Testez un serveur inexistant
3. Vérifiez la gestion des erreurs

---

## 🛠️ **DÉPANNAGE**

### **L'application ne démarre pas**
```bash
cd radarr-ftps-web
dotnet build
dotnet run --urls=http://0.0.0.0:5000
```

### **Port 5000 déjà utilisé**
```bash
# Utiliser un autre port
dotnet run --urls=http://0.0.0.0:5001
```

### **Erreur de connexion FTPS**
- Vérifiez les paramètres de connexion
- Testez avec "Validation certificat" désactivée
- Essayez le mode "Implicit" si "Explicit" ne fonctionne pas

---

## 🎉 **PROCHAINES ÉTAPES**

Une fois les tests validés avec cette application, vous pourrez :

1. **Intégrer dans Radarr** : Copier les composants dans le code source Radarr
2. **Tests unitaires** : Exécuter la suite de tests complète
3. **Déploiement** : Installer sur votre serveur Radarr de production

---

## 📞 **SUPPORT**

Si vous rencontrez des problèmes :
1. Consultez les logs : `tail -f radarr-ftps-web/app.log`
2. Vérifiez la configuration réseau
3. Testez avec les serveurs publics d'abord

**L'application est maintenant prête à l'utilisation ! 🎯**