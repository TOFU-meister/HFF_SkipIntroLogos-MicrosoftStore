# HFF Skip Intro Logos - Microsoft Store

Microsoft Store / Xbox PC port of the Human: Fall Flat **Skip Intro Logos** BepInEx plugin.

This repository is a fork of the original Steam plugin by Permamiss. The Microsoft Store port keeps the same basic behavior: it intercepts `StartupExperienceController.PlayStartupExperience()`, skips the startup experience, and transitions to the main menu.

## Current status

Early test build / development version.

Target environment:

- Human: Fall Flat Microsoft Store / Xbox PC (Windows x64)
- Unity IL2CPP
- BepInEx 6 IL2CPP (`6.0.0-be.793` used for development)
- Unity 2019.4.24f1 / game v1696732 used for initial porting

## Build

The project references DLLs generated/installed by the target game's BepInEx environment. Do not commit those DLLs to this repository.

PowerShell example:

```powershell
dotnet build .\SkipIntroLogos\HFF_SkipIntroLogos.csproj -c Release -p:BepInExDir="C:\path\to\BepInEx"
```

The required references are read from `BepInEx\core` and `BepInEx\interop`.

## Install for testing

Copy the built `HFF_SkipIntroLogos_MicrosoftStore.dll` directly into:

```text
BepInEx\plugins
```

For the first test, confirm that:

1. BepInEx reports the plugin as loaded.
2. The startup logo sequence is skipped.
3. The game reaches the main menu normally.
4. The menu remains usable.

## Credits

Original Steam plugin: **Permamiss / HFF_SkipIntroLogos**

This fork contains the Microsoft Store / Xbox PC BepInEx 6 IL2CPP port.
