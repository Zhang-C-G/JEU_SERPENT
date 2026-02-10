@echo off
echo === SNAKE MULTIJOUEUR ===
echo.
echo 1. Lancement du serveur...
start "Snake Server" cmd /k "cd SnakeGame.Server && echo === SERVEUR === && echo Attente de joueurs... && dotnet run"
timeout /t 3 >nul
echo.
echo 2. Lancement du client 1 (Joueur)...
start "Snake Client 1" cmd /k "cd SnakeGame.Client && echo === CLIENT 1 === && echo Entrez un nom et cliquez 'Se connecter' && dotnet run"
timeout /t 2 >nul
echo.
echo 3. Lancement du client 2 (Observateur)...
start "Snake Client 2" cmd /k "cd SnakeGame.Client && echo === CLIENT 2 === && echo Cochez 'Observateur' puis 'Se connecter' && dotnet run"
timeout /t 2 >nul
echo.
echo 4. Lancement du client 3 (Joueur)...
start "Snake Client 3" cmd /k "cd SnakeGame.Client && echo === CLIENT 3 === && echo Entrez un nom et cliquez 'Se connecter' && dotnet run"
echo.
echo === INSTRUCTIONS ===
echo Pour chaque client:
echo 1. Entrez un nom unique
echo 2. Pour JOUER: Decochez "Observateur"
echo 3. Pour OBSERVER: Cochez "Observateur"
echo 4. Cliquez "Se connecter"
echo 5. Utilisez les FLECHES pour jouer
echo.
echo Le cercle DORE rend invincible 3 secondes!
echo.
pause
