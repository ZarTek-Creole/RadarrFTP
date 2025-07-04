#!/bin/bash

# 🎯 Script de Lancement Radarr avec Support FTPS
# ===============================================

set -e

echo ""
echo "🚀 Lancement Radarr avec Support FTPS"
echo "====================================="
echo ""

# Configuration
RADARR_URL="http://localhost:7878"
BUILD_CONFIG="Release"

# Fonctions utilitaires
print_status() {
    echo -e "\033[34m[INFO]\033[0m $1"
}

print_success() {
    echo -e "\033[32m[SUCCESS]\033[0m $1"
}

print_error() {
    echo -e "\033[31m[ERROR]\033[0m $1"
}

print_warning() {
    echo -e "\033[33m[WARNING]\033[0m $1"
}

# Vérifier les prérequis
print_status "Vérification des prérequis..."

# Vérifier .NET
if ! command -v dotnet &> /dev/null; then
    print_error ".NET 6.0 n'est pas installé"
    echo "Installez .NET 6.0 depuis: https://dotnet.microsoft.com/download/dotnet/6.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
print_success ".NET détecté: $DOTNET_VERSION"

# Vérifier Node.js (optionnel pour le frontend)
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    print_success "Node.js détecté: $NODE_VERSION"
else
    print_warning "Node.js non détecté (requis uniquement pour recompiler le frontend)"
fi

# Vérifier le répertoire source
if [ ! -d "src" ]; then
    print_error "Répertoire 'src' non trouvé"
    print_error "Assurez-vous d'être dans le répertoire racine de Radarr"
    exit 1
fi

print_success "Répertoire source trouvé"

# Vérifier l'intégration FTPS
print_status "Vérification de l'intégration FTPS..."

if [ ! -f "src/NzbDrone.Core/Download/Clients/Ftps/Ftps.cs" ]; then
    print_error "Fichier Ftps.cs non trouvé"
    print_error "L'intégration FTPS n'est pas présente"
    exit 1
fi

if [ ! -f "src/NzbDrone.Core/Download/Clients/Ftps/FtpsSettings.cs" ]; then
    print_error "Fichier FtpsSettings.cs non trouvé"
    exit 1
fi

if [ ! -f "src/NzbDrone.Core/Download/Clients/Ftps/FtpsProxy.cs" ]; then
    print_error "Fichier FtpsProxy.cs non trouvé"
    exit 1
fi

print_success "Intégration FTPS validée"

# Vérifier FluentFTP dans le projet
if ! grep -q "FluentFTP" src/NzbDrone.Core/Radarr.Core.csproj; then
    print_error "Dépendance FluentFTP non trouvée dans le projet"
    exit 1
fi

print_success "Dépendance FluentFTP validée"

# Restaurer les dépendances
print_status "Restauration des dépendances NuGet..."
cd src
dotnet restore
if [ $? -eq 0 ]; then
    print_success "Dépendances restaurées"
else
    print_error "Erreur lors de la restauration des dépendances"
    exit 1
fi

# Compiler le projet Core
print_status "Compilation du Core Radarr..."
dotnet build NzbDrone.Core/Radarr.Core.csproj --configuration $BUILD_CONFIG --no-restore
if [ $? -eq 0 ]; then
    print_success "Core compilé avec succès"
else
    print_error "Erreur lors de la compilation du Core"
    exit 1
fi

# Compiler l'Host
print_status "Compilation de l'Host Radarr..."
dotnet build Radarr.Host/Radarr.Host.csproj --configuration $BUILD_CONFIG --no-restore
if [ $? -eq 0 ]; then
    print_success "Host compilé avec succès"
else
    print_error "Erreur lors de la compilation de l'Host"
    exit 1
fi

# Vérifier si le frontend existe
if [ -d "../frontend" ] && command -v npm &> /dev/null; then
    print_status "Frontend détecté, vérification..."
    if [ ! -d "../frontend/build" ]; then
        print_warning "Frontend non compilé, compilation en cours..."
        cd ../frontend
        npm install --silent
        npm run build --silent
        cd ../src
        print_success "Frontend compilé"
    else
        print_success "Frontend déjà compilé"
    fi
fi

# Démarrer Radarr
echo ""
print_status "Démarrage de Radarr avec support FTPS..."
print_status "URL d'accès: $RADARR_URL"
echo ""

# Aller dans le répertoire Host
cd Radarr.Host

# Démarrer avec gestion des signaux
cleanup() {
    print_status "Arrêt de Radarr..."
    exit 0
}

trap cleanup SIGINT SIGTERM

print_success "Radarr démarré avec succès !"
echo ""
echo "📋 Informations importantes :"
echo "   • URL d'accès: $RADARR_URL"
echo "   • Support FTPS: ✅ Activé"
echo "   • Configuration: Settings → Download Clients → Add → FTPS"
echo "   • Test rapide: Serveur Rebex (test.rebex.net)"
echo ""
echo "🔧 Actions disponibles :"
echo "   • Ctrl+C pour arrêter"
echo "   • Ouvrir $RADARR_URL dans votre navigateur"
echo ""
echo "🎯 Prochaines étapes :"
echo "   1. Configurer un client FTPS dans Settings → Download Clients"
echo "   2. Ajouter un film pour tester le téléchargement"
echo "   3. Surveiller dans Activity → Queue"
echo ""

# Démarrer Radarr
dotnet run --configuration $BUILD_CONFIG --urls=$RADARR_URL