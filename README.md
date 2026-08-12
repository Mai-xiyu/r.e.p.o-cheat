# r.e.p.o-cheat

An open-source C# Mono cheat for the Unity game [R.E.P.O.](https://store.steampowered.com/app/2594580/REPO/), injected at runtime through SharpMonoInjector and patched with Harmony.

> This project is open source and free to use. It is provided for personal and educational purposes only. Use it in your own private sessions and respect other players. The authors are not responsible for how it is used.

- [中文文档](README_CN.md)

## Compatibility

The build is adapted against the following installed game baseline:

| Component | Version |
|---|---|
| Game | v0.4.4.3 |
| Assembly-CSharp.dll SHA-256 | `CE995A182DDC884EA965E87786F1986248D9616300FA825BCC04BCA671EE6526` |
| Unity | 2022.3.67f2 (Mono) |
| Photon | PUN 2.52 |
| TextMeshPro | 1.4.0 |

Compatibility is decided by assembly fingerprint and capability detection, not by version string. After a game update, run the workflow described in [.api-compatibility.md](.api-compatibility.md) to re-validate the used API surface.

## Features

- **Self**: god mode, infinite health/stamina, noclip, customizable speed/strength/jump/gravity/grab range, no recoil/cooldown, grab through walls, custom FOV, no fog, auto dodge, self revive, teammate heal/revive, creative mode, one-click max upgrades (all 13 upgrade kinds), game speed control, game debug flags (infinite energy / no overcharge / no tumble).
- **Visuals**: TMP-based ESP (enemies, items, players, extraction points) with boxes/names/distance/health/value filters, chams, mini radar, trace lines, ESP presets, map value summary, player status list.
- **Combat and players**: damage/heal/kill/revive players, teleport players, name spoofing, possession, aimbot with smoothness and range settings.
- **Enemies**: blind/freeze/kill enemies, disable traps, teleport enemies, enemy spawner (host only).
- **Items**: item spawner with custom values (host only), item teleport, value inflation, duplicate held item, remote sell, auto pickup, auto sell.
- **Teleport**: crosshair teleport, extraction teleport, random teleport, named waypoints, synced through the game's own `PlayerAvatar.Spawn` API.
- **World**: zero haul goal, auto-complete round, chat commands (`!help`), shop tools (run currency via the game's own `SetRunStatSet`), level transitions via `RunManager.ChangeLevel`.
- **Cosmetics**: unlock all cosmetics (game-native `CosmeticUnlockAll` + save), random outfit, rainbow cosmetics cycle - all through the game's own MetaManager/PlayerCosmetics pipeline with lobby sync.
- **Room**: host takeover, lobby discovery/creation, RPC injection, anti-kick, anti-crash protection. These network modules are provided as-is; verify them in private lobbies only.
- **Game localization (new, off by default)**: Simplified Chinese rendering for the game UI through the game's own Unity.Localization string tables, with a CJK font fallback. See [Localization](#game-localization).

## Usage

1. Build the project (see below) or download `r.e.p.o.cheat.dll` from [Releases](https://github.com/Mai-xiyu/r.e.p.o-cheat/releases).
2. Place `r.e.p.o.cheat.dll` and SharpMonoInjector's `smi.exe` in the same folder.
3. Start R.E.P.O. and run from an elevated terminal:

   ```
   .\smi.exe inject -p repo -a r.e.p.o.cheat.dll -n r.e.p.o_cheat -c Loader -m Init
   ```

4. Open/close the menu with **Delete** (rebindable in the Hotkeys tab). **F5** reloads the menu, **F10** unloads the cheat.
5. `C:\temp\inject_debug.txt` records initialization and the per-class Harmony patch results.

## Game localization

The game UI can be rendered in Simplified Chinese as an optional feature, **disabled by default**. It is implemented through the game's own localization pipeline and is purely local: it never touches Photon state, RPCs, player data, or lobby state, and player names, chat messages, and lobby names are never translated.

- Enable it in the cheat menu under **Menu > Game Chinese** (toggle, language mode Auto / Simplified Chinese / English, missing-translation scanner).
- The embedded corpus covers the 603 official string-table keys (HUD, Menu, Game).
- External overrides merge over the embedded corpus: `<game root>\REPOChinese\zh-CN.json`.
- Missing translations always fall back to the original English. With the scanner enabled, untranslated game-authored strings are written to `<game root>\localization-missing.txt`.

## Configuration

- Toggles and sliders persist via Unity PlayerPrefs.
- Extended configuration (`DarkCheat_Config.json`) holds waypoints and full config.

## Building from source

Requirements: .NET SDK 8 or newer (the game reference assemblies in `libs/` are the only binaries required).

```
dotnet build "r.e.p.o cheat.sln" -c Release
```

The build output is `r.e.p.o cheat\bin\Release\r.e.p.o cheat.dll`. 0Harmony is embedded as a resource, so the DLL is self-contained.

## Tests

Unit tests cover the pure C# localization components (translation database, placeholder/rich-text validation) and the compatibility fingerprint logic:

```
powershell -ExecutionPolicy Bypass -File run-tests.ps1
```

## Project structure

| Directory | Purpose |
|---|---|
| `r.e.p.o cheat\Core` | Loader entry point, IMGUI menu, config, hotkeys, theme |
| `r.e.p.o cheat\Compatibility` | Game fingerprint and capability detection |
| `r.e.p.o cheat\Localization` | Game localization module and translation data |
| `r.e.p.o cheat\Player / Combat / Visuals / Items / World` | Feature modules |
| `r.e.p.o cheat\Network` | Room and network modules |
| `r.e.p.o cheat\Patches` | Harmony patches |
| `r.e.p.o cheat\Utils` | Shared helpers and the scene cache |
| `r.e.p.o cheat.Tests` | Unit tests |
| `libs` | Game reference assemblies (see Compatibility above) |
| `.api-compatibility.md` | Used-API compatibility matrix and update workflow |

## Contributing

Contributions are welcome. Please keep the compatibility layer up to date when adapting to a new game version: update the fingerprint in `Compatibility\GameVersionInfo.cs`, re-run the API diff described in `.api-compatibility.md`, and keep the test suite green.

## License

[MIT](LICENSE)
