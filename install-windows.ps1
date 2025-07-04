# 🖥️ Script d'Installation Automatique Windows - FTPS Client Radarr
# PowerShell Script pour installer et démarrer l'application

param(
    [string]$Port = "5000",
    [switch]$SkipDotNetInstall
)

Write-Host "🚀 Installation FTPS Client Radarr pour Windows" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green
Write-Host ""

# Vérifier si on est en mode administrateur
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")

if (!$isAdmin) {
    Write-Host "⚠️  ATTENTION: Exécutez ce script en tant qu'administrateur pour une installation complète" -ForegroundColor Yellow
    Write-Host ""
}

# Fonction pour vérifier si une commande existe
function Test-CommandExists {
    param($command)
    $null = Get-Command $command -ErrorAction SilentlyContinue
    return $?
}

# Étape 1: Vérifier/Installer .NET 6.0
Write-Host "📋 Étape 1: Vérification de .NET 6.0..." -ForegroundColor Blue

if (Test-CommandExists "dotnet") {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET détecté: $dotnetVersion" -ForegroundColor Green
    
    # Vérifier si c'est .NET 6.0
    $dotnetInfo = dotnet --info | Select-String "Microsoft.NETCore.App 6\."
    if ($dotnetInfo) {
        Write-Host "✅ .NET 6.0 Runtime trouvé" -ForegroundColor Green
    } else {
        Write-Host "⚠️  .NET 6.0 Runtime non détecté" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ .NET non installé" -ForegroundColor Red
    
    if (!$SkipDotNetInstall) {
        Write-Host "📥 Installation de .NET 6.0..." -ForegroundColor Blue
        
        # Essayer winget d'abord
        if (Test-CommandExists "winget") {
            Write-Host "   Utilisation de winget..." -ForegroundColor Gray
            winget install Microsoft.DotNet.SDK.6 --silent
        } else {
            Write-Host "   Téléchargement manuel requis:" -ForegroundColor Yellow
            Write-Host "   1. Allez sur: https://dotnet.microsoft.com/download/dotnet/6.0" -ForegroundColor Yellow
            Write-Host "   2. Téléchargez 'ASP.NET Core Runtime 6.0'" -ForegroundColor Yellow
            Write-Host "   3. Exécutez l'installeur" -ForegroundColor Yellow
            Write-Host "   4. Relancez ce script" -ForegroundColor Yellow
            Read-Host "   Appuyez sur Entrée après l'installation"
        }
    }
}

# Étape 2: Vérifier le répertoire du projet
Write-Host ""
Write-Host "📁 Étape 2: Vérification du projet..." -ForegroundColor Blue

$projectPath = "radarr-ftps-web"
if (Test-Path $projectPath) {
    Write-Host "✅ Répertoire du projet trouvé: $projectPath" -ForegroundColor Green
    Set-Location $projectPath
} else {
    Write-Host "❌ Répertoire du projet non trouvé: $projectPath" -ForegroundColor Red
    Write-Host "   Assurez-vous d'être dans le bon répertoire" -ForegroundColor Yellow
    exit 1
}

# Étape 3: Restaurer les dépendances
Write-Host ""
Write-Host "📦 Étape 3: Restauration des dépendances..." -ForegroundColor Blue

try {
    dotnet restore
    Write-Host "✅ Dépendances restaurées avec succès" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur lors de la restauration des dépendances" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Étape 4: Compiler l'application
Write-Host ""
Write-Host "🔨 Étape 4: Compilation de l'application..." -ForegroundColor Blue

try {
    dotnet build --configuration Release
    Write-Host "✅ Compilation réussie" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur lors de la compilation" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Étape 5: Configurer le firewall (si admin)
Write-Host ""
Write-Host "🔥 Étape 5: Configuration du firewall..." -ForegroundColor Blue

if ($isAdmin) {
    try {
        # Vérifier si la règle existe déjà
        $existingRule = Get-NetFirewallRule -DisplayName "FTPS Client Radarr" -ErrorAction SilentlyContinue
        if ($existingRule) {
            Write-Host "✅ Règle firewall déjà configurée" -ForegroundColor Green
        } else {
            New-NetFirewallRule -DisplayName "FTPS Client Radarr" -Direction Inbound -Protocol TCP -LocalPort $Port -Action Allow
            Write-Host "✅ Règle firewall ajoutée pour le port $Port" -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠️  Impossible de configurer le firewall automatiquement" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️  Droits admin requis pour configurer le firewall" -ForegroundColor Yellow
    Write-Host "   Ajoutez manuellement le port $Port si nécessaire" -ForegroundColor Gray
}

# Étape 6: Démarrer l'application
Write-Host ""
Write-Host "🚀 Étape 6: Démarrage de l'application..." -ForegroundColor Blue

$url = "http://localhost:$Port"
Write-Host "   URL: $url" -ForegroundColor Cyan

# Créer un job pour démarrer l'application en arrière-plan
$job = Start-Job -ScriptBlock {
    param($Port)
    Set-Location $using:PWD
    dotnet run --urls="http://localhost:$Port"
} -ArgumentList $Port

Write-Host "✅ Application démarrée en arrière-plan (Job ID: $($job.Id))" -ForegroundColor Green

# Attendre que l'application soit prête
Write-Host ""
Write-Host "⏳ Attente du démarrage de l'application..." -ForegroundColor Blue

for ($i = 1; $i -le 10; $i++) {
    Start-Sleep -Seconds 2
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Application prête et accessible !" -ForegroundColor Green
            break
        }
    } catch {
        Write-Host "   Tentative $i/10..." -ForegroundColor Gray
    }
    
    if ($i -eq 10) {
        Write-Host "⚠️  L'application met du temps à démarrer" -ForegroundColor Yellow
        Write-Host "   Vérifiez manuellement: $url" -ForegroundColor Yellow
    }
}

# Étape 7: Ouvrir le navigateur
Write-Host ""
Write-Host "🌐 Étape 7: Ouverture du navigateur..." -ForegroundColor Blue

try {
    Start-Process $url
    Write-Host "✅ Navigateur ouvert sur $url" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Impossible d'ouvrir le navigateur automatiquement" -ForegroundColor Yellow
    Write-Host "   Ouvrez manuellement: $url" -ForegroundColor Yellow
}

# Résumé final
Write-Host ""
Write-Host "🎉 INSTALLATION TERMINÉE AVEC SUCCÈS !" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Résumé:" -ForegroundColor White
Write-Host "   • Application démarrée: ✅" -ForegroundColor Green
Write-Host "   • URL d'accès: $url" -ForegroundColor Cyan
Write-Host "   • Job PowerShell: $($job.Id)" -ForegroundColor Gray
Write-Host ""
Write-Host "🚀 Prochaines étapes:" -ForegroundColor White
Write-Host "   1. Testez la connexion avec le preset 'Rebex Test'" -ForegroundColor Yellow
Write-Host "   2. Configurez vos propres serveurs FTPS" -ForegroundColor Yellow
Write-Host "   3. Intégrez dans Radarr si les tests sont concluants" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔧 Commandes utiles:" -ForegroundColor White
Write-Host "   • Arrêter l'application: Stop-Job $($job.Id)" -ForegroundColor Gray
Write-Host "   • Voir les logs: Get-Job $($job.Id) | Receive-Job" -ForegroundColor Gray
Write-Host "   • Redémarrer: dotnet run --urls=http://localhost:$Port" -ForegroundColor Gray
Write-Host ""
Write-Host "🎯 L'application est maintenant prête à l'utilisation !" -ForegroundColor Green

# Maintenir le script ouvert
Write-Host ""
Write-Host "Appuyez sur Entrée pour fermer ce script (l'application continuera à tourner)..." -ForegroundColor DarkGray
Read-Host