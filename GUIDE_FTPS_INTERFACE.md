# 🔍 GUIDE : OÙ TROUVER LES OPTIONS FTPS DANS RADARR

## 🚨 IMPORTANT : UTILISEZ LE BON PACKAGE !

**Vous devez utiliser :** `Radarr_v4_FTPS_Windows_Complete.zip` (107 MB)
❌ **PAS** l'ancien ZIP de 98 MB

---

## 📍 LOCALISATION EXACTE DES OPTIONS FTPS

### 1️⃣ INDEXER FTPS (Pour découvrir les films)

```
🏠 Page d'accueil Radarr (http://localhost:7878)
    ↓
⚙️ Settings (dans le menu de gauche)
    ↓
📋 Indexers (dans le menu Settings)
    ↓
➕ Add Indexer (bouton + en haut à droite)
    ↓
🔍 Chercher dans la liste : "FTPS Indexer"
```

### 2️⃣ CLIENT FTPS (Pour télécharger les films)

```
🏠 Page d'accueil Radarr (http://localhost:7878)
    ↓
⚙️ Settings (dans le menu de gauche)
    ↓
⬇️ Download Clients (dans le menu Settings)
    ↓
➕ Add Download Client (bouton + en haut à droite)
    ↓
🔍 Chercher dans la liste : "FTPS Client"
```

---

## 🔧 SI VOUS NE VOYEZ PAS LES OPTIONS FTPS

### ✅ Solution 1 : Redémarrer l'application
1. Fermer complètement `Radarr.exe`
2. Attendre 10 secondes
3. Relancer `Radarr.exe`
4. Attendre 30 secondes le démarrage complet
5. Rafraîchir le navigateur (CTRL+F5)

### ✅ Solution 2 : Vider le cache
1. Dans votre navigateur : CTRL+SHIFT+DELETE
2. Cocher "Images et fichiers en cache"
3. Cliquer "Effacer les données"
4. Recharger Radarr : http://localhost:7878

### ✅ Solution 3 : Navigation privée
1. Ouvrir une fenêtre privée/incognito
2. Aller sur http://localhost:7878
3. Vérifier si FTPS apparaît

### ✅ Solution 4 : Vérifier les logs
1. Dans Radarr : System → Logs
2. Chercher "FTPS" ou "FtpsIndexer" 
3. Vérifier qu'il n'y a pas d'erreurs

---

## 🎯 À QUOI RESSEMBLE L'INTERFACE

### Indexer FTPS
```
Nom : FTPS Indexer
Description : Indexer for FTPS servers
Implementation : FtpsIndexer
Protocol : FTPS
```

### Client FTPS  
```
Nom : FTPS Client
Description : Download client for FTPS servers
Implementation : FtpsClient
Protocol : FTPS
```

---

## 🔧 CONFIGURATION RAPIDE

### Paramètres Indexer FTPS
- **Host** : `votre-serveur.com`
- **Port** : `21` (ou `990` pour FTPS implicite)
- **Username** : `votre_utilisateur`
- **Password** : `votre_mot_de_passe`
- **Movie Directory** : `movies` (dossier des films)
- **Security Mode** : `Explicit` ou `Implicit`
- **Connection Mode** : `Passive` (recommandé)

### Paramètres Client FTPS
- **Host** : même que l'indexer
- **Port** : même que l'indexer  
- **Username** : même que l'indexer
- **Password** : même que l'indexer
- **Remote Path** : `/downloads/` (dossier de téléchargement)
- **Security Mode** : même que l'indexer

---

## 🚀 TEST DE CONNEXION

1. Configurez l'indexer FTPS
2. Cliquez "Test Connection"
3. ✅ Doit afficher "Connection successful"
4. Sauvegardez
5. Répétez pour le client FTPS

---

## 📞 AIDE SUPPLÉMENTAIRE

Si FTPS n'apparaît toujours pas :

1. **Vérifiez la version** : Doit être `Radarr_v4_FTPS_Windows_Complete.zip`
2. **Vérifiez les logs** : System → Logs → Chercher "FTPS"
3. **Redémarrez** complètement l'application
4. **Port** : Vérifiez que le port 7878 n'est pas bloqué

**L'interface FTPS est intégrée nativement et doit apparaître automatiquement !**