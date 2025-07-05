#!/bin/bash

echo "🧪 SUITE COMPLÈTE DE TESTS FTPS POUR RADARR"
echo "==========================================="
echo

# Couleurs pour la sortie
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
WORKSPACE="/workspace"
SRC_DIR="$WORKSPACE/src"
TEST_RESULTS_DIR="$WORKSPACE/test-results"
COVERAGE_DIR="$WORKSPACE/coverage"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Créer les répertoires de résultats
mkdir -p "$TEST_RESULTS_DIR"
mkdir -p "$COVERAGE_DIR"

echo -e "${BLUE}📁 Répertoires de test configurés${NC}"
echo "   - Résultats: $TEST_RESULTS_DIR"
echo "   - Couverture: $COVERAGE_DIR"
echo

# Fonction pour exécuter un test spécifique
run_test_category() {
    local test_name=$1
    local test_filter=$2
    local description=$3
    
    echo -e "${YELLOW}🔍 Testing: $description${NC}"
    echo "   Filter: $test_filter"
    
    cd "$SRC_DIR"
    
    # Exécuter les tests avec sortie détaillée
    local test_output=$(dotnet test --filter "$test_filter" \
        --logger "trx;LogFileName=${test_name}_${TIMESTAMP}.trx" \
        --logger "console;verbosity=detailed" \
        --results-directory "$TEST_RESULTS_DIR" \
        --collect:"XPlat Code Coverage" \
        --settings "$WORKSPACE/coverlet.runsettings" \
        --verbosity normal 2>&1)
    
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        echo -e "${GREEN}✅ $description: PASSED${NC}"
        echo "   $(echo "$test_output" | grep -E "(Passed|Failed|Skipped)" | tail -1)"
    else
        echo -e "${RED}❌ $description: FAILED${NC}"
        echo "   $(echo "$test_output" | grep -E "(error|Error|ERROR)" | head -3)"
    fi
    
    echo
    return $exit_code
}

# Fonction pour analyser la couverture
analyze_coverage() {
    echo -e "${BLUE}📊 ANALYSE DE LA COUVERTURE${NC}"
    echo "================================="
    
    # Compter les fichiers de couverture
    local coverage_files=$(find "$TEST_RESULTS_DIR" -name "*.xml" -path "*/coverage*" | wc -l)
    echo "   Fichiers de couverture trouvés: $coverage_files"
    
    if [ $coverage_files -gt 0 ]; then
        echo -e "${GREEN}✅ Données de couverture disponibles${NC}"
        
        # Copier les fichiers de couverture
        find "$TEST_RESULTS_DIR" -name "*.xml" -path "*/coverage*" -exec cp {} "$COVERAGE_DIR/" \;
        
        echo "   Fichiers copiés vers: $COVERAGE_DIR"
    else
        echo -e "${YELLOW}⚠️ Aucune donnée de couverture générée${NC}"
    fi
    
    echo
}

# Fonction pour générer un rapport de résumé
generate_summary() {
    echo -e "${BLUE}📋 RÉSUMÉ DES TESTS FTPS${NC}"
    echo "========================="
    
    # Analyser les fichiers de résultats
    local trx_files=$(find "$TEST_RESULTS_DIR" -name "*.trx" | wc -l)
    echo "   Fichiers de résultats générés: $trx_files"
    
    if [ $trx_files -gt 0 ]; then
        echo -e "${GREEN}✅ Résultats détaillés disponibles${NC}"
        
        # Extraire les statistiques (approximatives)
        local total_passed=0
        local total_failed=0
        local total_skipped=0
        
        for trx_file in $(find "$TEST_RESULTS_DIR" -name "*.trx"); do
            if [ -f "$trx_file" ]; then
                local passed=$(grep -c "outcome=\"Passed\"" "$trx_file" 2>/dev/null || echo 0)
                local failed=$(grep -c "outcome=\"Failed\"" "$trx_file" 2>/dev/null || echo 0)
                local skipped=$(grep -c "outcome=\"NotExecuted\"" "$trx_file" 2>/dev/null || echo 0)
                
                total_passed=$((total_passed + passed))
                total_failed=$((total_failed + failed))
                total_skipped=$((total_skipped + skipped))
            fi
        done
        
        echo "   📊 Statistiques globales:"
        echo "      ✅ Passed: $total_passed"
        echo "      ❌ Failed: $total_failed"
        echo "      ⏭️ Skipped: $total_skipped"
        echo "      📈 Total: $((total_passed + total_failed + total_skipped))"
        
        # Calculer le pourcentage de réussite
        local total_executed=$((total_passed + total_failed))
        if [ $total_executed -gt 0 ]; then
            local success_rate=$((total_passed * 100 / total_executed))
            echo "      🎯 Taux de réussite: ${success_rate}%"
        fi
    fi
    
    echo
}

