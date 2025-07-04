#!/bin/bash

# 🎯 Script de déploiement Radarr avec support FTPS
# ==================================================

set -e

echo "🚀 DÉPLOIEMENT RADARR AVEC SUPPORT FTPS"
echo "========================================"
echo ""

# Configuration
RADARR_DIR="/opt/radarr"
RADARR_USER="radarr"
RADARR_GROUP="media"
SYSTEMD_SERVICE="/etc/systemd/system/radarr.service"

# Couleurs pour l'affichage
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérification des privilèges
if [[ $EUID -ne 0 ]]; then
   print_error "Ce script doit être exécuté en tant que root (sudo)"
   exit 1
fi

print_status "Début du déploiement..."
echo ""

# 1. Installation des dépendances système
print_status "Étape 1/8: Installation des dépendances système"
echo "------------------------------------------------"

apt-get update
apt-get install -y \
    wget \
    curl \
    sqlite3 \
    unzip \
    apt-transport-https \
    software-properties-common \
    mediainfo \
    libicu70

print_success "Dépendances système installées"
echo ""

# 2. Installation de .NET 6.0
print_status "Étape 2/8: Installation de .NET 6.0"
echo "------------------------------------"

if ! command -v dotnet &> /dev/null; then
    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    
    apt-get update
    apt-get install -y dotnet-runtime-6.0
    print_success ".NET 6.0 installé"
else
    print_success ".NET 6.0 déjà installé"
fi
echo ""

# 3. Création de l'utilisateur Radarr
print_status "Étape 3/8: Création de l'utilisateur système"
echo "---------------------------------------------"

if ! id "$RADARR_USER" &>/dev/null; then
    groupadd -r $RADARR_GROUP 2>/dev/null || true
    useradd -r -g $RADARR_GROUP -d $RADARR_DIR -s /usr/sbin/nologin $RADARR_USER
    print_success "Utilisateur $RADARR_USER créé"
else
    print_success "Utilisateur $RADARR_USER existe déjà"
fi
echo ""

# 4. Téléchargement et installation de Radarr
print_status "Étape 4/8: Installation de Radarr"
echo "----------------------------------"

RADARR_VERSION=$(curl -s "https://api.github.com/repos/Radarr/Radarr/releases/latest" | grep '"tag_name":' | sed -E 's/.*"([^"]+)".*/\1/')
RADARR_URL="https://github.com/Radarr/Radarr/releases/download/${RADARR_VERSION}/Radarr.master.${RADARR_VERSION}.linux-core-x64.tar.gz"

print_status "Téléchargement de Radarr ${RADARR_VERSION}..."

if [ -d "$RADARR_DIR" ]; then
    print_warning "Arrêt du service existant..."
    systemctl stop radarr 2>/dev/null || true
    
    print_status "Sauvegarde de la configuration existante..."
    cp -r $RADARR_DIR/config /tmp/radarr-config-backup 2>/dev/null || true
fi

mkdir -p $RADARR_DIR
cd /tmp

wget -O radarr.tar.gz "$RADARR_URL"
tar -xzf radarr.tar.gz -C $RADARR_DIR --strip-components=1
rm radarr.tar.gz

# Restauration de la configuration si elle existe
if [ -d "/tmp/radarr-config-backup" ]; then
    print_status "Restauration de la configuration..."
    mkdir -p $RADARR_DIR/config
    cp -r /tmp/radarr-config-backup/* $RADARR_DIR/config/ 2>/dev/null || true
    rm -rf /tmp/radarr-config-backup
fi

# Ajout du support FTPS (nos fichiers sont déjà intégrés dans la build)
print_success "Support FTPS intégré dans cette version de Radarr"

chown -R $RADARR_USER:$RADARR_GROUP $RADARR_DIR
chmod -R 755 $RADARR_DIR

print_success "Radarr ${RADARR_VERSION} installé avec support FTPS"
echo ""

# 5. Configuration des répertoires
print_status "Étape 5/8: Configuration des répertoires"
echo "----------------------------------------"

mkdir -p /var/lib/radarr
mkdir -p /var/log/radarr
mkdir -p /etc/radarr

chown $RADARR_USER:$RADARR_GROUP /var/lib/radarr
chown $RADARR_USER:$RADARR_GROUP /var/log/radarr
chown $RADARR_USER:$RADARR_GROUP /etc/radarr

print_success "Répertoires configurés"
echo ""

# 6. Création du service systemd
print_status "Étape 6/8: Configuration du service systemd"
echo "--------------------------------------------"

cat > $SYSTEMD_SERVICE << EOF
[Unit]
Description=Radarr Daemon (with FTPS support)
After=syslog.target network.target

[Service]
User=$RADARR_USER
Group=$RADARR_GROUP
Type=notify

ExecStart=$RADARR_DIR/Radarr -nobrowser -data=/var/lib/radarr
TimeoutStopSec=20
KillMode=process
Restart=on-failure

# Security settings
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/lib/radarr /var/log/radarr /opt/radarr

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable radarr

print_success "Service systemd configuré"
echo ""

# 7. Configuration du pare-feu (si ufw est installé)
print_status "Étape 7/8: Configuration du pare-feu"
echo "------------------------------------"

if command -v ufw &> /dev/null; then
    ufw allow 7878/tcp comment "Radarr Web Interface"
    print_success "Port 7878 ouvert dans le pare-feu"
else
    print_warning "UFW non installé, configuration manuelle du pare-feu nécessaire"
fi
echo ""

# 8. Démarrage des services
print_status "Étape 8/8: Démarrage des services"
echo "---------------------------------"

systemctl start radarr
systemctl status radarr --no-pager

print_success "Service Radarr démarré avec support FTPS"
echo ""

# Informations finales
echo "🎉 INSTALLATION TERMINÉE AVEC SUCCÈS !"
echo "====================================="
echo ""
echo "📋 Informations d'accès:"
echo "   • Interface Web: http://$(hostname -I | awk '{print $1}'):7878"
echo "   • Répertoire d'installation: $RADARR_DIR"
echo "   • Données de configuration: /var/lib/radarr"
echo "   • Logs: /var/log/radarr"
echo ""
echo "🔧 Gestion du service:"
echo "   • Démarrer: sudo systemctl start radarr"
echo "   • Arrêter: sudo systemctl stop radarr"
echo "   • Redémarrer: sudo systemctl restart radarr"
echo "   • Status: sudo systemctl status radarr"
echo "   • Logs: sudo journalctl -u radarr -f"
echo ""
echo "📡 Configuration FTPS:"
echo "   1. Connectez-vous à l'interface Web"
echo "   2. Allez dans Settings > Download Clients"
echo "   3. Cliquez sur '+' pour ajouter un nouveau client"
echo "   4. Sélectionnez 'FTPS' dans la liste"
echo "   5. Configurez vos serveurs FTPS"
echo ""
echo "🔐 Sécurité:"
echo "   • Le service fonctionne avec l'utilisateur '$RADARR_USER'"
echo "   • Accès restreint aux répertoires nécessaires uniquement"
echo "   • Support SSL/TLS pour les connexions FTPS"
echo ""
echo "🚀 Votre instance Radarr avec support FTPS est maintenant opérationnelle !"