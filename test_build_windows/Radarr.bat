@echo off
echo Starting Radarr with FTPS Integration...
echo =========================================
echo.
echo Web Interface: http://localhost:7878
echo.
echo ✅ FTPS Indexer: Settings → Indexers → Add Indexer → "FTPS Indexer"
echo ✅ FTPS Client: Settings → Download Clients → Add → "FTPS Client"
echo.
echo Starting Radarr Console Application...
Radarr.Console.exe
echo.
echo Radarr has stopped.
pause