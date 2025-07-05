# 🎬 Guide de Test : Client FTPS pour Radarr

## 🚀 **Lancement de Radarr**

### Démarrage rapide
```bash
./start-radarr.sh
```

### Démarrage manuel
```bash
cd _output/net6.0/
./Radarr --host=0.0.0.0 --port=7878 --data=$HOME/.config/Radarr
```

## 🌐 **Accès à l'interface**

Une fois Radarr démarré, accédez à :
- **URL :** http://localhost:7878
- **Configuration initiale :** Suivez l'assistant de première configuration

## ⚙️ **Configuration du Client FTPS**

### 1. Accès aux Paramètres
1. Connectez-vous à l'interface web Radarr
2. Allez dans **Paramètres** > **Clients de téléchargement**
3. Cliquez sur **Ajouter** (+)
4. Sélectionnez **FTPS Client**

### 2. Configuration des Paramètres FTPS

#### Paramètres de connexion
- **Nom** : `Mon Serveur FTPS`
- **Hôte** : `ftp.exemple.com`
- **Port** : `21` (ou `990` pour FTPS Implicite)
- **Nom d'utilisateur** : votre nom d'utilisateur
- **Mot de passe** : votre mot de passe

#### Paramètres de sécurité
- **Mode de sécurité** :
  - `None` : FTP standard (non chiffré)
  - `Explicit` : FTPS Explicite (TLS)
  - `Implicit` : FTPS Implicite (SSL)

#### Paramètres de connexion
- **Mode de connexion** :
  - `Passive` : Recommandé (par défaut)
  - `Active` : Pour certaines configurations réseau
- **Valider certificat** : Décochez pour les tests avec certificats auto-signés

#### Chemins
- **Chemin de base** : `/films` ou `/movies`
- **Répertoire des films** : `releases` ou laissez vide

### 3. Test de Connexion
1. Cliquez sur **Tester** pour vérifier la connexion
2. Un message de succès devrait apparaître
3. Cliquez sur **Sauvegarder**

## 🧪 **Serveurs de Test Gratuits**

### Test.Rebex.net (FTP public)
```
Hôte: test.rebex.net
Port: 21
Utilisateur: demo
Mot de passe: password
Mode de sécurité: None
Chemin de base: /pub/example
```

### FileZilla Server Local
Si vous avez accès à un serveur FTP local :
```
Hôte: localhost
Port: 21
Mode de sécurité: None ou Explicit
```

## 📋 **Tests à Effectuer**

### ✅ Test 1 : Connexion de base
- [ ] Connexion réussie au serveur FTPS
- [ ] Test de connexion dans l'interface passe

### ✅ Test 2 : Navigation des répertoires
- [ ] Les répertoires s'affichent correctement
- [ ] Navigation dans les sous-dossiers

### ✅ Test 3 : Détection des fichiers
- [ ] Les fichiers vidéo sont détectés
- [ ] Les métadonnées sont correctement extraites

### ✅ Test 4 : Téléchargement
- [ ] Téléchargement d'un fichier test
- [ ] Progression visible dans l'interface
- [ ] Fichier téléchargé avec succès

## 🔧 **Fonctionnalités Implémentées**

### ✨ **Protocoles Supportés**
- **FTP** : Connexion non chiffrée standard
- **FTPS Explicite** : Connexion chiffrée avec TLS
- **FTPS Implicite** : Connexion SSL directe

### 📁 **Gestion des Fichiers**
- Détection automatique des films par regex
- Support des formats : 1080p, 720p, 480p, 2160p, 4K
- Sélection du meilleur fichier par taille
- Téléchargement avec reprise d'erreur

### 🛡️ **Sécurité**
- Validation de certificats SSL/TLS configurables
- Chiffrement des identifiants
- Support des connexions actives et passives

## 🐛 **Dépannage**

### Problème : Connexion échoue
- **Solution** : Vérifiez les paramètres réseau et pare-feu
- **Astuce** : Testez d'abord avec un client FTP standard

### Problème : Certificat SSL invalide
- **Solution** : Décochez "Valider certificat" pour les tests
- **Production** : Configurez un certificat valide

### Problème : Pas de fichiers détectés
- **Solution** : Vérifiez le chemin de base et les permissions
- **Format** : Assurez-vous que les noms suivent le format attendu

### Problème : Téléchargement lent
- **Solution** : Basculez entre modes Actif/Passif
- **Réseau** : Vérifiez la bande passante disponible

## 📞 **Support**

### Logs de débogage
Les logs sont disponibles dans :
```bash
$HOME/.config/Radarr/logs/
```

### Fichiers de configuration
```bash
$HOME/.config/Radarr/config.xml
```

## 🎯 **Limitations Actuelles**

- Recherche de films par regex simple
- Pas de support SFTP (protocole différent)
- Téléchargement séquentiel uniquement
- Cache local basique

## 🚀 **Évolutions Futures**

- Amélioration de la détection des films
- Support de la reprise de téléchargement
- Interface de gestion avancée
- Intégration avec indexeurs FTPS

---

**✅ Le client FTPS est maintenant prêt pour vos tests !**

Bon test ! 🎬✨