# Fonction pour lister les tests disponibles
list_available_tests() {
    echo -e "${BLUE}🔍 TESTS FTPS DISPONIBLES${NC}"
    echo "========================="
    
    cd "$SRC_DIR"
    
    # Rechercher tous les fichiers de test FTPS
    local ftps_test_files=$(find . -name "*Ftps*Fixture.cs" -o -name "*Ftps*Test.cs" | sort)
    
    if [ ! -z "$ftps_test_files" ]; then
        echo "   Fichiers de test trouvés:"
        echo "$ftps_test_files" | while read -r file; do
            echo "      📄 $file"
        done
    else
        echo -e "${YELLOW}⚠️ Aucun fichier de test FTPS trouvé${NC}"
    fi
    
    echo
}

# Fonction principale
main() {
    echo -e "${BLUE}🚀 DÉBUT DES TESTS FTPS${NC}"
    echo "Timestamp: $TIMESTAMP"
    echo
    
    # Vérifier l'environnement
    if [ ! -d "$SRC_DIR" ]; then
        echo -e "${RED}❌ Répertoire source non trouvé: $SRC_DIR${NC}"
        exit 1
    fi
    
    # Lister les tests disponibles
    list_available_tests
    
    # Initialiser les compteurs
    local passed_categories=0
    local failed_categories=0
    local total_categories=0
    
    # Catégories de tests à exécuter
    declare -A test_categories=(
        ["FtpsProxy"]="FtpsProxyFixture|Contains=FtpsProxy"
        ["FtpsClient"]="FtpsClientFixture|Contains=FtpsClient"
        ["FtpsIndexer"]="FtpsIndexerFixture|Contains=FtpsIndexer"
        ["FtpsSettings"]="FtpsSettingsFixture|Contains=FtpsSettings"
        ["FtpsIndexerSettings"]="FtpsIndexerSettingsFixture|Contains=FtpsIndexerSettings"
        ["FtpsDirectoryItem"]="FtpsDirectoryItemFixture|Contains=FtpsDirectoryItem"
        ["FtpsIntegration"]="FtpsIntegrationFixture|Contains=FtpsIntegration"
    )
    
    # Exécuter chaque catégorie de tests
    for category in "${!test_categories[@]}"; do
        total_categories=$((total_categories + 1))
        
        if run_test_category "$category" "${test_categories[$category]}" "Tests $category"; then
            passed_categories=$((passed_categories + 1))
        else
            failed_categories=$((failed_categories + 1))
        fi
    done
    
    # Tests génériques FTPS
    total_categories=$((total_categories + 1))
    if run_test_category "FtpsGeneral" "FullyQualifiedName~Ftps" "Tests génériques FTPS"; then
        passed_categories=$((passed_categories + 1))
    else
        failed_categories=$((failed_categories + 1))
    fi
    
    # Analyser la couverture
    analyze_coverage
    
    # Générer le résumé
    generate_summary
    
    # Résultats finaux
    echo -e "${BLUE}🏁 RÉSULTATS FINAUX${NC}"
    echo "=================="
    echo "   📊 Catégories testées: $total_categories"
    echo "   ✅ Catégories réussies: $passed_categories"
    echo "   ❌ Catégories échouées: $failed_categories"
    
    if [ $failed_categories -eq 0 ]; then
        echo -e "${GREEN}🎉 TOUS LES TESTS FTPS ONT RÉUSSI !${NC}"
        echo
        echo "✅ Votre implémentation FTPS est complète et testée"
        echo "✅ Tous les composants fonctionnent correctement"
        echo "✅ Prêt pour la production"
    else
        echo -e "${YELLOW}⚠️ Certains tests ont échoué${NC}"
        echo
        echo "📋 Actions recommandées:"
        echo "   1. Vérifier les logs détaillés dans: $TEST_RESULTS_DIR"
        echo "   2. Corriger les erreurs de compilation ou logique"
        echo "   3. Relancer les tests spécifiques"
        echo "   4. Vérifier l'intégration des composants"
    fi
    
    echo
    echo -e "${BLUE}📁 FICHIERS GÉNÉRÉS${NC}"
    echo "==================="
    echo "   📊 Résultats: $TEST_RESULTS_DIR"
    echo "   🔍 Couverture: $COVERAGE_DIR"
    echo "   📋 Logs détaillés disponibles pour analyse"
    echo
    
    # Code de sortie
    if [ $failed_categories -eq 0 ]; then
        exit 0
    else
        exit 1
    fi
}

# Créer le fichier de configuration de couverture
create_coverage_config() {
    cat > "$WORKSPACE/coverlet.runsettings" << EOF
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>opencover</Format>
          <IncludeTestAssembly>false</IncludeTestAssembly>
          <Include>[NzbDrone.Core]*Ftps*</Include>
          <Exclude>[NzbDrone.Core.Test]*</Exclude>
          <ExcludeByAttribute>Obsolete,GeneratedCode,CompilerGenerated</ExcludeByAttribute>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
EOF
}

# Exécuter la configuration et les tests
create_coverage_config
main "$@"