@echo off
echo === SNAKE MULTIJOUEUR - MULTI-SALONS ===
echo.
echo 1. Lancement du serveur...
start cmd /k "cd SnakeGame.Server && echo === SERVEUR MULTI-SALONS === && echo Commandes: rooms, create [nom], exit && dotnet run"
timeout /t 3 >nul
echo.
echo 2. Lancement du client 1...
start cmd /k "cd SnakeGame.Client && echo === CLIENT 1 === && dotnet run"
timeout /t 2 >nul
echo.
echo 3. Lancement du client 2...
start cmd /k "cd SnakeGame.Client && echo === CLIENT 2 === && dotnet run"
timeout /t 2 >nul
echo.
echo 4. Lancement du client 3...
start cmd /k "cd SnakeGame.Client && echo === CLIENT 3 === && dotnet run"
echo.
echo === NOUVEAUTES ===
echo - MULTI-SALONS : plusieurs parties en meme temps
echo - Selectionnez un salon dans la liste
echo - Creez vos propres salons
echo - Rafraichissez la liste des salons
echo.
echo Commandes serveur:
echo - rooms : liste des salons
echo - create [nom] : creer un salon
echo - exit : arreter le serveur
echo.
pause
