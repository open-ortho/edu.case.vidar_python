{
  description = "BFD9010: An HL7 FHIR API for Scanners";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    utils.url = "github:numtide/flake-utils";
  };

  outputs =
    { nixpkgs, utils, ... }:
    utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs { inherit system; };
        dn = (
          with pkgs.dotnetCorePackages;
          combinePackages [
            sdk_8_0
            sdk_10_0
          ]
        );
      in
      {
        packages = rec {
          nugetDeps = pkgs.dotnetCorePackages.addNuGetDeps {
            nugetDeps = ./nix/deps.json;
          };
          default = app;
          base = {
            pname = "BFD9010.Gui";
            version = "1.0.0";
            src = ./.;

            nativeBuildInputs = [
              pkgs.dotnetCorePackages.sdk_8_0
              pkgs.cacert
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
                  ${pkgs.wine}/bin/wineboot -u
                fi
              exec ${pkgs.wine}/bin/wine $out/share/bfd9010/bfd9010.exe "\$@"
              EOF

              chmod +x $out/bin/bfd9010-server
            '';
          };
          app = pkgs.stdenv.mkDerivation (nugetDeps base);
        };

        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [
            dn
            nuget-to-json
            nil
            nixd
            csharp-ls
          ];

          shellHook = ''
            export DOTNET_ROOT="${dn}/share/dotnet"
          '';
        };
      }
    );
}
