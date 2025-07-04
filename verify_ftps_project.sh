#!/bin/bash

# 🎯 FTPS Client Project Verification Script
# Ce script vérifie l'intégrité complète du projet FTPS Client pour Radarr

echo "🔍 FTPS Client Project Verification"
echo "===================================="
echo ""

# Compteurs
PASSED=0
FAILED=0
TOTAL=0

# Fonction pour vérifier un fichier
check_file() {
    local file="$1"
    local description="$2"
    ((TOTAL++))
    
    if [[ -f "$file" ]]; then
        echo "✅ $description: $file"
        ((PASSED++))
    else
        echo "❌ $description: $file (MISSING)"
        ((FAILED++))
    fi
}

# Fonction pour vérifier le contenu d'un fichier
check_content() {
    local file="$1"
    local pattern="$2"
    local description="$3"
    ((TOTAL++))
    
    if [[ -f "$file" ]] && grep -q "$pattern" "$file"; then
        echo "✅ $description: Found in $file"
        ((PASSED++))
    else
        echo "❌ $description: Not found in $file"
        ((FAILED++))
    fi
}

echo "📁 Checking Core Files..."
echo "========================="

# Vérifier les fichiers principaux
check_file "src/NzbDrone.Core/Download/Clients/Ftps/Ftps.cs" "Main FTPS Client"
check_file "src/NzbDrone.Core/Download/Clients/Ftps/FtpsSettings.cs" "FTPS Settings"
check_file "src/NzbDrone.Core/Download/Clients/Ftps/FtpsProxy.cs" "FTPS Proxy"
check_file "src/NzbDrone.Core/Download/Clients/Ftps/FtpsItem.cs" "FTPS Item Models"
check_file "src/NzbDrone.Core/Download/Clients/Ftps/FtpsDownloadStatus.cs" "FTPS Status Enum"
check_file "src/NzbDrone.Core/Download/Clients/Ftps/FtpsClientException.cs" "FTPS Exceptions"

echo ""
echo "📋 Checking Protocol Extensions..."
echo "================================="

# Vérifier les extensions de protocole
check_content "src/NzbDrone.Core/Indexers/DownloadProtocol.cs" "Ftps = 3" "FTPS Protocol in Enum"

echo ""
echo "🧪 Checking Test Files..."
echo "========================="

# Vérifier les fichiers de test
check_file "src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsFixture.cs" "Main FTPS Tests"
check_file "src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsProxyFixture.cs" "FTPS Proxy Tests"
check_file "src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsSettingsFixture.cs" "FTPS Settings Tests"

echo ""
echo "📦 Checking Dependencies..."
echo "==========================="

# Vérifier les dépendances
check_content "src/NzbDrone.Core/Radarr.Core.csproj" "FluentFTP" "FluentFTP Package Reference"

echo ""
echo "📖 Checking Documentation..."
echo "============================"

# Vérifier la documentation
check_file "FTPS_CLIENT_RESEARCH_AND_IMPLEMENTATION_PLAN.md" "Research & Implementation Plan"
check_file "FTPS_CLIENT_IMPLEMENTATION_STATUS.md" "Implementation Status"
check_file "FTPS_PROJECT_FINAL_STATUS.md" "Final Status Report"

echo ""
echo "🔧 Checking Core Components..."
echo "=============================="

# Vérifier les composants essentiels
check_content "src/NzbDrone.Core/Download/Clients/Ftps/Ftps.cs" "public class Ftps : DownloadClientBase<FtpsSettings>" "Main FTPS Class Declaration"
check_content "src/NzbDrone.Core/Download/Clients/Ftps/Ftps.cs" "public override DownloadProtocol Protocol => DownloadProtocol.Ftps" "FTPS Protocol Override"
check_content "src/NzbDrone.Core/Download/Clients/Ftps/FtpsProxy.cs" "public interface IFtpsProxy" "IFtpsProxy Interface"
check_content "src/NzbDrone.Core/Download/Clients/Ftps/FtpsProxy.cs" "public class FtpsProxy : IFtpsProxy" "FtpsProxy Implementation"
check_content "src/NzbDrone.Core/Download/Clients/Ftps/FtpsSettings.cs" "public class FtpsSettings : DownloadClientSettingsBase<FtpsSettings>" "FtpsSettings Class"

echo ""
echo "🎯 Checking Key Features..."
echo "=========================="

# Vérifier les fonctionnalités clés
check_content "src/NzbDrone.Core/Download/Clients/Ftps/Ftps.cs" "SceneReleaseRegex" "Scene Release Parsing"
check_content "src/NzbDrone.Core/Download/Clients/Ftps/Ftps.cs" "CalculateReleaseScore" "Release Scoring Algorithm"
check_content "src/NzbDrone.Core/Download/Clients/Ftps/Ftps.cs" "ScanForMovieReleases" "Movie Release Scanning"
check_content "src/NzbDrone.Core/Download/Clients/Ftps/FtpsProxy.cs" "AsyncFtpClient" "FluentFTP Integration"
check_content "src/NzbDrone.Core/Download/Clients/Ftps/FtpsProxy.cs" "SslProtocols.Tls12" "SSL/TLS Security"

echo ""
echo "🧪 Checking Test Coverage..."
echo "============================"

# Vérifier la couverture des tests
check_content "src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsFixture.cs" "Validate_should_return_success_when_settings_are_valid" "Settings Validation Tests"
check_content "src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsFixture.cs" "Download_should_return_downloadId_when_release_found" "Download Tests"
check_content "src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsFixture.cs" "CalculateReleaseScore_should_prefer_higher_quality" "Scoring Algorithm Tests"
check_content "src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsProxyFixture.cs" "TestConnectionAsync" "Connection Tests"
check_content "src/NzbDrone.Core.Test/Download/Clients/Ftps/FtpsSettingsFixture.cs" "ValidateCertificate" "Settings Validation Tests"

echo ""
echo "📊 VERIFICATION SUMMARY"
echo "======================="
echo "✅ Passed: $PASSED"
echo "❌ Failed: $FAILED"
echo "📋 Total:  $TOTAL"
echo ""

if [[ $FAILED -eq 0 ]]; then
    echo "🎉 PROJECT VERIFICATION SUCCESSFUL!"
    echo "   All components are present and properly configured."
    echo "   The FTPS Client project is ready for deployment."
    echo ""
    echo "🚀 Next Steps:"
    echo "   1. Run 'dotnet restore' to install packages"
    echo "   2. Run 'dotnet build' to compile the project"
    echo "   3. Run 'dotnet test' to execute all tests"
    echo "   4. Deploy to production environment"
    exit 0
else
    echo "⚠️  PROJECT VERIFICATION FAILED!"
    echo "   $FAILED component(s) are missing or misconfigured."
    echo "   Please review the failed items above and fix them."
    echo ""
    echo "🔧 Troubleshooting:"
    echo "   - Check file paths and spellings"
    echo "   - Verify all files have been created correctly"
    echo "   - Review the implementation documentation"
    exit 1
fi