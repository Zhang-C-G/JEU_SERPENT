@echo off
echo === NETTOYAGE COMPLET ===
echo.
echo 1. Arret des processus...
taskkill /f /im SnakeGame.Server.exe 2>nul
taskkill /f /im SnakeGame.Client.exe 2>nul
taskkill /f /im dotnet.exe 2>nul
timeout /t 2 >nul
echo.
echo 2. Nettoyage des fichiers...
rmdir /s /q SnakeGame.Server\bin 2>nul
rmdir /s /q SnakeGame.Server\obj 2>nul
rmdir /s /q SnakeGame.Client\bin 2>nul
rmdir /s /q SnakeGame.Client\obj 2>nul
rmdir /s /q SnakeGame.Shared\bin 2>nul
rmdir /s /q SnakeGame.Shared\obj 2>nul
echo.
echo 3. Recompilation...
dotnet build SnakeGame.sln
if errorlevel 1 (
    echo Erreur compilation
    pause
    exit /b 1
)
echo.
echo 4. Lancement serveur...
start cmd /k "cd SnakeGame.Server && echo === SERVEUR === && dotnet run"
timeout /t 3
echo.
echo 5. Lancement client...
start cmd /k "cd SnakeGame.Client && echo === CLIENT === && dotnet run"
echo.
echo === PRET ===
echo.
pause
