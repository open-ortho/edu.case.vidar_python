{
  dotnetCorePackages,
  cacert,
  wine,
  stdenv,
  ...
}:
let
  nugetDeps = dotnetCorePackages.addNuGetDeps {
    nugetDeps = ../nix/deps.json;
  };
  base = {
    pname = "BFD9010.Gui";
    version = "1.0.0";
    src = ./..;

    nativeBuildInputs = [
      dotnetCorePackages.sdk_8_0
      cacert
    ];

    # Explicitly set runtimeId to null to block the host-side targeting pack additions
    runtimeId = null;

    configurePhase = ''
      export HOME=$TMPDIR
      export DOTNET_CLI_HOME=$TMPDIR
      export NUGET_PACKAGES=$nugetDeps
    '';

    buildPhase = ''
      # Pure offline compilation phase using only the decoupled Windows targets

      dotnet restore BFD9010/BFD9010.Gui/BFD9010.Gui.csproj \
        -r win-x86 \
        --source "$nugetDeps" \
        -p:EnableWindowsTargeting=true

      dotnet publish BFD9010/BFD9010.Gui/BFD9010.Gui.csproj \
        -c Release \
        -r win-x86 \
        --no-restore \
        -p:TargetFramework=net8.0-windows \
        --self-contained true \
        -o $out/share/bfd9010
    '';

    installPhase = ''
      mkdir -p $out/bin

      cat <<EOF > $out/bin/bfd9010-server
      #!/usr/bin/env bash
      export WINEDEBUG=-all
      export WINEARCH=win32
        export WINEPREFIX=\$HOME/.local/share/bfd9010-wine
        if [ ! -d "\$WINEPREFIX" ]; then
          echo "Initializing 32-bit Wine environment..."
          ${wine}/bin/wineboot -u
        fi
      exec ${wine}/bin/wine $out/share/bfd9010/bfd9010.exe "\$@"
      EOF

      chmod +x $out/bin/bfd9010-server
    '';
  };
in
stdenv.mkDerivation (nugetDeps base)
