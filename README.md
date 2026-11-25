# HFF Skip Intro Logos - Microsoft Store

Microsoft Store / Xbox PC port of Permamiss's Human: Fall Flat **Skip Intro Logos** BepInEx plugin. It skips the Curve, No Brakes Games, and startup Human: Fall Flat logos. **Press Anything** still waits for input, and the Human: Fall Flat title logo appears in the main menu.

## Supported environment

- Microsoft Store / Xbox PC (Windows x64)
- Unity 2019.4.24f1 (IL2CPP)
- BepInEx 6 IL2CPP x64 (6.0.0-be.793)
- Analyzed internal build ID in `BuildVersion.txt` is `v169673214`. The player-facing version shown in the game is a separate identifier.

Before applying patches, the plugin checks the SHA-256 hashes of `GameAssembly.dll`, `global-metadata.dat`, and BepInEx's generated `interop\Assembly-CSharp.dll`. If any hash differs from the analyzed binaries, the plugin leaves the game's normal startup in place. A game or interop update can therefore disable intro skipping.

## Install

With the game closed, place `HFF_SkipIntroLogos_MicrosoftStore.dll` in the game's `BepInEx\plugins` folder, replacing any older copy of this plugin. On startup, check `BepInEx\LogOutput.log` for `Intro logo skip enabled.`

## Build

Use a .NET SDK with the .NET 6 targeting pack. Start the game once with BepInEx to generate the interop assemblies, then run this command from the repository root in PowerShell.

```powershell
dotnet build .\SkipIntroLogos\HFF_SkipIntroLogos.csproj -c Release -p:BepInExDir="C:\path\to\BepInEx"
```

The build reads references from `BepInEx\core` and `BepInEx\interop` and writes the plugin DLL to `SkipIntroLogos\bin\Release`.

## Credits and licensing

This port preserves the Git history of the original Steam plugin. [Original HFF_SkipIntroLogos by Permamiss](https://github.com/Permamiss/HFF_SkipIntroLogos)

No license file or redistribution permission was found in the upstream repository.
