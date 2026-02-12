@echo off
echo === TEST RAPIDE SNAKE ===
echo.
echo 1. Arret des anciens processus...
taskkill /f /im SnakeGame.Server.exe 2>nul
taskkill /f /im SnakeGame.Client.exe 2>nul
timeout /t 2 >nul
echo.
echo 2. Lancement du serveur...
start cmd /k "cd SnakeGame.Server && echo [SERVEUR] Pret sur port 8888 && dotnet run"
timeout /t 3
echo.
echo 3. Lancement du client...
start cmd /k "cd SnakeGame.Client && echo [CLIENT] Lancement... && dotnet run"
echo.
echo === INSTRUCTIONS ===
echo 1. Dans le client: entrez un nom
echo 2. DECOCHEZ "Observateur"
echo 3. Cliquez "CONNECTER"
echo 4. Cliquez "ACTIVER CONTROLES"
echo 5. Utilisez les FLECHES !
echo.
pause
