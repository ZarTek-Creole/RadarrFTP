@echo off
setlocal enabledelayedexpansion

:: ========================================
:: COMMANDES DE DÉVELOPPEMENT RADARR FTPS
:: ========================================

:menu
cls
echo.
echo ==========================================
echo    RADARR FTPS - COMMANDES DE DEV
echo ==========================================
echo.
echo Choisissez une action:
echo.
echo [1] Clean - Nettoyer les fichiers de build
echo [2] Restore - Restaurer les packages NuGet
echo [3] Build - Compiler le projet (Debug)
echo [4] Build Release - Compiler le projet (Release)
echo [5] Test - Lancer les tests
echo [6] Publish Windows - Créer l'exécutable Windows
echo [7] Publish All - Créer tous les exécutables
echo [8] Full Build - Clean + Restore + Build + Publish
echo [9] Quick Test - Build rapide pour tester
echo [0] Quitter
echo.
set /p choice="Votre choix (0-9): "

if "%choice%"=="1" goto clean
if "%choice%"=="2" goto restore
if "%choice%"=="3" goto build_debug
if "%choice%"=="4" goto build_release
if "%choice%"=="5" goto test
if "%choice%"=="6" goto publish_windows
if "%choice%"=="7" goto publish_all
if "%choice%"=="8" goto full_build
if "%choice%"=="9" goto quick_test
if "%choice%"=="0" goto exit
goto menu

:clean
echo.
echo [CLEAN] Nettoyage des fichiers de build...
if exist "_output" rmdir /s /q "_output"
if exist "_tests" rmdir /s /q "_tests"
dotnet clean src\Radarr.sln
echo [✓] Nettoyage terminé
pause
goto menu

:restore
echo.
echo [RESTORE] Restauration des packages...
dotnet restore src\Radarr.sln --force --no-cache
if exist "package.json" yarn install
echo [✓] Restauration terminée
pause
goto menu

:build_debug
echo.
echo [BUILD DEBUG] Compilation en mode Debug...
dotnet build src\Radarr.sln --configuration Debug --no-restore
echo [✓] Build Debug terminé
pause
goto menu

:build_release
echo.
echo [BUILD RELEASE] Compilation en mode Release...
dotnet build src\Radarr.sln --configuration Release --no-restore
echo [✓] Build Release terminé
pause
goto menu

:test
echo.
echo [TEST] Lancement des tests...
dotnet test src\Radarr.sln --configuration Debug --no-build
echo [✓] Tests terminés
pause
goto menu

:publish_windows
echo.
echo [PUBLISH WINDOWS] Création de l'exécutable Windows...
if exist "RadarrFTP-Windows" rmdir /s /q "RadarrFTP-Windows"
dotnet publish src\NzbDrone.Console\Radarr.Console.csproj -c Release -r win-x64 -f net6.0 --self-contained true -o .\RadarrFTP-Windows
echo [✓] Exécutable Windows créé dans RadarrFTP-Windows\
pause
goto menu

:publish_all
echo.
echo [PUBLISH ALL] Création de tous les exécutables...
call :publish_windows_silent
call :publish_linux_silent
call :publish_macos_silent
echo [✓] Tous les exécutables créés
pause
goto menu

:publish_windows_silent
if exist "RadarrFTP-Windows" rmdir /s /q "RadarrFTP-Windows"
dotnet publish src\NzbDrone.Console\Radarr.Console.csproj -c Release -r win-x64 -f net6.0 --self-contained true -o .\RadarrFTP-Windows
goto :eof

:publish_linux_silent
if exist "RadarrFTP-Linux" rmdir /s /q "RadarrFTP-Linux"
dotnet publish src\NzbDrone.Console\Radarr.Console.csproj -c Release -r linux-x64 -f net6.0 --self-contained true -o .\RadarrFTP-Linux
goto :eof

:publish_macos_silent
if exist "RadarrFTP-macOS" rmdir /s /q "RadarrFTP-macOS"
dotnet publish src\NzbDrone.Console\Radarr.Console.csproj -c Release -r osx-x64 -f net6.0 --self-contained true -o .\RadarrFTP-macOS
goto :eof

:full_build
echo.
echo [FULL BUILD] Build complet...
call :clean_silent
call :restore_silent
call :build_release_silent
call :publish_all_silent
echo [✓] Build complet terminé
pause
goto menu

:clean_silent
if exist "_output" rmdir /s /q "_output"
if exist "_tests" rmdir /s /q "_tests"
dotnet clean src\Radarr.sln
goto :eof

:restore_silent
dotnet restore src\Radarr.sln --force --no-cache
if exist "package.json" yarn install
goto :eof

:build_release_silent
dotnet build src\Radarr.sln --configuration Release --no-restore
goto :eof

:publish_all_silent
call :publish_windows_silent
call :publish_linux_silent
call :publish_macos_silent
goto :eof

:quick_test
echo.
echo [QUICK TEST] Build rapide pour test...
dotnet build src\Radarr.sln --configuration Debug --no-restore
if !errorlevel! equ 0 (
    echo [✓] Build rapide réussi - Prêt pour les tests
) else (
    echo [✗] Erreurs de compilation détectées
)
pause
goto menu

:exit
echo.
echo Au revoir !
exit /b 0

