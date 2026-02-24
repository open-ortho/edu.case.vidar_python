@echo off
REM BFD9010 GUI Launcher
REM This script starts the GUI application with integrated FHIR API server

echo ================================================
echo BFD9010 Scanner GUI with FHIR API
echo ================================================
echo.
echo Starting GUI application...
echo.
echo A small window will appear showing scanner status.
echo The FHIR API server will start on http://localhost:5000
echo.
echo The GUI will display a clickable link to the web application.
echo.
echo ================================================
echo.

REM Change to the directory containing this script
cd /d "%~dp0"

REM Check if GUI executable exists
if not exist "BFD9010.Gui\bin\Release\net8.0-windows\publish\bfd9010.exe" (
    echo ERROR: GUI executable not found!
    echo.
    echo Please run build.bat first to build the application.
    echo.
    pause
    exit /b 1
)

REM Start the GUI application
BFD9010.Gui\bin\Release\net8.0-windows\publish\bfd9010.exe %*

echo.
echo Application closed.
pause
