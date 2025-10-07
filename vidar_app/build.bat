@echo off
REM BFD9010 Build Script
REM Builds all projects in Release configuration

echo ================================================
echo BFD9010 Scanner Software - Build Script
echo ================================================
echo.

REM Change to the directory containing this script
cd /d "%~dp0"

echo Building solution...
echo.

REM Build the solution
dotnet build vidar_app.sln --configuration Release

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ================================================
    echo Build Successful!
    echo ================================================
    echo.
    echo Output locations:
    echo   CLI: vidar_app\bin\Release\net8.0\bfd9010.exe
    echo   FHIR API: BFD9010.FhirApi\bin\Release\net8.0\BFD9010.FhirApi.exe
    echo   GUI: BFD9010.Gui\bin\Release\net8.0-windows\bfd9010_fhir32.exe
    echo.
    echo You can now run:
    echo   - start-cli.bat (for CLI)
    echo   - start-fhir-api.bat (for API server)
    echo   - start-gui.bat (for GUI with integrated API)
    echo.
) else (
    echo.
    echo ================================================
    echo Build Failed!
    echo ================================================
    echo.
    echo Please check the error messages above.
    echo.
)

pause
