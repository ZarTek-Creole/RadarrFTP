#!/bin/bash

# Script de démarrage pour Radarr avec support FTPS
echo "🚀 Démarrage de Radarr avec client FTPS..."

# Définir les variables d'environnement
export RADARR_HOST="0.0.0.0"
export RADARR_PORT="7878"
export RADARR_DATA_DIR="$HOME/.config/Radarr"

# Créer le répertoire de données s'il n'existe pas
mkdir -p "$RADARR_DATA_DIR"

echo "📁 Répertoire de données: $RADARR_DATA_DIR"
echo "🌐 Interface web: http://localhost:$RADARR_PORT"
echo ""
echo "✨ Client FTPS activé et prêt à l'emploi !"
echo ""

# Naviguer vers le répertoire de l'exécutable
cd _output/net6.0/

# Démarrer Radarr
echo "🎬 Démarrage de Radarr..."
./Radarr --host="$RADARR_HOST" --port="$RADARR_PORT" --data="$RADARR_DATA_DIR"