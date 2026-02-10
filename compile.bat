@echo off
echo === SNAKE MULTIJOUEUR - COMPILATION ===
echo.
echo Compilation en cours...
dotnet build
if errorlevel 1 (
    echo ERREUR de compilation
    pause
    exit /b 1
)
echo.
echo === COMPILATION TERMINEE ===
echo.
pause
