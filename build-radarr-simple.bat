@echo off
setlocal

echo.
echo ==========================================
echo    RADARR FTPS - SCRIPT DE BUILD AUTO
echo ==========================================
echo.

REM Vérifier si on est dans le bon répertoire
if not exist "src\Radarr.sln" (
    echo ERREUR: Fichier src\Radarr.sln non trouvé !
    echo Assurez-vous d'être dans le répertoire racine du projet RadarrFTP.
    pause
    exit /b 1
)

set BUILD_CONFIG=Release
set TARGET_FRAMEWORK=net6.0

echo [INFO] Configuration: %BUILD_CONFIG%
echo [INFO] Framework cible: %TARGET_FRAMEWORK%
echo.

REM ========================================
REM ÉTAPE 1: NETTOYAGE
REM ========================================
echo [1/6] Nettoyage des fichiers de build précédents...
if exist "_output" rmdir /s /q "_output"
if exist "_tests" rmdir /s /q "_tests"
if exist "RadarrFTP-Windows" rmdir /s /q "RadarrFTP-Windows"
if exist "RadarrFTP-Linux" rmdir /s /q "RadarrFTP-Linux"
if exist "RadarrFTP-macOS" rmdir /s /q "RadarrFTP-macOS"
if exist "RadarrFTP-Single" rmdir /s /q "RadarrFTP-Single"

dotnet clean src\Radarr.sln --configuration %BUILD_CONFIG%
if %errorlevel% neq 0 (
    echo ERREUR: Échec du nettoyage !
    pause
    exit /b 1
)
echo [OK] Nettoyage terminé
echo.

REM ========================================
REM ÉTAPE 2: RESTAURATION DES PACKAGES
REM ========================================
echo [2/6] Restauration des packages NuGet...
dotnet restore src\Radarr.sln --force --no-cache
if %errorlevel% neq 0 (
    echo ERREUR: Échec de la restauration des packages !
    pause
    exit /b 1
)
echo [OK] Restauration terminée
echo.

REM ========================================
REM ÉTAPE 3: INSTALLATION DES DÉPENDANCES FRONTEND
REM ========================================
echo [3/6] Installation des dépendances frontend...
if exist "package.json" (
    echo [INFO] Tentative avec Yarn...
    yarn install --ignore-engines
    if %errorlevel% equ 0 (
        echo [OK] Yarn install réussi
    ) else (
        echo [WARN] Yarn a échoué, tentative avec npm...
        npm install --legacy-peer-deps
        if %errorlevel% equ 0 (
            echo [OK] npm install réussi avec --legacy-peer-deps
        ) else (
            echo [WARN] npm a aussi échoué, continuons sans frontend...
        )
    )
) else (
    echo [INFO] Pas de package.json trouvé, étape ignorée
)
echo [OK] Dépendances frontend traitées
echo.

REM ========================================
REM ÉTAPE 4: COMPILATION
REM ========================================
echo [4/6] Compilation du projet...
dotnet build src\Radarr.sln --configuration %BUILD_CONFIG% --no-restore
if %errorlevel% neq 0 (
    echo ERREUR: Échec de la compilation !
    pause
    exit /b 1
)
echo [OK] Compilation terminée
echo.

REM ========================================
REM ÉTAPE 5: PUBLICATION WINDOWS
REM ========================================
echo [5/6] Publication Windows 64-bit...
dotnet publish src\NzbDrone.Console\Radarr.Console.csproj -c %BUILD_CONFIG% -r win-x64 -f %TARGET_FRAMEWORK% --self-contained true -o .\RadarrFTP-Windows
if %errorlevel% neq 0 (
    echo ERREUR: Échec de la publication Windows !
    pause
    exit /b 1
)
echo [OK] Windows 64-bit publié dans RadarrFTP-Windows\
echo.

REM ========================================
REM ÉTAPE 6: RÉSUMÉ
REM ========================================
echo [6/6] Build terminé avec succès !
echo.
echo ==========================================
echo           BUILD TERMINÉ AVEC SUCCÈS !
echo ==========================================
echo.
echo EXÉCUTABLE GÉNÉRÉ:
echo 📁 RadarrFTP-Windows\Radarr.exe
echo.
echo POUR DÉMARRER:
echo 1. Naviguez vers RadarrFTP-Windows\
echo 2. Lancez Radarr.exe
echo 3. Ouvrez http://localhost:7878 dans votre navigateur
echo 4. Configurez vos serveurs FTPS dans Settings
echo.
echo FONCTIONNALITÉS INCLUSES:
echo - Support FTPS complet (TLS/SSL)
echo - Client de téléchargement FTPS
echo - Indexeur FTPS
echo - Extraction RAR automatique
echo - Interface web Radarr standard
echo.

pause

