All platforms:

Marian executable and the embedded Python are not included in the repository. You can build Marian yourself, or copy it from a previous release of OPUS-CAT. Likewise, copy the embedded Python folder from a previous release (or build your own).

## Windows:

Build from Visual Studio in the usual manner.

## MacOS:

It might be possible to build the MacOS program in Windows, but it doesn't seem work out of the box (although this might be because of some opaque permission issue in MacOS). Because of this, I've built the program only on MacOS machines. This requires installing the dotnet SDK. When working with remote Macs (e.g. from Scaleway, don't use MacInCloud, they don't provide admin permissions that you need), I've found the best approach to be to use SSH for all actual file handling and building, and using the remote desktop (very slow) only for testing the user interface (you can launch the UI from the terminal).

Build from the command line using the dotnet command:

`dotnet publish -c ReleaseMacos --self-contained -r osx-arm64`

The build will be in *bin/ReleaseMacos/osx-arm64/publish*.

You will need to grant execution permissions to the launch script:

`chmod +x bin/ReleaseMacos/osx-arm64/publish/OpusCatMtEngine.command`

Then you need to be copy embedded Python directory and the Marian executable from a previous release (make sure they have been dequarantined with *sudo xattr -c -r .*).

For Intel Macs, the procedure is the same, just replace *osx-arm64* with *osx-x64*.

## Linux

You can build the program in Windows

`dotnet publish -c ReleaseMacos --self-contained -r linux-x64`

But it needs some modifications to run:

1. First off, make sure there are no carriage returns in the OpusCatMtEngine.sh file.
2. Give execution permissions to *OpusCatMtEngine.sh* and *OpusCatMtEngineCore* with *chmod +x*.
3. Copy the embedded Python directory and the Marian executable from a previous release, and make sure that *Marian/marian* is executable.
