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
        packages = {
          app = pkgs.callPackage ./nix/build.nix { };
          default = pkgs.callPackage ./nix/build.nix { };
        };

        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [
            dn
            nuget-to-json
            nil
            nixd
            csharp-ls
            wine
          ];

          shellHook = ''
            export DOTNET_ROOT="${dn}/share/dotnet"
          '';
        };

        # Cannot do windows checks on linux as we do not have the necessary 32-bit runtimes.
        # Possible solution: run in a Windows VM to test
      }
    );
}
