@echo off
:: 🖥️ Script d'Installation Batch Windows - FTPS Client Radarr
:: Script batch pour installer et démarrer l'application

setlocal enabledelayedexpansion

echo.
echo 🚀 Installation FTPS Client Radarr pour Windows
echo =================================================
echo.

:: Vérifier si dotnet est installé
where dotnet >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ .NET détecté
    dotnet --version
) else (
    echo ❌ .NET non installé
    echo.
    echo 📥 Installation de .NET 6.0 requise:
    echo 1. Allez sur: https://dotnet.microsoft.com/download/dotnet/6.0
    echo 2. Téléchargez 'ASP.NET Core Runtime 6.0'
    echo 3. Exécutez l'installeur
    echo 4. Relancez ce script
    echo.
    pause
    exit /b 1
)

:: Vérifier le répertoire du projet
echo.
echo 📁 Vérification du projet...
if exist "radarr-ftps-web" (
    echo ✅ Répertoire du projet trouvé: radarr-ftps-web
    cd radarr-ftps-web
) else (
    echo ❌ Répertoire du projet non trouvé: radarr-ftps-web
    echo    Assurez-vous d'être dans le bon répertoire
    pause
    exit /b 1
)

:: Restaurer les dépendances
echo.
echo 📦 Restauration des dépendances...
dotnet restore
if %errorlevel% equ 0 (
    echo ✅ Dépendances restaurées avec succès
) else (
    echo ❌ Erreur lors de la restauration des dépendances
    pause
    exit /b 1
)

:: Compiler l'application
echo.
echo 🔨 Compilation de l'application...
dotnet build --configuration Release
if %errorlevel% equ 0 (
    echo ✅ Compilation réussie
) else (
    echo ❌ Erreur lors de la compilation
    pause
    exit /b 1
)

:: Configurer le firewall (nécessite des droits admin)
echo.
echo 🔥 Configuration du firewall...
net session >nul 2>&1
if %errorlevel% equ 0 (
    :: Droits admin disponibles
    netsh advfirewall firewall show rule name="FTPS Client Radarr" >nul 2>&1
    if %errorlevel% equ 0 (
        echo ✅ Règle firewall déjà configurée
    ) else (
        netsh advfirewall firewall add rule name="FTPS Client Radarr" dir=in action=allow protocol=TCP localport=5000
        echo ✅ Règle firewall ajoutée pour le port 5000
    )
) else (
    echo ⚠️  Droits admin requis pour configurer le firewall
    echo    Relancez en tant qu'administrateur si nécessaire
)

:: Démarrer l'application
echo.
echo 🚀 Démarrage de l'application...
echo    URL: http://localhost:5000
echo.

:: Créer un fichier batch temporaire pour démarrer l'application
echo @echo off > start_app.bat
echo echo 🚀 FTPS Client Radarr en cours d'exécution... >> start_app.bat
echo echo    URL: http://localhost:5000 >> start_app.bat
echo echo    Fermez cette fenêtre pour arrêter l'application >> start_app.bat
echo echo. >> start_app.bat
echo dotnet run --urls=http://localhost:5000 >> start_app.bat
echo pause >> start_app.bat

:: Démarrer l'application en arrière-plan
start "FTPS Client Radarr" start_app.bat

:: Attendre que l'application soit prête
echo ⏳ Attente du démarrage de l'application...
timeout /t 5 /nobreak >nul

:: Essayer d'ouvrir le navigateur
echo.
echo 🌐 Ouverture du navigateur...
start http://localhost:5000

:: Résumé final
echo.
echo 🎉 INSTALLATION TERMINÉE AVEC SUCCÈS !
echo =====================================
echo.
echo 📋 Résumé:
echo    • Application démarrée: ✅
echo    • URL d'accès: http://localhost:5000
echo    • Fenêtre séparée ouverte pour l'application
echo.
echo 🚀 Prochaines étapes:
echo    1. Testez la connexion avec le preset 'Rebex Test'
echo    2. Configurez vos propres serveurs FTPS
echo    3. Intégrez dans Radarr si les tests sont concluants
echo.
echo 🔧 Notes:
echo    • L'application tourne dans une fenêtre séparée
echo    • Fermez cette fenêtre pour arrêter l'application
echo    • Pour redémarrer: dotnet run --urls=http://localhost:5000
echo.
echo 🎯 L'application est maintenant prête à l'utilisation !
echo.

:: Nettoyer le fichier temporaire après un délai
timeout /t 2 /nobreak >nul
del start_app.bat >nul 2>&1

pause