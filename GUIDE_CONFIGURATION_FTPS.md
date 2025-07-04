# 🎯 Guide de Configuration FTPS pour Radarr

## 📋 Table des Matières

1. [Vue d'ensemble](#vue-densemble)
2. [Installation](#installation)
3. [Configuration du Client FTPS](#configuration-du-client-ftps)
4. [Configuration Avancée](#configuration-avancée)
5. [Dépannage](#dépannage)
6. [Sécurité](#sécurité)
7. [FAQ](#faq)

---

## 🎯 Vue d'ensemble

Le client FTPS pour Radarr permet de télécharger des films directement depuis des serveurs FTPS privés de la scène warez. Cette fonctionnalité offre une alternative moderne aux méthodes traditionnelles de téléchargement (Usenet/BitTorrent).

### ✨ Fonctionnalités

- **🔐 Sécurité SSL/TLS** : Chiffrement complet des connexions
- **🎬 Détection intelligente** : Reconnaissance automatique des releases scene
- **📊 Scoring avancé** : Priorisation basée sur la qualité et la source
- **🔄 Reprise de téléchargement** : Gestion des interruptions
- **📈 Suivi en temps réel** : Progression et statut des téléchargements
- **🔍 Multi-serveurs** : Support de plusieurs serveurs FTPS
- **⚡ Performance optimisée** : Transferts par chunks avec retry automatique

---

## 🚀 Installation

### Prérequis

- **Système d'exploitation** : Linux (Ubuntu 20.04+, Debian 11+)
- **Runtime** : .NET 6.0
- **Mémoire** : Minimum 512MB RAM
- **Stockage** : Espace libre pour les téléchargements

### Installation Automatique

```bash
# Télécharger le script d'installation
wget https://raw.githubusercontent.com/your-repo/radarr-ftps/main/deploy-ftps-radarr.sh

# Rendre le script exécutable
chmod +x deploy-ftps-radarr.sh

# Exécuter l'installation (en tant que root)
sudo ./deploy-ftps-radarr.sh
```

### Installation Manuelle

Si vous préférez installer manuellement, suivez les étapes du script ou consultez la documentation détaillée.

---

## ⚙️ Configuration du Client FTPS

### 1. Accéder à l'Interface Web

Après l'installation, accédez à votre instance Radarr :

```
http://votre-serveur:7878
```

### 2. Ajouter un Client FTPS

1. **Naviguer vers les paramètres**
   - Cliquez sur `Settings` (⚙️)
   - Sélectionnez `Download Clients`

2. **Ajouter un nouveau client**
   - Cliquez sur le bouton `+` (Plus)
   - Sélectionnez `FTPS` dans la liste des clients disponibles

3. **Configuration de base**

   ```yaml
   Nom: Mon Serveur FTPS
   Activer: ✓
   Serveur: ftps.monserveur.com
   Port: 21
   Nom d'utilisateur: monusername
   Mot de passe: monmotdepasse
   ```

### 3. Paramètres SSL/TLS

```yaml
Utiliser SSL: ✓
Mode de chiffrement: Explicit FTPS
Valider le certificat: ✓ (Recommandé)
Protocoles SSL: TLS 1.2, TLS 1.3
```

### 4. Configuration des Chemins

```yaml
Chemin de base distant: /movies
Répertoire de téléchargement: /downloads/movies
Catégorie: movies
```

### 5. Paramètres de Performance

```yaml
Taille des chunks: 1048576 (1MB)
Tentatives de reconnexion: 3
Timeout de connexion: 30 secondes
Transferts simultanés: 2
```

### 6. Test de Connexion

- Cliquez sur `Test` pour vérifier la connexion
- ✅ **Succès** : Configuration correcte
- ❌ **Échec** : Vérifiez les paramètres

---

## 🔧 Configuration Avancée

### Serveurs Multiples

Vous pouvez configurer plusieurs serveurs FTPS avec des priorités différentes :

```yaml
Serveur 1:
  Nom: Serveur Principal
  Priorité: 1
  Serveur: primary.ftps.com

Serveur 2:
  Nom: Serveur Secondaire  
  Priorité: 2
  Serveur: backup.ftps.com
```

### Filtrage par Qualité

Configurez des règles pour filtrer automatiquement les releases :

```yaml
Qualités acceptées:
  - 1080p BluRay
  - 720p WEB-DL
  - 2160p UHD

Sources préférées:
  - BluRay (Score: 100)
  - WEB-DL (Score: 80)
  - WEBRip (Score: 60)
```

### Patterns de Nommage Scene

Le client reconnaît automatiquement ces formats :

```
Movie.Title.2023.1080p.BluRay.x264-GROUP
Movie.Title.2023.720p.WEB-DL.x264-GROUP
Movie.Title.2023.2160p.UHD.BluRay.x265-GROUP
Movie.Title.2023.480p.DVDRip.XviD-GROUP
```

### Configuration des Retry

```yaml
Tentatives maximum: 5
Délai entre tentatives: 30 secondes
Backoff exponentiel: ✓
Timeout par tentative: 300 secondes
```

---

## 🔍 Dépannage

### Problèmes de Connexion

**❌ Erreur SSL/TLS**
```
Solution: Vérifiez le mode de chiffrement (Explicit vs Implicit)
Testez avec "Valider le certificat" désactivé temporairement
```

**❌ Timeout de connexion**
```
Solution: Augmentez le timeout de connexion
Vérifiez la connectivité réseau
Testez avec un client FTP externe
```

**❌ Authentification échouée**
```
Solution: Vérifiez les identifiants
Confirmez que le compte FTPS est actif
Testez avec un autre client FTPS
```

### Problèmes de Téléchargement

**❌ Releases non détectées**
```
Solution: Vérifiez le chemin de base distant
Confirmez la structure des répertoires
Activez les logs détaillés
```

**❌ Téléchargements interrompus**
```
Solution: Réduisez la taille des chunks
Augmentez le nombre de tentatives
Vérifiez la stabilité de la connexion
```

### Logs et Diagnostics

```bash
# Consulter les logs Radarr
sudo journalctl -u radarr -f

# Logs spécifiques FTPS
tail -f /var/lib/radarr/logs/radarr.txt | grep FTPS

# Test de connectivité
telnet serveur-ftps.com 21
```

---

## 🔐 Sécurité

### Bonnes Pratiques

1. **Chiffrement obligatoire**
   - Toujours utiliser SSL/TLS
   - Préférer TLS 1.2+ uniquement
   - Valider les certificats en production

2. **Authentification sécurisée**
   - Utiliser des mots de passe forts
   - Changer régulièrement les credentials
   - Considérer l'authentification par clé

3. **Réseau sécurisé**
   - Utiliser un VPN si nécessaire
   - Configurer un pare-feu approprié
   - Limiter l'accès aux IPs autorisées

### Configuration Pare-feu

```bash
# UFW (Ubuntu/Debian)
sudo ufw allow 7878/tcp comment "Radarr Web Interface"
sudo ufw allow out 21/tcp comment "FTPS Control"
sudo ufw allow out 20/tcp comment "FTPS Data Active"
sudo ufw allow out 1024:65535/tcp comment "FTPS Data Passive"

# iptables
iptables -A OUTPUT -p tcp --dport 21 -j ACCEPT
iptables -A OUTPUT -p tcp --dport 20 -j ACCEPT
iptables -A OUTPUT -p tcp --dport 1024:65535 -j ACCEPT
```

### Chiffrement des Credentials

Les mots de passe sont automatiquement chiffrés dans la base de données Radarr avec AES-256.

---

## ❓ FAQ

### **Q: Quelle est la différence entre Explicit et Implicit FTPS ?**

**R:** 
- **Explicit FTPS** : Démarre en FTP normal puis passe en SSL (port 21)
- **Implicit FTPS** : Chiffré dès la connexion (port 990)
- La plupart des serveurs utilisent Explicit FTPS

### **Q: Puis-je utiliser FTPS avec un serveur FTP normal ?**

**R:** Non, ce client nécessite SSL/TLS. Pour du FTP normal, utilisez le client FTP standard de Radarr.

### **Q: Comment optimiser les performances de téléchargement ?**

**R:** 
- Augmentez la taille des chunks (1-4MB)
- Utilisez le mode passif
- Configurez plusieurs connexions simultanées
- Choisissez un serveur géographiquement proche

### **Q: Le client supporte-t-il IPv6 ?**

**R:** Oui, FluentFTP supporte IPv6. Utilisez une adresse IPv6 dans le champ serveur.

### **Q: Comment migrer depuis un autre client de téléchargement ?**

**R:** 
1. Configurez le client FTPS en parallèle
2. Testez avec quelques téléchargements
3. Désactivez progressivement l'ancien client
4. Transférez les téléchargements en cours si nécessaire

### **Q: Que faire si mon serveur FTPS utilise un port non-standard ?**

**R:** Changez simplement le port dans la configuration. Ports courants :
- Port 21 : FTP/FTPS Explicit standard
- Port 990 : FTPS Implicit
- Ports personnalisés : Selon votre serveur

---

## 📞 Support

### Ressources d'Aide

- **📖 Documentation** : [Wiki Radarr](https://wiki.servarr.com/radarr)
- **💬 Discord** : [Serveur Radarr](https://discord.gg/radarr)
- **🐛 Issues** : [GitHub Issues](https://github.com/Radarr/Radarr/issues)
- **📧 Support** : [Forums Radarr](https://forums.radarr.video)

### Rapporter un Bug

Lorsque vous rapportez un problème, incluez :

1. Version de Radarr
2. Système d'exploitation
3. Configuration FTPS (sans les credentials)
4. Logs détaillés
5. Étapes de reproduction

---

## 🎉 Conclusion

Le client FTPS pour Radarr offre une solution moderne et sécurisée pour automatiser le téléchargement de films depuis des serveurs FTPS privés. Avec une configuration appropriée, vous bénéficierez d'une expérience fluide et sécurisée.

**🚀 Profitez de votre nouveau client FTPS !**

---

*Guide mis à jour le $(date '+%Y-%m-%d') - Version 1.0*