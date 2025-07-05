# 🧪 Guide Complet de Test FTPS dans Radarr

## 🎯 **MÉTHODES DE TEST DISPONIBLES**

### **1. Tests Unitaires (Développement)**
### **2. Tests d'Interface (UI)**  
### **3. Tests API (Programmation)**
### **4. Tests avec Serveur FTPS Réel**
### **5. Tests avec Serveur FTPS Local**

---

## 🔧 **1. TESTS UNITAIRES**

### **Exécuter les Tests Existants**
```bash
cd /workspace/src
dotnet test --filter "Ftps" --verbosity normal
```

### **Tests Spécifiques Disponibles**
```bash
# Tests du Client FTPS
dotnet test --filter "FtpsClientFixture" 

# Tests de l'Indexer FTPS
dotnet test --filter "FtpsIndexerFixture"

# Tous les tests FTPS
dotnet test --filter "ClassName~Ftps"
```

### **Vérifier les Résultats**
```bash
# Les tests doivent être ✅ PASSED
# Format de sortie :
Test Run Successful.
Total tests: X
     Passed: X
     Failed: 0
     Skipped: 0
```

---

## 🖥️ **2. TESTS INTERFACE UTILISATEUR**

### **Accès à l'Interface**
```
URL: http://localhost:7878
API Key: 36a5d1e3a99a46358954df8874aa05e5
```

### **Test 1: Ajouter un Indexer FTPS**
```
1. Aller à: Settings → Indexers
2. Cliquer: Add Indexer
3. Chercher: "FTPS Indexer" dans la liste
4. Vérifier: Formulaire de configuration s'affiche
5. Remplir les champs (voir config ci-dessous)
6. Cliquer: Test
7. Vérifier: ✅ Connection successful OU message d'erreur explicite
8. Sauvegarder si test OK
```

### **Test 2: Ajouter un Client FTPS**
```
1. Aller à: Settings → Download Clients  
2. Cliquer: Add
3. Chercher: "FTPS Client" dans la liste
4. Vérifier: Formulaire de configuration s'affiche
5. Remplir les champs (voir config ci-dessous)
6. Cliquer: Test
7. Vérifier: ✅ Connection successful OU message d'erreur explicite
8. Sauvegarder si test OK
```

---

## 📡 **3. TESTS API DIRECTS**

### **Vérifier Disponibilité des Schémas**
```bash
# Test Indexer Schema
curl -s "http://localhost:7878/api/v3/indexer/schema" \
  -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" \
  | grep -i "ftps indexer"

# Test Download Client Schema  
curl -s "http://localhost:7878/api/v3/downloadclient/schema" \
  -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" \
  | grep -i "ftps client"
```

### **Créer un Indexer FTPS via API**
```bash
curl -X POST "http://localhost:7878/api/v3/indexer" \
  -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" \
  -H "Content-Type: application/json" \
  -d '{
    "enableRss": true,
    "enableAutomaticSearch": true,
    "enableInteractiveSearch": true,
    "name": "Test FTPS Indexer",
    "implementation": "FtpsIndexer",
    "configContract": "FtpsIndexerSettings",
    "settings": {
      "host": "test.ftps.server.com",
      "port": 21,
      "username": "testuser",
      "password": "testpass",
      "movieDirectory": "movies",
      "securityMode": "explicit",
      "connectionMode": "passive"
    }
  }'
```

### **Créer un Client FTPS via API**
```bash
curl -X POST "http://localhost:7878/api/v3/downloadclient" \
  -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" \
  -H "Content-Type: application/json" \
  -d '{
    "enable": true,
    "name": "Test FTPS Client", 
    "implementation": "FtpsClient",
    "configContract": "FtpsSettings",
    "settings": {
      "host": "test.ftps.server.com",
      "port": 21,
      "username": "testuser", 
      "password": "testpass",
      "remotePath": "/downloads",
      "securityMode": "explicit",
      "connectionMode": "passive"
    }
  }'
```

### **Tester la Connexion via API**
```bash
# Test Indexer (remplacer {id} par l'ID retourné)
curl -X POST "http://localhost:7878/api/v3/indexer/test/{id}" \
  -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5"

# Test Download Client (remplacer {id} par l'ID retourné)  
curl -X POST "http://localhost:7878/api/v3/downloadclient/test/{id}" \
  -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5"
```

---

## 🖧 **4. TESTS AVEC SERVEUR FTPS RÉEL**

