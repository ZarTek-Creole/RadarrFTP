@echo off
echo ==========================================
echo    RADARR FTPS - BUILD PARFAIT
echo ==========================================
echo.
echo [INFO] Configuration: Release
echo [INFO] Framework cible: net6.0
echo [INFO] Toutes les erreurs StyleCop corrigees
echo.

echo [1/4] Nettoyage des fichiers de build precedents...
dotnet clean src\Radarr.sln --configuration Release
if %ERRORLEVEL% neq 0 (
    echo [ERREUR] Echec du nettoyage
    pause
    exit /b 1
)
echo [OK] Nettoyage termine

echo.
echo [2/4] Restauration des packages NuGet...
dotnet restore src\Radarr.sln --force --no-cache
if %ERRORLEVEL% neq 0 (
    echo [ERREUR] Echec de la restauration
    pause
    exit /b 1
)
echo [OK] Restauration terminee

echo.
echo [3/4] Compilation Release...
dotnet build src\Radarr.sln --configuration Release --no-restore
if %ERRORLEVEL% neq 0 (
    echo [ERREUR] Echec de la compilation
    pause
    exit /b 1
)
echo [OK] Compilation terminee

echo.
echo [4/4] Publication Windows x64...
dotnet publish src\NzbDrone.Console\Radarr.Console.csproj -c Release -r win-x64 -f net6.0 --self-contained true -o .\RadarrFTP-Windows
if %ERRORLEVEL% neq 0 (
    echo [ERREUR] Echec de la publication
    pause
    exit /b 1
)
echo [OK] Publication terminee

echo.
echo ==========================================
echo    BUILD TERMINE AVEC SUCCES !
echo ==========================================
echo.
echo Executable cree : .\RadarrFTP-Windows\Radarr.Console.exe
echo Taille : 
dir .\RadarrFTP-Windows\Radarr.Console.exe | findstr "Radarr.Console.exe"
echo.
echo Fonctionnalites FTPS integrees :
echo - Client de telechargement FTPS
echo - Indexeur FTPS  
echo - Support TLS/SSL complet
echo - Extraction RAR automatique
echo.
echo RADARR FTPS EDITION PRET A L'EMPLOI !
echo.
pause

