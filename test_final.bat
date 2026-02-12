@echo off
echo === SNAKE MULTI - TEST RAPIDE ===
echo.
echo 1. Arret des anciens processus...
taskkill /f /im SnakeGame.Server.exe 2>nul
taskkill /f /im SnakeGame.Client.exe 2>nul
timeout /t 2 >nul
echo.
echo 2. Lancement du serveur...
start cmd /k "cd SnakeGame.Server && echo [SERVEUR] Demarrage sur port 8888... && dotnet run"
timeout /t 3
echo.
echo 3. Lancement du client...
start cmd /k "cd SnakeGame.Client && echo [CLIENT] Lancement du jeu... && dotnet run"
echo.
echo === INSTRUCTIONS ===
echo.
echo 1. Dans le client: 
echo    - Nom: Joueur1
echo    - DECOCHEZ "Observateur"
echo    - Cliquez "CONNECTER"
echo    - Cliquez "ACTIVER CONTROLES"
echo    - Utilisez les FLECHES !
echo.
echo 2. Regardez la fenetre SERVEUR pour voir les connexions
echo.
pause
