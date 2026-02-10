@echo off
echo === TEST SIMPLE ===
echo.
echo 1. Serveur...
cd SnakeGame.Server
start dotnet run
timeout /t 3 >nul
cd ..
echo.
echo 2. Client...
cd SnakeGame.Client
dotnet run
cd ..
echo.
pause
