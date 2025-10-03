@echo on
REM Build and publish the app
setlocal enabledelayedexpansion
set PROJ=vidar_app\vidar_app.csproj
set PUBDIR=vidar_app\bin\Release\net8.0\win-x86\publish
set DLL=Vscsi32.dll
set "DLLSRC=C:\Program Files (x86)\VIDAR\Driver\Vscsi32.dll"
set "ARTIFACTS=artifacts"

REM Ensure artifacts directory exists (relative to this script's working dir)
if not exist "%ARTIFACTS%" mkdir "%ARTIFACTS%"

REM Extract version from .csproj
set VER=
for /f "tokens=3 delims=<>" %%v in ('findstr /i "<Version>" %PROJ%') do (
    set "VER=%%v"
)
REM Fallback: if Version tag not found, try VersionPrefix
if not defined VER (
    for /f "tokens=3 delims=<>" %%v in ('findstr /i "<VersionPrefix>" %PROJ%') do (
        set "VER=%%v"
    )
)
REM Remove spaces
set VER_TRIMMED=!VER: =!
REM Check for valid version string
if not defined VER_TRIMMED (
    echo ERROR: Version string not found or empty in %PROJ%.
    exit /b 1
)

set "ZIPNAME=vidar_app_release_v!VER_TRIMMED!.zip"
set "ZIPPATH=%ARTIFACTS%\!ZIPNAME!"
REM Check for valid zip file name - use goto to avoid multiline-if parsing issues
if "!ZIPNAME!"=="vidar_app_release_v.zip" goto :ZIP_ERROR

REM Build and publish
call dotnet publish %PROJ% -c Release -r win-x86 --self-contained true
if errorlevel 1 exit /b 1

REM Check if publish directory exists
if not exist "%PUBDIR%" (
    echo ERROR: Publish directory not found: %PUBDIR%
    exit /b 1
)

REM Copy DLL from current directory or fallback to VIDAR driver directory
if exist "%DLL%" (
    copy "%DLL%" "%PUBDIR%"
    if errorlevel 1 exit /b 1
) else (
    if exist "%DLLSRC%" (
        copy "%DLLSRC%" "%PUBDIR%"
        if errorlevel 1 exit /b 1
    ) else (
        echo ERROR: Vscsi32.dll not found in current directory or VIDAR driver directory.
        exit /b 1
    )
)

REM Remove existing ZIP file if it exists
if exist "%ZIPPATH%" del /f "%ZIPPATH%"

REM Zip the publish directory (requires PowerShell 5+)
powershell Compress-Archive -Path "%PUBDIR%\\*" -DestinationPath "%ZIPPATH%"
if errorlevel 1 exit /b 1

echo Build, copy, and zip complete! Output: %CD%\%ARTIFACTS%\!ZIPNAME!

endlocal

goto :EOF

:ZIP_ERROR
echo ERROR: ZIP file name is invalid (empty version).
exit /b 1
