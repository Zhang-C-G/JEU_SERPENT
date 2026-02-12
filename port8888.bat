@echo off
echo === RETOUR AU PORT 8888 ===
echo.
echo 1. Changement du port serveur...
cd SnakeGame.Server
powershell -Command "(Get-Content Program.cs) -replace '8889', '8888' -replace '8890', '8888' | Set-Content Program.cs"
cd ..
echo.
echo 2. Changement du port client...
cd SnakeGame.Client
powershell -Command "(Get-Content MainForm.cs) -replace '8889', '8888' -replace '8890', '8888' | Set-Content MainForm.cs"
cd ..
echo.
echo 3. Arret des processus...
taskkill /f /im SnakeGame.Server.exe 2>nul
taskkill /f /im dotnet.exe 2>nul
timeout /t 2 >nul
echo.
echo 4. Nettoyage...
rmdir /s /q SnakeGame.Server\bin 2>nul
rmdir /s /q SnakeGame.Server\obj 2>nul
rmdir /s /q SnakeGame.Client\bin 2>nul
rmdir /s /q SnakeGame.Client\obj 2>nul
echo.
echo 5. Recompilation...
dotnet build SnakeGame.sln
if errorlevel 1 (
    echo Erreur compilation
    pause
    exit /b 1
)
echo.
echo 6. Lancement serveur...
start cmd /k "cd SnakeGame.Server && echo === SERVEUR SUR PORT 8888 === && dotnet run"
timeout /t 3
echo.
echo 7. Lancement client...
start cmd /k "cd SnakeGame.Client && echo === CLIENT PORT 8888 === && dotnet run"
echo.
echo === PORT 8888 RESTAURE ===
echo.
pause
