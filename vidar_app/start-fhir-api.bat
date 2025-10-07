@echo off
REM BFD9010 FHIR API Server Launcher
REM This script starts the FHIR API server for the BFD9010 scanner

echo ================================================
echo BFD9010 FHIR API Server
echo ================================================
echo.
echo Starting FHIR API server on http://localhost:5000
echo.
echo The scanner will be initialized automatically.
echo.
echo To scan, navigate to: https://wingate.case.edu/bfd9000/
echo.
echo Press Ctrl+C to stop the server.
echo.
echo ================================================
echo.

REM Change to the directory containing this script
cd /d "%~dp0"

REM Start the FHIR API server
BFD9010.FhirApi\bin\Release\net8.0\BFD9010.FhirApi.exe

echo.
echo Server stopped.
pause
