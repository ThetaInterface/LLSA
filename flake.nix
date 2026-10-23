{
    description = "LLSA";

    inputs = {
        nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
    };

    outputs = { self, nixpkgs }:
    let 
        supportedSystems = [ "x86_64-linux" "aarch64-linux" ];
        forAllSystems = nixpkgs.lib.genAttrs supportedSystems;
    in {
        packages = forAllSystems(system:
            let 
                pkgs = import nixpkgs { inherit system; };
            in {
                default = pkgs.callPackage ./nix/default.nix {};
            }
        );
    };
}