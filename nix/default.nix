{ pkgs, lib, buildDotnetModule, dotnetCorePackages, fetchFromGitHub }:

buildDotnetModule rec {
    pname = "LLSA";
    version = "1.0.6";

    src = fetchFromGitHub {
        owner = "ThetaInterface";
        repo = "LLSA";
        rev = "3c6919319b7d5a20b3b599c63bb580c24d1c8eb8";
        sha256 = "sha256-qGxqFChVWl4F8E8HgQeunwvB7eEM4IbZDrbOrj01wVE=";
    };

    projectFile = "LLSA.csproj";

    postInstall = ''
        mkdir -p $out/share/LLSA
        cp -R Data $out/share/LLSA/
    '';

    dotnet-sdk = dotnetCorePackages.sdk_9_0;
    dotnet-runtime = dotnetCorePackages.runtime_9_0;

    nugetDeps = ./deps.json; 

    runtimeDeps = with pkgs; [
        fontconfig
        libGL
        libx11
        libice
        libsm
        libxcursor
        libxext
        libxrandr
        libxi
    ];

    meta = with lib; {
        description = "LLSA";
        homepage = "https://github.com/ThetaInterface/LLSA";
        license = licenses.mit;
        platforms = platforms.linux;
        mainProgram = "LLSA";
    };
}