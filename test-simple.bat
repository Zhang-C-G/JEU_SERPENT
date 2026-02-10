@echo off
echo === TEST RAPIDE SNAKE MULTIJOUEUR ===
echo.
echo 1. Lancement du serveur...
start cmd /k "cd SnakeGame.Server && echo SERVEUR SNAKE - Port 8888 && echo En attente de joueurs... && dotnet run"
timeout /t 3 >nul
echo.
echo 2. Lancement d'un client (Joueur)...
start cmd /k "cd SnakeGame.Client && echo CLIENT 1 - Joueur && echo 1. Entrez un nom && echo 2. DECOCHEZ 'Observateur' && echo 3. Cliquez 'Se connecter' && echo 4. Utilisez les FLECHES && dotnet run"
timeout /t 2 >nul
echo.
echo 3. Lancement d'un client (Observateur)...
start cmd /k "cd SnakeGame.Client && echo CLIENT 2 - Observateur && echo 1. Entrez un nom && echo 2. COCHEZ 'Observateur' && echo 3. Cliquez 'Se connecter' && dotnet run"
echo.
echo Instructions:
echo - Joueur: Utilisez les FLECHES pour deplacer
echo - Observateur: Regardez seulement
echo - Le cercle DORE rend invincible 3 secondes!
echo.
pause
