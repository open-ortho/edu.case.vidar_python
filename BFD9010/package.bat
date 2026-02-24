@echo off
REM BFD9010 Package Script
REM Builds projects and creates distribution packages
setlocal enabledelayedexpansion

echo ================================================
echo BFD9010 Scanner Software - Build and Package
echo ================================================
echo.

REM Change to the directory containing this script
cd /d "%~dp0"

set "ARTIFACTS=artifacts"
set "DLLPATH=BFD9010.Scanner\Vscsi32.dll"

REM Ensure artifacts directory exists
if not exist "%ARTIFACTS%" mkdir "%ARTIFACTS%"

REM Extract version from Directory.Build.props using PowerShell XML parsing
set "PROPFILE=Directory.Build.props"
for /f "delims=" %%v in ('powershell -NoProfile -Command "[xml]$xml = Get-Content '%PROPFILE%'; $xml.Project.PropertyGroup.Version"') do set "VER_TRIMMED=%%v"

if "!VER_TRIMMED!"=="" (
    echo ERROR: Version string not found in %PROPFILE%
    exit /b 1
)

echo Version: !VER_TRIMMED!
echo.

REM Ensure scanner DLL is present so it can be bundled
if not exist "%DLLPATH%" (
    echo ERROR: %DLLPATH% not found. Please add Vscsi32.dll to the project.
    exit /b 1
)

REM ==============================================
REM Build all projects
REM ==============================================
echo [1/5] Cleaning previous build artifacts...
dotnet clean BFD9010.sln --configuration Release --verbosity quiet
if %ERRORLEVEL% NEQ 0 goto :error

echo [2/5] Building solution...
dotnet build BFD9010.sln --configuration Release --no-incremental
if %ERRORLEVEL% NEQ 0 goto :error

echo [3/5] Publishing CLI (single-file, self-contained)...
dotnet publish BFD9010.Cli\BFD9010.Cli.csproj -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if %ERRORLEVEL% NEQ 0 goto :error

echo [4/5] Publishing GUI (single-file, self-contained)...
dotnet publish BFD9010.Gui\BFD9010.Gui.csproj -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if %ERRORLEVEL% NEQ 0 goto :error

REM ==============================================
REM Package CLI and GUI
REM ==============================================
echo [5/5] Creating distribution packages...
echo.
set "CLI_PUBDIR=BFD9010.Cli\bin\Release\net8.0\win-x86\publish"
set "GUI_PUBDIR=BFD9010.Gui\bin\Release\net8.0-windows\win-x86\publish"

REM Vscsi32.dll is bundled into the single-file publish and will extract at runtime

REM Remove any scan_config.ini files from publish directories
REM This ensures packages use default settings (16-bit, 300 DPI)
if exist "%CLI_PUBDIR%\scan_config.ini" del /f "%CLI_PUBDIR%\scan_config.ini"
if exist "%GUI_PUBDIR%\scan_config.ini" del /f "%GUI_PUBDIR%\scan_config.ini"

REM Remove PDB files (debug symbols) from publish directories
if exist "%CLI_PUBDIR%\*.pdb" del /f "%CLI_PUBDIR%\*.pdb"
if exist "%GUI_PUBDIR%\*.pdb" del /f "%GUI_PUBDIR%\*.pdb"

REM Create combined ZIP package
set "BUNDLE_DIR=%ARTIFACTS%\bundle"
set "BUNDLE_ZIP=%ARTIFACTS%\bfd9010.zip"

if exist "%BUNDLE_DIR%" rmdir /s /q "%BUNDLE_DIR%"
mkdir "%BUNDLE_DIR%"

copy /y "%CLI_PUBDIR%\bfd9010_cli.exe" "%BUNDLE_DIR%\bfd9010_cli.exe"
if %ERRORLEVEL% NEQ 0 goto :error

copy /y "%GUI_PUBDIR%\bfd9010.exe" "%BUNDLE_DIR%\bfd9010.exe"
if %ERRORLEVEL% NEQ 0 goto :error

if exist "%BUNDLE_ZIP%" del /f "%BUNDLE_ZIP%"

echo Creating combined package...
powershell -Command "Compress-Archive -Path '%BUNDLE_DIR%\*' -DestinationPath '%BUNDLE_ZIP%' -Force"
if %ERRORLEVEL% NEQ 0 goto :error

echo.
echo ================================================
echo Build and Package Successful!
echo ================================================
echo.
echo Distribution package created:
echo   %ARTIFACTS%\bfd9010.zip
echo.
echo Package includes:
echo   - bfd9010.exe (GUI)
echo   - bfd9010_cli.exe (CLI)
echo   - Executables are single-file and self-contained
echo   - Native libraries extract to user temp at runtime
echo   - Vscsi32.dll is bundled in the EXE
echo.
echo NOTE: scan_config.ini is NOT included in packages.
echo       On first run, a default config will be created with:
echo       - BitDepth: 16
echo       - DPI: 300
echo       Users can then customize settings as needed.
echo.
pause
endlocal
exit /b 0

:error
echo.
echo ================================================
echo Build/Package Failed!
echo ================================================
echo.
echo Please check the error messages above.
echo.
pause
endlocal
exit /b 1
