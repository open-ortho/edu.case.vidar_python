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
echo To scan, navigate to: https://wingate.case.edu/bfd9000/
echo.
echo ================================================
echo.

REM Change to the directory containing this script
cd /d "%~dp0"

REM Start the GUI application
BFD9010.Gui\bin\Release\net8.0-windows\bfd9010_fhir32.exe

echo.
echo Application closed.
pause
