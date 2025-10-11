# Rename Summary: vidar_app → BFD9010.Cli

## Date: October 10, 2025

## Changes Made

### Physical Renames
1. **Folder**: `vidar_app/vidar_app/` → `vidar_app/BFD9010.Cli/`
2. **Project File**: `vidar_app.csproj` → `BFD9010.Cli.csproj`

### Files Updated

#### Solution File
- **vidar_app.sln**: Updated project reference from `vidar_app\vidar_app.csproj` to `BFD9010.Cli\BFD9010.Cli.csproj`

#### Build Scripts
- **build.bat**: Updated output path from `vidar_app\bin\...` to `BFD9010.Cli\bin\...`
- **package.bat**: 
  - Updated PROJ variable to `BFD9010.Cli\BFD9010.Cli.csproj`
  - Updated CLI_PUBDIR to `BFD9010.Cli\bin\Release\...`
  - Updated dotnet publish command
- **start-cli.bat**: Updated executable path to `BFD9010.Cli\bin\Release\...`
- **build-package.bat.old**: 
  - Updated PROJ and PUBDIR variables
  - Updated ZIP filename from `vidar_app_release_v...` to `bfd9010_cli_release_v...`

#### Documentation
- **API_README.md**: Updated all references to vidar_app → BFD9010.Cli
- **README.md** (in vidar_app folder): Updated output paths
- **README.md** (root): Updated code references and paths

## What Was NOT Changed

- **Assembly Name**: Still `bfd9010` (as configured in .csproj)
- **Executable Name**: Still `bfd9010.exe`
- **Solution Name**: Still `vidar_app.sln`
- **Namespaces**: No namespace changes needed (Program.cs uses global namespace)
- **Project References**: Other projects reference by path, which are updated

## Testing Checklist

Before committing, verify:
- [ ] Solution opens in Visual Studio without errors
- [ ] All projects build successfully: `dotnet build vidar_app.sln --configuration Release`
- [ ] CLI executable is created at: `BFD9010.Cli\bin\Release\net8.0\bfd9010.exe`
- [ ] start-cli.bat launches the application
- [ ] package.bat creates distribution packages successfully
- [ ] All inter-project references still work

## Benefits of This Change

1. **Consistency**: Now follows the naming convention of other projects (BFD9010.Scanner, BFD9010.FhirApi, BFD9010.Gui)
2. **Clarity**: The name "BFD9010.Cli" clearly indicates this is the command-line interface for the BFD9010 project
3. **Maintainability**: Easier to understand the project structure at a glance
4. **Professionalism**: Removes the generic "vidar_app" name and aligns with project branding

## Notes

- The solution file name (`vidar_app.sln`) was kept as-is to avoid breaking existing build scripts and documentation links
- The assembly/executable name remains `bfd9010` for end-user simplicity
- This is a non-breaking change for compiled binaries - the executable name and behavior are unchanged