### **Configuration Test avec Serveur Existant**
```yaml
# Si vous avez un serveur FTPS privé
Host: votre-serveur.com
Port: 21 (ou 990 pour FTPS implicite)
Username: votre-username  
Password: votre-password
SSL/TLS Mode: Explicit (ou Implicit selon votre serveur)
Connection Mode: Passive
Movie Directory: movies (ou votre dossier de films)
```

### **Structure de Test Recommandée**
```
/movies/
├── Test.Movie.2023.1080p.BluRay.x264-TEST/
│   ├── Test.Movie.2023.1080p.BluRay.x264-TEST.mkv
│   └── Test.Movie.2023.1080p.BluRay.x264-TEST.nfo
├── Archive.Movie.2023.720p.WEB.x264-GROUP/
│   ├── Archive.Movie.2023.720p.WEB.x264-GROUP.rar
│   ├── Archive.Movie.2023.720p.WEB.x264-GROUP.r00
│   └── Archive.Movie.2023.720p.WEB.x264-GROUP.sfv
└── Simple.Movie.2023.1080p.WEB.x264-SIMPLE/
    └── Simple.Movie.2023.1080p.WEB.x264-SIMPLE.mp4
```

### **Tests à Effectuer**
```
1. Connexion SSL/TLS ✅
2. Authentification ✅  
3. Navigation dans le dossier movies ✅
4. Détection des sous-dossiers ✅
5. Sélection du meilleur fichier par dossier ✅
6. Parsing des noms de release ✅
7. Création des ReleaseInfo ✅
8. Téléchargement d'un fichier test ✅
```

---

## 💾 **5. SERVEUR FTPS LOCAL POUR TESTS**

### **Option A: Docker FTPS Server**
```bash
# Créer un serveur FTPS local pour tests
docker run -d \
  --name test-ftps \
  -p 21:21 \
  -p 30000-30009:30000-30009 \
  -e USERS="testuser|testpass|/home/testuser|1001" \
  -e ADDRESS=localhost \
  stilliard/pure-ftpd:hardened

# Créer structure de test
docker exec test-ftps mkdir -p /home/testuser/movies
docker exec test-ftps mkdir -p /home/testuser/movies/Test.Movie.2023.1080p.BluRay.x264-TEST

# Créer fichier de test
docker exec test-ftps sh -c 'echo "Test video file" > /home/testuser/movies/Test.Movie.2023.1080p.BluRay.x264-TEST/Test.Movie.2023.1080p.BluRay.x264-TEST.mkv'
```

### **Option B: vsftpd Local (Ubuntu)**
```bash
# Installer vsftpd
sudo apt update
sudo apt install vsftpd openssl

# Configuration SSL
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /etc/ssl/private/vsftpd.pem \
  -out /etc/ssl/private/vsftpd.pem

# Configuration vsftpd
sudo nano /etc/vsftpd.conf
# Ajouter:
# ssl_enable=YES
# force_local_data_ssl=YES
# force_local_logins_ssl=YES
# ssl_ciphers=HIGH
# rsa_cert_file=/etc/ssl/private/vsftpd.pem
# rsa_private_key_file=/etc/ssl/private/vsftpd.pem

# Créer utilisateur test
sudo useradd -m ftpstest
sudo passwd ftpstest  # Définir mot de passe

# Créer structure de test
sudo mkdir -p /home/ftpstest/movies/Test.Movie.2023.1080p.BluRay.x264-TEST
sudo chown -R ftpstest:ftpstest /home/ftpstest

# Démarrer service
sudo systemctl restart vsftpd
```

### **Configuration de Test Local**
```yaml
Host: localhost (ou 127.0.0.1)
Port: 21
Username: testuser (ou ftpstest)
Password: testpass (ou votre mot de passe)
SSL/TLS Mode: Explicit
Connection Mode: Passive
Movie Directory: movies
Base Path: /
Accept Invalid Certificates: true (pour certificats auto-signés)
```

---

## 🧪 **SCÉNARIOS DE TEST COMPLETS**

### **Test 1: Découverte de Films**
```
1. Configurer indexer FTPS avec serveur de test
2. Ajouter films dans /movies/ avec structure correcte
3. Aller à: System → Tasks → Refresh Monitored Downloads
4. Vérifier logs: System → Logs → chercher "FTPS"
5. Vérifier: Films détectés dans Activity → Recent
```

