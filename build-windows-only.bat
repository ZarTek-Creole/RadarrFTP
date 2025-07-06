@echo off

echo ==========================================
echo    RADARR FTPS - BUILD WINDOWS SIMPLE
echo ==========================================
echo.

if not exist "src\Radarr.sln" (
    echo ERREUR: Pas dans le bon répertoire !
    pause
    exit /b 1
)

echo [1/5] Nettoyage...
if exist "RadarrFTP-Windows" rmdir /s /q "RadarrFTP-Windows"
dotnet clean src\Radarr.sln --configuration Release

echo [2/5] Restauration...
dotnet restore src\Radarr.sln --force --no-cache

echo [3/5] Frontend (optionnel)...
if exist "package.json" (
    yarn install --ignore-engines
)

echo [4/5] Compilation...
dotnet build src\Radarr.sln --configuration Release --no-restore
if %errorlevel% neq 0 (
    echo ERREUR DE COMPILATION !
    pause
    exit /b 1
)

echo [5/5] Publication Windows...
dotnet publish src\NzbDrone.Console\Radarr.Console.csproj -c Release -r win-x64 -f net6.0 --self-contained true -o .\RadarrFTP-Windows
if %errorlevel% neq 0 (
    echo ERREUR DE PUBLICATION !
    pause
    exit /b 1
)

echo.
echo ==========================================
echo           BUILD RÉUSSI !
echo ==========================================
echo.
echo Exécutable créé: RadarrFTP-Windows\Radarr.exe
echo.
echo Pour démarrer:
echo 1. Ouvrez RadarrFTP-Windows\
echo 2. Lancez Radarr.exe
echo 3. Allez sur http://localhost:7878
echo.

pause

