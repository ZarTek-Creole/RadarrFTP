#!/bin/bash

echo "⚡ TESTS RAPIDES FTPS - VÉRIFICATION DE BASE"
echo "============================================"
echo

# Couleurs
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Variables
WORKSPACE="/workspace"
SRC_DIR="$WORKSPACE/src"

# Fonction de test rapide
quick_test() {
    local test_name=$1
    local test_filter=$2
    
    echo -e "${BLUE}🔍 Test: $test_name${NC}"
    
    cd "$SRC_DIR"
    
    # Exécuter test avec timeout de 60 secondes
    local result=$(timeout 60 dotnet test --filter "$test_filter" --verbosity quiet --nologo 2>&1)
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        echo -e "${GREEN}✅ $test_name: OK${NC}"
        return 0
    else
        echo -e "${RED}❌ $test_name: ÉCHEC${NC}"
        echo "   $(echo "$result" | grep -E "(error|Error|failed)" | head -1)"
        return 1
    fi
}

echo -e "${YELLOW}📋 Tests de vérification de base...${NC}"
echo

# Tests essentiels
passed=0
total=0

# Test 1: Compilation des classes FTPS
echo -e "${BLUE}🔧 Vérification de la compilation...${NC}"
cd "$SRC_DIR"
if dotnet build Radarr.sln -c Release --no-restore -p:TreatWarningsAsErrors=false --verbosity quiet > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Compilation: OK${NC}"
    passed=$((passed + 1))
else
    echo -e "${RED}❌ Compilation: ÉCHEC${NC}"
fi
total=$((total + 1))
echo

# Test 2: Validation des settings
if quick_test "FtpsSettings" "FtpsSettingsFixture"; then
    passed=$((passed + 1))
fi
total=$((total + 1))

# Test 3: Validation du client FTPS
if quick_test "FtpsClient" "FtpsClientFixture.should_return_correct_protocol"; then
    passed=$((passed + 1))
fi
total=$((total + 1))

# Test 4: Validation de l'indexer FTPS
if quick_test "FtpsIndexer" "FtpsIndexerFixture.should_return_correct_protocol"; then
    passed=$((passed + 1))
fi
total=$((total + 1))

# Test 5: Vérification du modèle de données
if quick_test "FtpsDirectoryItem" "FtpsDirectoryItemFixture.should_initialize_with_default_values"; then
    passed=$((passed + 1))
fi
total=$((total + 1))

echo
echo -e "${BLUE}📊 RÉSULTATS RAPIDES${NC}"
echo "==================="
echo "   ✅ Tests réussis: $passed/$total"

if [ $passed -eq $total ]; then
    echo -e "${GREEN}🎉 TOUS LES TESTS DE BASE RÉUSSIS !${NC}"
    echo
    echo "✅ Votre implémentation FTPS fonctionne"
    echo "✅ Prêt pour les tests complets avec: ./run_all_ftps_tests.sh"
    echo
    echo "🚀 ÉTAPES SUIVANTES:"
    echo "   1. Démarrer Radarr: ./start_radarr.sh"
    echo "   2. Ouvrir l'interface: http://localhost:7878"
    echo "   3. Ajouter un indexer FTPS dans Settings > Indexers"
    echo "   4. Ajouter un client FTPS dans Settings > Download Clients"
    exit 0
else
    echo -e "${YELLOW}⚠️ Certains tests de base ont échoué${NC}"
    echo
    echo "🔧 ACTIONS RECOMMANDÉES:"
    echo "   1. Vérifier la compilation: dotnet build"
    echo "   2. Exécuter les tests complets: ./run_all_ftps_tests.sh"
    echo "   3. Corriger les erreurs identifiées"
    exit 1
fi