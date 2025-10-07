@echo off
REM BFD9010 CLI Launcher
REM This script starts the command-line interface for the BFD9010 scanner

echo ================================================
echo BFD9010 Scanner CLI
echo ================================================
echo.

REM Change to the directory containing this script
cd /d "%~dp0"

REM Start the CLI application
vidar_app\bin\Release\net8.0\bfd9010.exe

echo.
echo Application closed.
pause
