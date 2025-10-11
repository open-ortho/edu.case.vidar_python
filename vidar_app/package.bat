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
set DLL=Vscsi32.dll
set "DLLSRC=C:\Program Files (x86)\VIDAR\Driver\Vscsi32.dll"

REM Ensure artifacts directory exists
if not exist "%ARTIFACTS%" mkdir "%ARTIFACTS%"

REM Extract version from CLI project
set "PROJ=BFD9010.Cli\BFD9010.Cli.csproj"
set VER=
for /f "tokens=3 delims=<>" %%v in ('findstr /i "<Version>" %PROJ%') do (
    set "VER=%%v"
)
REM Remove spaces
set VER_TRIMMED=!VER: =!
if not defined VER_TRIMMED (
    echo ERROR: Version string not found in %PROJ%
    exit /b 1
)

echo Version: !VER_TRIMMED!
echo.

REM ==============================================
REM Build all projects
REM ==============================================
echo [1/5] Cleaning previous build artifacts...
dotnet clean vidar_app.sln --configuration Release --verbosity quiet
if %ERRORLEVEL% NEQ 0 goto :error

echo [2/5] Building solution...
dotnet build vidar_app.sln --configuration Release --no-incremental
if %ERRORLEVEL% NEQ 0 goto :error

echo [3/5] Publishing CLI (self-contained)...
dotnet publish BFD9010.Cli\BFD9010.Cli.csproj -c Release -r win-x86 --self-contained true
if %ERRORLEVEL% NEQ 0 goto :error

echo [4/5] Publishing GUI (self-contained)...
dotnet publish BFD9010.Gui\BFD9010.Gui.csproj -c Release -r win-x86 --self-contained true
if %ERRORLEVEL% NEQ 0 goto :error

REM ==============================================
REM Package CLI
REM ==============================================
echo [5/5] Creating distribution packages...
echo.
set "CLI_PUBDIR=BFD9010.Cli\bin\Release\net8.0\win-x86\publish"
set "GUI_PUBDIR=BFD9010.Gui\bin\Release\net8.0-windows\win-x86\publish"

REM Copy Vscsi32.dll to CLI publish directory
if exist "%DLL%" (
    copy "%DLL%" "%CLI_PUBDIR%"
) else if exist "%DLLSRC%" (
    copy "%DLLSRC%" "%CLI_PUBDIR%"
) else (
    echo WARNING: Vscsi32.dll not found. Package may not work without it.
)

REM Copy Vscsi32.dll to GUI publish directory
if exist "%DLL%" (
    copy "%DLL%" "%GUI_PUBDIR%"
) else if exist "%DLLSRC%" (
    copy "%DLLSRC%" "%GUI_PUBDIR%"
) else (
    echo WARNING: Vscsi32.dll not found. Package may not work without it.
)

REM Copy sample config to both packages
copy scan_config.ini "%CLI_PUBDIR%\"
copy scan_config.ini "%GUI_PUBDIR%\"

REM Create ZIP packages
set "CLI_ZIP=%ARTIFACTS%\bfd9010_cli_v!VER_TRIMMED!.zip"
set "GUI_ZIP=%ARTIFACTS%\bfd9010_gui_v!VER_TRIMMED!.zip"

if exist "%CLI_ZIP%" del /f "%CLI_ZIP%"
if exist "%GUI_ZIP%" del /f "%GUI_ZIP%"

echo Creating CLI package...
powershell Compress-Archive -Path "%CLI_PUBDIR%\*" -DestinationPath "%CLI_ZIP%"
if %ERRORLEVEL% NEQ 0 goto :error

echo Creating GUI package...
powershell Compress-Archive -Path "%GUI_PUBDIR%\*" -DestinationPath "%GUI_ZIP%"
if %ERRORLEVEL% NEQ 0 goto :error

echo.
echo ================================================
echo Build and Package Successful!
echo ================================================
echo.
echo Distribution packages created:
echo   CLI: %ARTIFACTS%\bfd9010_cli_v!VER_TRIMMED!.zip
echo   GUI: %ARTIFACTS%\bfd9010_gui_v!VER_TRIMMED!.zip
echo.
echo Each package includes:
echo   - Executable (self-contained, no .NET required)
echo   - All dependencies
echo   - Vscsi32.dll (scanner driver)
echo   - scan_config.ini (sample configuration)
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
