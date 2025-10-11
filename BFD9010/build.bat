@echo off
REM BFD9010 Build Script
REM Builds all projects in Release configuration

echo ================================================
echo BFD9010 Scanner Software - Build Script
echo ================================================
echo.

REM Change to the directory containing this script
cd /d "%~dp0"

echo [1/3] Cleaning previous build artifacts...
echo.
dotnet clean BFD9010.sln --configuration Release --verbosity quiet
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Clean failed!
    goto :error
)

echo [2/3] Building solution...
echo.
dotnet build BFD9010.sln --configuration Release --no-incremental
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed!
    goto :error
)

echo.
echo [3/3] Publishing GUI executable...
echo.
dotnet publish BFD9010.Gui\BFD9010.Gui.csproj --configuration Release --no-build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: GUI publish failed!
    goto :error
)

echo.
echo ================================================
echo Build Successful!
echo ================================================
echo.
echo Output locations:
echo   CLI:       BFD9010.Cli\bin\Release\net8.0\bfd9010.exe
echo   GUI:       BFD9010.Gui\bin\Release\net8.0-windows\publish\bfd9010_fhir32.exe
echo   Libraries: BFD9010.Scanner\bin\Release\net8.0\*.dll
echo              BFD9010.FhirApi\bin\Release\net8.0\*.dll
echo.
echo Quick start:
echo   - start-gui.bat  (GUI with integrated API - recommended)
echo   - start-cli.bat  (CLI interface)
echo.
echo To create distribution package:
echo   - package.bat
echo.
echo To use custom config:
echo   - bfd9010.exe --config path\to\config.ini
echo   - bfd9010_fhir32.exe --config path\to\config.ini
echo.
pause
exit /b 0

:error
echo.
echo ================================================
echo Build Failed!
echo ================================================
echo.
echo Please check the error messages above.
echo.
pause
exit /b 1
