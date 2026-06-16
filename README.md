# NMA Community Linux Fix

> **Unofficial community fork of NexusMods.App v0.21.1.**
>
> This project is **not an official Nexus Mods release**, is not endorsed by Nexus Mods, and is provided without warranty.
> It exists to preserve and improve the discontinued Nexus Mods App for a specific Linux / Heroic / GOG use case.

## What this fork is

`NMA Community Linux Fix` is a community-maintained fork based on the archived `Nexus-Mods/NexusMods.App` project.

Initial focus:

- Linux desktop usage.
- Cyberpunk 2077 installed through **GOG via Heroic Games Launcher**.
- Better handling of GOG DLC detection, especially Phantom Liberty / DLC-style products.
- Avoiding false-positive missing-file diagnostics for optional GOG Galaxy metadata files.

Tested by the maintainer on:

- Ubuntu 26.04 Cinnamon
- Cyberpunk 2077 GOG via Heroic
- Phantom Liberty installed
- French language files installed
- Proton/Wine workflow launched through Heroic

## Important disclaimer

This is an experimental community fork.

Use it at your own risk. Always back up your game installation before applying mods.

Recommended safety workflow:

1. Keep a clean backup of your game folder.
2. Use **Preview changes** before clicking **Apply**.
3. Do not apply large mod lists or collections as a first test.
4. Start with a small known-good set of framework mods.
5. Launch the game from **Heroic**, not from the app.

## Why this fork exists

Nexus Mods discontinued active development of the Nexus Mods App and moved its focus back to Vortex. Since the original app is open source, the community can fork it, provided forks use a different name and are clearly distinguishable from official Nexus Mods tools.

This fork therefore uses a different name and is clearly marked as unofficial.

## Current changes

Compared with upstream `NexusMods.App v0.21.1`, this fork currently includes the following local fixes.

### 1. Heroic/GOG DLC detection

File:

```text
src/NexusMods.StandardGameLocators/HeroicGogLocator.cs
```

Problem:

The upstream Heroic GOG locator had an explicit placeholder:

```csharp
// TODO: FIX THIS
DLCBuildIds = [],
```

This meant Heroic/GOG installations could detect the base game but fail to expose installed DLC IDs to the loadout/version system.

Fix:

The locator now uses:

```csharp
heroicGOGGame.InstalledDLCs
```

and forwards those IDs into `DLCBuildIds`.

Expected effect:

Cyberpunk 2077 GOG installations through Heroic can correctly expose installed DLC-style products, including Phantom Liberty, to NexusMods.App's file/version logic.

### 2. Optional GOG Galaxy metadata files no longer block loadouts

Files:

```text
src/NexusMods.Games.FileHashes/FileHashesService.cs
src/NexusMods.Games.FileHashes/Emitters/UndeployableLoadoutDueToMissingGameFiles.cs
```

Problem:

For some GOG installations, especially Heroic/GOG installations, the following files may be absent even when the game itself is valid:

```text
goggame-*.info
goggame-*.hashdb
```

Upstream NexusMods.App could treat them as required game files and block applying a loadout with:

```text
Loadout can't be applied due to 2 missing game file(s) with no valid source
```

Fix:

This fork treats those GOG/Galaxy metadata files as optional for loadout-blocking purposes.

The fix is applied at two levels:

1. `FileHashesService.GetGameFiles()` filters them out for new game file calculations.
2. `UndeployableLoadoutDueToMissingGameFiles` filters them out when an existing sync tree/loadout state still contains them.

Expected effect:

Missing `goggame-*.info` and `goggame-*.hashdb` files no longer prevent a valid Heroic/GOG Cyberpunk 2077 loadout from passing health checks.

### 3. Larger backup safety limit

File:

```text
src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs
```

Change:

The maximum backup size was increased to:

```text
128 GB
```

Reason:

Large games and DLC-heavy installations can legitimately require more than the old backup limit. This is especially relevant for Cyberpunk 2077 with Phantom Liberty and large archive files.

Warning:

This can consume substantial disk space. Make sure your storage can handle large backups before applying mods.

### 4. Linux desktop integration notes

This fork is intended to be run as a manually published Linux build.

Recommended local desktop integration:

```bash
mkdir -p "$HOME/.local/share/icons/hicolor/scalable/apps"

cp -f src/NexusMods.App/icon.svg \
  "$HOME/.local/share/icons/hicolor/scalable/apps/com.nma-community-linux-fix.svg"
```

Example desktop file:

```ini
[Desktop Entry]
Type=Application
Name=NMA Community Linux Fix
GenericName=Mod Manager
Comment=Unofficial community Linux fix fork of NexusMods.App
Categories=Game;
Terminal=false
StartupWMClass=NexusMods.App
MimeType=x-scheme-handler/nxm;
Icon=com.nma-community-linux-fix
Exec=/data/Apps/NMA-Community-Linux-Fix/NexusMods.App %u
TryExec=/data/Apps/NMA-Community-Linux-Fix/NexusMods.App
```

Register it:

```bash
xdg-mime default com.nma-community-linux-fix.desktop x-scheme-handler/nxm
gtk-update-icon-cache -f -t "$HOME/.local/share/icons/hicolor" 2>/dev/null || true
update-desktop-database "$HOME/.local/share/applications"
```

## Wine / Proton requirement for Cyberpunk 2077

Cyberpunk 2077 framework mods commonly require Wine DLL overrides.

In Heroic, configure the Cyberpunk 2077 game environment variable:

```text
WINEDLLOVERRIDES=winmm=n,b;version=n,b
```

Recommended launch workflow:

```text
Use this app to install/apply mods.
Use Heroic to launch the game.
```

## Known-good basic Cyberpunk framework mod set

The initial successful test target for this fork was the common Cyberpunk 2077 base framework stack:

- Cyber Engine Tweaks (CET)
- RED4ext
- redscript
- ArchiveXL
- TweakXL
- Codeware

Recommended first test:

1. Install only the framework stack.
2. Click **Preview changes**.
3. Check that no massive game/DLC deletion or restoration is planned.
4. Click **Apply**.
5. Launch Cyberpunk 2077 from Heroic.
6. Confirm the game, Phantom Liberty, and language files still work.

## Build instructions

### Requirements

On Ubuntu-like systems:

```bash
sudo apt update
sudo apt install git dotnet-sdk-9.0 desktop-file-utils
```

If `dotnet-sdk-9.0` is not available from your normal repositories, configure the official Microsoft/Ubuntu package source or a suitable Ubuntu backports source first.

### Clone

```bash
git clone --recurse-submodules https://github.com/lirycutarefson/nma-community-linux-fix.git
cd nma-community-linux-fix
```

### Build

```bash
dotnet build NexusMods.App.sln -c Release
```

### Publish self-contained Linux build

```bash
dotnet publish src/NexusMods.App/NexusMods.App.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:Version=0.21.1-community.1-heroic-gog-cyberpunk \
  -p:TieredCompilation=true \
  -p:PublishReadyToRun=true \
  -p:PublishSingleFile=true \
  -o /data/Apps/NMA-Community-Linux-Fix
```

Run:

```bash
/data/Apps/NMA-Community-Linux-Fix/NexusMods.App
```

## Release naming

Suggested release tag:

```text
v0.21.1-community.1-heroic-gog-cyberpunk
```

Suggested release title:

```text
NMA Community Linux Fix v0.21.1-community.1 - Heroic/GOG Cyberpunk fixes
```

Suggested release summary:

```text
Unofficial community fork based on NexusMods.App v0.21.1.

Changes:
- Fix Heroic/GOG DLC detection by using HeroicGOGGame.InstalledDLCs.
- Avoid blocking Cyberpunk 2077 Heroic/GOG loadouts on optional GOG Galaxy metadata files:
  - goggame-*.info
  - goggame-*.hashdb
- Suppress the corresponding false-positive missing-game-files diagnostic for existing loadouts.
- Increase MaximumBackupSize to 128 GB.
- Include Linux desktop/icon integration notes.

This is not an official Nexus Mods release.
Use at your own risk.
```

## License

This project is based on NexusMods.App and remains licensed under the GNU General Public License v3.0.

You must preserve the upstream license and copyright notices when redistributing this fork or binaries built from it.

If you distribute binaries, also provide the corresponding source code for the exact version used to build them.

## Trademark / naming note

This project is not named "Nexus Mods App" because it is not an official Nexus Mods tool.

The name `NMA Community Linux Fix` is intended to make clear that this is a community fork, not an official Nexus Mods release.

## Credits

Original project:

- Nexus Mods App by Nexus Mods and contributors

Community fork / Linux fixes:

- Cyril Cestça / lirycutarefson

## Status

Experimental but initially validated for:

```text
Cyberpunk 2077 GOG via Heroic
Phantom Liberty installed
French language files installed
Health Check: passed with no issue
```

Further testing is needed before using this fork with large mod collections or other games.
