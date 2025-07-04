# 🎯 Script de Lancement Radarr avec Support FTPS (Windows)
# ==========================================================

param(
    [string]$Port = "7878",
    [string]$Configuration = "Release"
)

# Configuration
$RadarrUrl = "http://localhost:$Port"

# Fonctions utilitaires
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

# Header
Write-Host ""
Write-Host "🚀 Lancement Radarr avec Support FTPS" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""

# Vérifier les prérequis
Write-Status "Vérification des prérequis..."

# Vérifier .NET
if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error ".NET 6.0 n'est pas installé"
    Write-Host "Installez .NET 6.0 depuis: https://dotnet.microsoft.com/download/dotnet/6.0"
    exit 1
}

$dotnetVersion = dotnet --version
Write-Success ".NET détecté: $dotnetVersion"

# Vérifier Node.js (optionnel)
if (Get-Command "node" -ErrorAction SilentlyContinue) {
    $nodeVersion = node --version
    Write-Success "Node.js détecté: $nodeVersion"
} else {
    Write-Warning "Node.js non détecté (requis uniquement pour recompiler le frontend)"
}

# Vérifier le répertoire source
if (!(Test-Path "src")) {
    Write-Error "Répertoire 'src' non trouvé"
    Write-Error "Assurez-vous d'être dans le répertoire racine de Radarr"
    exit 1
}

Write-Success "Répertoire source trouvé"

# Vérifier l'intégration FTPS
Write-Status "Vérification de l'intégration FTPS..."

$ftpsFiles = @(
    "src\NzbDrone.Core\Download\Clients\Ftps\Ftps.cs",
    "src\NzbDrone.Core\Download\Clients\Ftps\FtpsSettings.cs",
    "src\NzbDrone.Core\Download\Clients\Ftps\FtpsProxy.cs"
)

foreach ($file in $ftpsFiles) {
    if (!(Test-Path $file)) {
        Write-Error "Fichier $file non trouvé"
        Write-Error "L'intégration FTPS n'est pas présente"
        exit 1
    }
}

Write-Success "Intégration FTPS validée"

# Vérifier FluentFTP dans le projet
$csprojContent = Get-Content "src\NzbDrone.Core\Radarr.Core.csproj" -Raw
if ($csprojContent -notmatch "FluentFTP") {
    Write-Error "Dépendance FluentFTP non trouvée dans le projet"
    exit 1
}

Write-Success "Dépendance FluentFTP validée"

# Restaurer les dépendances
Write-Status "Restauration des dépendances NuGet..."
Set-Location src

try {
    dotnet restore
    Write-Success "Dépendances restaurées"
} catch {
    Write-Error "Erreur lors de la restauration des dépendances"
    Write-Error $_.Exception.Message
    exit 1
}

# Compiler le projet Core
Write-Status "Compilation du Core Radarr..."
try {
    dotnet build "NzbDrone.Core\Radarr.Core.csproj" --configuration $Configuration --no-restore
    Write-Success "Core compilé avec succès"
} catch {
    Write-Error "Erreur lors de la compilation du Core"
    Write-Error $_.Exception.Message
    exit 1
}

# Compiler l'Host
Write-Status "Compilation de l'Host Radarr..."
try {
    dotnet build "Radarr.Host\Radarr.Host.csproj" --configuration $Configuration --no-restore
    Write-Success "Host compilé avec succès"
} catch {
    Write-Error "Erreur lors de la compilation de l'Host"
    Write-Error $_.Exception.Message
    exit 1
}

# Vérifier le frontend
if ((Test-Path "..\frontend") -and (Get-Command "npm" -ErrorAction SilentlyContinue)) {
    Write-Status "Frontend détecté, vérification..."
    if (!(Test-Path "..\frontend\build")) {
        Write-Warning "Frontend non compilé, compilation en cours..."
        Set-Location ..\frontend
        npm install --silent
        npm run build --silent
        Set-Location ..\src
        Write-Success "Frontend compilé"
    } else {
        Write-Success "Frontend déjà compilé"
    }
}

# Démarrer Radarr
Write-Host ""
Write-Status "Démarrage de Radarr avec support FTPS..."
Write-Status "URL d'accès: $RadarrUrl"
Write-Host ""

# Aller dans le répertoire Host
Set-Location "Radarr.Host"

# Gestion de l'arrêt propre
$cleanup = {
    Write-Status "Arrêt de Radarr..."
    exit 0
}

# Capturer Ctrl+C
[Console]::CancelKeyPress += $cleanup

Write-Success "Radarr démarré avec succès !"
Write-Host ""
Write-Host "📋 Informations importantes :" -ForegroundColor White
Write-Host "   • URL d'accès: $RadarrUrl" -ForegroundColor Cyan
Write-Host "   • Support FTPS: ✅ Activé" -ForegroundColor Green
Write-Host "   • Configuration: Settings → Download Clients → Add → FTPS" -ForegroundColor Yellow
Write-Host "   • Test rapide: Serveur Rebex (test.rebex.net)" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔧 Actions disponibles :" -ForegroundColor White
Write-Host "   • Ctrl+C pour arrêter" -ForegroundColor Gray
Write-Host "   • Ouvrir $RadarrUrl dans votre navigateur" -ForegroundColor Gray
Write-Host ""
Write-Host "🎯 Prochaines étapes :" -ForegroundColor White
Write-Host "   1. Configurer un client FTPS dans Settings → Download Clients" -ForegroundColor Yellow
Write-Host "   2. Ajouter un film pour tester le téléchargement" -ForegroundColor Yellow
Write-Host "   3. Surveiller dans Activity → Queue" -ForegroundColor Yellow
Write-Host ""

# Essayer d'ouvrir le navigateur
try {
    Start-Process $RadarrUrl
    Write-Success "Navigateur ouvert automatiquement"
} catch {
    Write-Warning "Impossible d'ouvrir le navigateur automatiquement"
    Write-Host "   Ouvrez manuellement: $RadarrUrl" -ForegroundColor Gray
}

Write-Host ""

# Démarrer Radarr
try {
    dotnet run --configuration $Configuration --urls=$RadarrUrl
} catch {
    Write-Error "Erreur lors du démarrage de Radarr"
    Write-Error $_.Exception.Message
    exit 1
}