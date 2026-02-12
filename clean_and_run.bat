@echo off
echo === NETTOYAGE COMPLET ===
echo.
echo 1. Arret de tous les processus Snake...
taskkill /f /im SnakeGame.Server.exe 2>nul
taskkill /f /im SnakeGame.Client.exe 2>nul
taskkill /f /im dotnet.exe 2>nul
echo.
echo 2. Liberation du port 8888...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :8888') do (
    taskkill /f /pid %%a 2>nul
)
timeout /t 2 >nul
echo.
echo 3. Nettoyage des fichiers compiles...
rmdir /s /q SnakeGame.Server\bin 2>nul
rmdir /s /q SnakeGame.Server\obj 2>nul
rmdir /s /q SnakeGame.Client\bin 2>nul
rmdir /s /q SnakeGame.Client\obj 2>nul
rmdir /s /q SnakeGame.Shared\bin 2>nul
rmdir /s /q SnakeGame.Shared\obj 2>nul
echo.
echo 4. Recompilation...
dotnet build SnakeGame.sln
if errorlevel 1 (
    echo Erreur compilation
    pause
    exit /b 1
)
echo.
echo 5. Lancement du serveur...
start cmd /k "cd SnakeGame.Server && echo === SERVEUR === && dotnet run"
timeout /t 3
echo.
echo 6. Lancement du client...
start cmd /k "cd SnakeGame.Client && echo === CLIENT === && dotnet run"
echo.
echo === PRET ===
echo.
echo INSTRUCTIONS:
echo 1. Dans le client, entrez un nom
echo 2. DECOCHEZ "Observateur"
echo 3. Cliquez "CONNECTER"
echo 4. Cliquez "ACTIVER CONTROLES"
echo 5. Utilisez les FLECHES !
echo.
pause
