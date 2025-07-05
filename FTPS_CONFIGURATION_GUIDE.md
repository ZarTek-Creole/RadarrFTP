# 🎯 Guide Complet : Radarr avec FTPS Indexer & Client

## 🎉 **PROBLÈME RÉSOLU : Workflow FTPS Complet Disponible**

Vous aviez **absolument raison** - un client FTPS sans indexer FTPS était inutile ! Maintenant vous avez les deux :

### ✅ **COMPOSANTS FONCTIONNELS**
1. **FTPS Indexer** - Scanne et découvre les films sur vos serveurs FTPS
2. **FTPS Client** - Télécharge les films depuis vos serveurs FTPS

---

## 📋 **CONFIGURATION ÉTAPE PAR ÉTAPE**

### **1. Configuration de l'Indexer FTPS**

**Accès :** http://localhost:7878 → Settings → Indexers → Add Indexer → **FTPS Indexer**

**Paramètres requis :**
```yaml
Host: votre-serveur-ftps.com
Port: 21 (ou 990 pour FTPS implicite)
Username: votre-nom-utilisateur
Password: votre-mot-de-passe
SSL/TLS Mode: Explicit (ou Implicit selon votre serveur)
Connection Mode: Passive (recommandé)
Movie Directory: movies (ou le dossier où sont vos films)
Base Path: / (ou le chemin de base sur votre serveur)
```

**Paramètres avancés :**
```yaml
Accept Invalid Certificates: true (si certificats auto-signés)
Connection Timeout: 30 secondes
Data Connection Timeout: 30 secondes
```

### **2. Configuration du Client FTPS**

**Accès :** http://localhost:7878 → Settings → Download Clients → Add → **FTPS Client**

**Paramètres requis :**
```yaml
Host: votre-serveur-ftps.com
Port: 21 (ou 990 pour FTPS implicite)
Username: votre-nom-utilisateur
Password: votre-mot-de-passe
SSL/TLS Mode: Explicit (ou Implicit selon votre serveur)
Connection Mode: Passive (recommandé)
Remote Path: /downloads (dossier de téléchargement)
Category: movies (optionnel)
```

---

## 🔄 **WORKFLOW AUTOMATIQUE**

### **Comment ça fonctionne maintenant :**

1. **Découverte automatique**
   - L'indexer FTPS scanne votre serveur toutes les heures
   - Il trouve tous les films disponibles dans `Movie Directory`
   - Parse les noms pour identifier : titre, année, qualité

2. **Sélection intelligente**
   - Radarr voit tous les films disponibles
   - Compare avec votre liste de films recherchés
   - Sélectionne automatiquement les meilleures releases

3. **Téléchargement automatique**
   - Le client FTPS reçoit les ordres de téléchargement
   - Télécharge les films vers votre dossier local
   - Organise automatiquement les fichiers

---

## 📁 **STRUCTURE DE SERVEUR FTPS RECOMMANDÉE**

```
/movies/
├── Avatar.2009.1080p.BluRay.x264-GROUP/
│   ├── Avatar.2009.1080p.BluRay.x264-GROUP.mkv
│   ├── Avatar.2009.1080p.BluRay.x264-GROUP.nfo
│   └── Subs/
├── Inception.2010.4K.UHD.BluRay.x265-TEAM/
│   ├── Inception.2010.4K.UHD.BluRay.x265-TEAM.mkv
│   └── Inception.2010.4K.UHD.BluRay.x265-TEAM.nfo
└── The.Matrix.1999.1080p.BluRay.x264-CLASSIC/
    ├── The.Matrix.1999.1080p.BluRay.x264-CLASSIC.mkv
    └── The.Matrix.1999.1080p.BluRay.x264-CLASSIC.nfo
```

---

## 🎯 **AVANTAGES POUR VOS SERVEURS PRIVÉS**

### **Pour la Scène Privée :**
- **Découverte automatique** de nouveaux uploads
- **Parsing intelligent** des noms de release
- **Sélection qualité** automatique (4K > 1080p > 720p)
- **Téléchargement sécurisé** avec SSL/TLS
- **Gestion des erreurs** et retry automatique

### **Versus Solutions Traditionnelles :**
```
❌ Jackett/Prowlarr: Limité aux sites publics
❌ Usenet: Payant et incomplet
❌ BitTorrent: Problèmes légaux potentiels
✅ FTPS Privé: Contrôle total, sécurisé, fiable
```

---

## 🛠️ **TEST & VALIDATION**

### **Tester l'Indexer FTPS :**
```bash
# Via API Radarr
curl -X GET "http://localhost:7878/api/v3/indexer/test" \
  -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" \
  -H "Content-Type: application/json" \
  -d '{"id": YOUR_INDEXER_ID}'
```

### **Tester le Client FTPS :**
```bash
# Via API Radarr
curl -X GET "http://localhost:7878/api/v3/downloadclient/test" \
  -H "X-Api-Key: 36a5d1e3a99a46358954df8874aa05e5" \
  -H "Content-Type: application/json" \
  -d '{"id": YOUR_CLIENT_ID}'
```

---

## 🔍 **MONITORING & LOGS**

### **Vérifier les Découvertes :**
- **Activity** → Recent → Indexer Searches
- **System** → Logs → Filtrer par "FTPS"

### **Vérifier les Téléchargements :**
- **Activity** → Queue → Downloads in Progress
- **System** → Status → Download Clients

---

## 🚀 **RÉSULTAT FINAL**

### **Vous avez maintenant :**
```
✅ Indexer FTPS → Découvre automatiquement les films
✅ Client FTPS → Télécharge automatiquement les films
✅ Workflow complet → Serveurs FTPS privés → Radarr
✅ Parsing intelligent → Noms de release → Métadonnées
✅ Sélection qualité → Préférences automatiques
✅ Téléchargement sécurisé → SSL/TLS chiffré
```

### **Interface Utilisateur :**
- **Add Indexer** → FTPS Indexer ✅ **DISPONIBLE**
- **Add Download Client** → FTPS Client ✅ **DISPONIBLE**
- **Recent Activity** → Affiche les découvertes et téléchargements
- **Movies** → Affiche les films disponibles sur vos serveurs

---

## 🔑 **INFORMATIONS TECHNIQUES**

- **API Radarr** : http://localhost:7878/api/v3
- **Clé API** : 36a5d1e3a99a46358954df8874aa05e5
- **Protocole** : `ftps` (reconnu par Radarr)
- **Bibliothèque** : FluentFTP v48.0.2
- **Support SSL/TLS** : Explicit, Implicit, None

**Le système est maintenant COMPLET et FONCTIONNEL pour vos serveurs FTPS privés !** 🎉