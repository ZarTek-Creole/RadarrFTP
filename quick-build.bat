@echo off
:: ========================================
:: BUILD RAPIDE RADARR FTPS
:: ========================================

echo.
echo ==========================================
echo      RADARR FTPS - BUILD RAPIDE
echo ==========================================
echo.

:: Vérification du répertoire
if not exist "src\Radarr.sln" (
    echo ERREUR: Pas dans le bon répertoire !
    echo Naviguez vers le dossier RadarrFTP-develop
    pause
    exit /b 1
)

echo [1/4] Nettoyage rapide...
dotnet clean src\Radarr.sln --configuration Release > nul 2>&1

echo [2/4] Restauration des packages...
dotnet restore src\Radarr.sln --no-cache > nul 2>&1

echo [3/4] Compilation...
dotnet build src\Radarr.sln --configuration Release --no-restore
if %errorlevel% neq 0 (
    echo.
    echo ❌ ERREUR DE COMPILATION !
    pause
    exit /b 1
)

echo [4/4] Création de l'exécutable Windows...
if exist "RadarrFTP-Windows" rmdir /s /q "RadarrFTP-Windows" > nul 2>&1
dotnet publish src\NzbDrone.Console\Radarr.Console.csproj -c Release -r win-x64 -f net6.0 --self-contained true -o .\RadarrFTP-Windows

if %errorlevel% equ 0 (
    echo.
    echo ✅ BUILD RÉUSSI !
    echo.
    echo 📁 Exécutable créé: RadarrFTP-Windows\Radarr.exe
    echo 🚀 Prêt à utiliser avec support FTPS !
    echo.
) else (
    echo.
    echo ❌ ERREUR DE PUBLICATION !
)

pause

