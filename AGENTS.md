# AGENTS

This file is for agentic coding tools working in this repo. It captures how to build, test, and follow local code conventions.

## Repo Map

- `BFD9010/` .NET 8 solution and projects (CLI, GUI, FHIR API, Scanner lib, tests).
- `documentation/` and `examples/` reference material.
- `.vscode/` contains a legacy task (`python test.py`) unrelated to the .NET build.

## Build, Run, Package

All solution files live under `BFD9010/`.

Build (dotnet, cross-platform):

```bash
dotnet build BFD9010/BFD9010.sln --configuration Release
```

Build GUI publish (needed for `bfd9010_fhir32.exe`):

```bash
dotnet publish BFD9010/BFD9010.Gui/BFD9010.Gui.csproj --configuration Release
```

Windows batch build (Release + GUI publish):

```bat
BFD9010\build.bat
```

Run in dev mode (recommended for development):

```bash
dotnet run --project BFD9010/BFD9010.Cli/BFD9010.Cli.csproj
dotnet run --project BFD9010/BFD9010.Gui/BFD9010.Gui.csproj
```

Run built binaries (Windows, after Release build):

```bat
BFD9010\start-cli.bat
BFD9010\start-gui.bat
```

Create self-contained ZIP packages (Windows only):

```bat
BFD9010\package.bat
```

Notes:

- `package.bat` expects `Vscsi32.dll` (either in repo root or in `C:\Program Files (x86)\VIDAR\Driver`).
- Output ZIPs go to `BFD9010\artifacts\`.

## Tests

Test project: `BFD9010/BFD9010.FhirApi.Tests/BFD9010.FhirApi.Tests.csproj` (xUnit).

Run all tests:

```bash
dotnet test BFD9010/BFD9010.FhirApi.Tests/BFD9010.FhirApi.Tests.csproj
```

Run a single test by fully-qualified name:

```bash
dotnet test BFD9010/BFD9010.FhirApi.Tests/BFD9010.FhirApi.Tests.csproj --filter "FullyQualifiedName=BFD9010.FhirApi.Tests.FhirApiTests.FhirBundle_ShouldHaveCorrectResourceType"
```

Run a subset by class name:

```bash
dotnet test BFD9010/BFD9010.FhirApi.Tests/BFD9010.FhirApi.Tests.csproj --filter "FullyQualifiedName~BFD9010.FhirApi.Tests.FhirApiTests"
```

## Lint / Format

- No repo-level lint or format tool is configured (no `.editorconfig`).
- Avoid running `dotnet format` unless explicitly requested.

## Code Style Guidelines

Language: C# (.NET 8, nullable enabled, implicit usings enabled in projects).

General:

- Prefer small, focused methods; keep scanner operations and API endpoints readable and imperative.
- Avoid unnecessary abstraction; favor clarity over cleverness.

Namespaces and files:

- File-scoped namespaces are common in `BFD9010.FhirApi` (e.g., `namespace BFD9010.FhirApi;`).
- Block-scoped namespaces exist in `BFD9010.Scanner` (e.g., `namespace BFD9010.Scanner { ... }`).
- Match the style of the file you are editing rather than switching styles mid-file.

Using directives:

- Keep `using` lists minimal and specific.
- `using static` is used for scanner native interop in CLI and service code; follow existing patterns.
- Preserve the local ordering in a file; do not reorder purely for preference.

Formatting:

- Indentation is 4 spaces.
- Braces on new lines.
- Keep lines reasonably short, but prioritize readability over hard limits.

Naming:

- Public types/methods/properties use PascalCase.
- Locals and parameters use camelCase.
- Private fields use `_camelCase` (example: `_logger`).
- Constants use `ALL_CAPS` (example: `CONFIG_FILENAME`).

Types and nullability:

- Use nullable annotations (`string?`, `ScannerData?`) consistently.
- Guard nulls early and return clear error results.
- Prefer `var` when the type is obvious from the RHS; otherwise use explicit types.

Async and concurrency:

- Use `async Task` / `async Task<T>`; avoid `async void` except event handlers.
- Avoid `.Result` or `.Wait()` on tasks; use `await` (see `FhirServerHost`).
- Use cancellation tokens when long-running operations are involved.

Error handling and logging:

- FHIR API uses structured logging (`_logger.LogInformation`, `LogError`, etc.).
- Prefer descriptive log messages with structured properties.
- For scanner operations, status code `0` is success; non-zero is error.
- Wrap native calls in `try/catch` and surface useful error messages (see `ScannerService.GetVidarErrorMessage`).

FHIR API conventions:

- Use `OperationOutcome` for error/success details.
- Use `Results.Content(..., "application/fhir+json")` for FHIR responses.
- Keep endpoint behavior consistent with existing routes in `FhirServerConfiguration`.

Tests:

- xUnit with `[Fact]`.
- Tests typically use Arrange/Act/Assert comments.
- Prefer clear, descriptive test names with underscores.

Configuration:

- Always load scanner config via `ScanConfig.Load(...)`.
- `scan_config.ini` is user-specific and ignored by git; do not commit it.
- When writing to config, keep the existing INI structure and comments.

Interop and unsafe code:

- `AllowUnsafeBlocks` is enabled; keep unsafe blocks tightly scoped.
- Avoid introducing new unsafe code unless necessary for scanner interop.

## Environment Notes

- GUI and scanner access are Windows-only; non-Windows environments may only build or run tests.
- `Vscsi32.dll` is required for real hardware interaction.
- `PlatformTarget` is `x86`; preserve this unless there is a strong reason to change it.

## Cursor / Copilot Rules

- No `.cursor/rules/`, `.cursorrules`, or `.github/copilot-instructions.md` found in this repo.