### **Test 2: Téléchargement Manuel** 
```
1. Configurer client FTPS avec serveur de test
2. Aller à: Movies → Add Movies
3. Ajouter un film en mode "Search for movie"
4. Aller à: Activity → Manual Search
5. Chercher releases FTPS disponibles
6. Cliquer: Download sur une release FTPS
7. Vérifier: Téléchargement dans Activity → Queue
```

### **Test 3: Archives RAR**
```
1. Créer dossier avec fichiers .rar/.r00/.r01
2. Vérifier: Indexer sélectionne le fichier .rar principal
3. Tester: Téléchargement du fichier RAR
4. Vérifier: Taille correcte après téléchargement
```

### **Test 4: Gestion d'Erreurs**
```
1. Tester avec mauvais mot de passe → Erreur auth
2. Tester avec mauvais host → Erreur connexion  
3. Tester avec certificat invalide → Erreur SSL
4. Vérifier: Messages d'erreur clairs dans logs
```

---

## 📊 **VALIDATION DES RÉSULTATS**

### **Logs à Vérifier**
```bash
# Localisation des logs
/home/ubuntu/.config/Radarr/logs/

# Logs importants
tail -f radarr.txt | grep -i ftps
tail -f radarr.debug.txt | grep -i ftps  
tail -f radarr.trace.txt | grep -i ftps
```

### **Messages de Succès Attendus**
```
[Info] FtpsIndexer: Connected to FTPS server successfully
[Debug] FtpsIndexer: Found X releases in directory /movies/
[Info] FtpsClient: Downloaded file successfully from ftps://...
[Debug] FtpsProxy: SSL connection established with server
```

### **Messages d'Erreur Courants**
```
[Error] FtpsProxy: Authentication failed for user 'username'
[Error] FtpsProxy: Could not connect to host 'hostname' on port 21
[Error] FtpsProxy: SSL certificate validation failed  
[Warn] FtpsIndexer: No video or archive files found in directory
```

---

## ✅ **CHECKLIST DE VALIDATION**

### **Tests de Base**
- [ ] Compilation réussie (0 erreurs)
- [ ] FTPS Indexer visible dans Add Indexer
- [ ] FTPS Client visible dans Add Download Client
- [ ] Connexion FTPS réussie (test connection)
- [ ] Navigation dans dossiers FTPS

### **Tests de Découverte**  
- [ ] Détection des dossiers de films
- [ ] Sélection du meilleur fichier (vidéo > archive)
- [ ] Parsing correct des noms de release
- [ ] Création des ReleaseInfo avec bon protocole

### **Tests de Téléchargement**
- [ ] Téléchargement fichier vidéo (.mkv, .mp4)
- [ ] Téléchargement archive (.rar, .zip)
- [ ] Vérification intégrité (taille fichier)
- [ ] Gestion erreurs réseau

### **Tests SSL/TLS**
- [ ] FTPS Explicit (AUTH TLS)
- [ ] FTPS Implicit (SSL dès connexion)  
- [ ] Certificats auto-signés acceptés
- [ ] Validation certificats (optionnelle)

---

## 🎯 **COMMANDES DE TEST RAPIDE**

### **Test Complet en Une Ligne**
```bash
# Vérifier que tout fonctionne
curl -s "http://localhost:7878/api/v3/indexer/schema" -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" | grep -q "FTPS Indexer" && curl -s "http://localhost:7878/api/v3/downloadclient/schema" -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" | grep -q "FTPS Client" && echo "✅ FTPS Integration OK" || echo "❌ FTPS Integration Failed"
```

### **Test Status Radarr**
```bash
# Vérifier que Radarr fonctionne  
curl -s "http://localhost:7878/api/v3/system/status" -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" | grep -o '"appName":"[^"]*"'
```

---

## 🏆 **RÉSULTATS ATTENDUS**

Si tous les tests passent, vous devriez avoir :

✅ **Interface** : FTPS Indexer & Client visibles dans UI  
✅ **API** : Schémas FTPS disponibles via endpoints  
✅ **Connexion** : Tests de connexion réussis  
✅ **Découverte** : Films détectés depuis serveurs FTPS  
✅ **Téléchargement** : Fichiers téléchargés correctement  
✅ **Archives** : RAR/ZIP gérés intelligemment  
✅ **SSL/TLS** : Connexions sécurisées fonctionnelles  
✅ **Logs** : Messages informatifs sans erreurs  

**🎯 Votre intégration FTPS est alors PRODUCTION-READY !**