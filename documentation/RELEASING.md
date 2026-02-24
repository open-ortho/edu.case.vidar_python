# Release Checklist

Before creating a production release, follow this checklist to ensure quality and readiness:

- [ ] **Update version number**: Edit `BFD9010/Directory.Build.props` to increment the version (e.g., from `1.0.0` to `1.0.1`)
- [ ] **Run tests**: Execute `dotnet test BFD9010/BFD9010.FhirApi.Tests/BFD9010.FhirApi.Tests.csproj` to verify all tests pass
- [ ] **Build in Release mode**: Run `build.bat` to ensure clean Release builds
- [ ] **Test production builds**: Run `start-cli.bat` and `start-gui.bat` to verify functionality
- [ ] **Package for distribution**: Run `package.bat` to create `bfd9010.zip`
- [ ] **Test packaged executables**: Extract the ZIP and run both `bfd9010.exe` and `bfd9010_cli.exe` to ensure they work correctly
- [ ] **Verify environment settings**: Ensure `ASPNETCORE_ENVIRONMENT=Production` in packaged builds (CORS restricted, Swagger disabled)
- [ ] **Update documentation**: Check that `README.md` and other docs reflect the new version
- [ ] **Commit changes**: Commit version updates and any final fixes with a clear message (e.g., "Release v1.0.1")
