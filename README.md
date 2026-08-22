# r.e.p.o-cheat

An open-source C# Mono cheat for the Unity game [R.E.P.O.](https://store.steampowered.com/app/2594580/REPO/), injected at runtime through SharpMonoInjector and patched with Harmony.

> This project is open source and free to use. It is provided for personal and educational purposes only. Use it in your own private sessions and respect other players. The authors are not responsible for how it is used.

- [中文文档](README_CN.md)

**Current release: [v2.5.0](https://github.com/Mai-xiyu/r.e.p.o-cheat/releases/tag/v2.5.0)** — adapted for R.E.P.O. v0.4.4.3.

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

Host-only actions are tagged `[Host]` in the menu. Guests keep owner-local and unsecured game APIs (heal others, item teleport, extraction request, held-item battery, `PhotonNetwork.Instantiate` spawns). Do not fake `MasterOnlyRPC` senders.

- **Self**: god mode, infinite health/stamina, **unlimited battery** (guests: held/equipped items; guns/drones stay host-authoritative), noclip, customizable speed/strength/jump/gravity/grab range (via `PunManager.UpdateStat`), no recoil/cooldown, **instant gun build-up**, grab through walls, custom FOV, no fog, auto dodge, self revive, teammate heal/revive (`HealOtherRPC`), creative mode, one-click max upgrades (all 13 upgrade kinds), game speed control, game debug flags (infinite energy / no overcharge / no tumble / slow walk / feather fall), **hide grabber beam, hide item labels, no camera shake, cinematic HUD, unlock FPS**, ignore death pit, infinite spectate battery, cosmetic tokens.
- **Visuals**: TMP-based ESP (enemies, items, players, extraction points) with boxes/names/distance/health/value filters, chams, mini radar, trace lines, ESP presets, map value summary, player status list, **full map reveal**.
- **Combat and players**: damage/heal/kill/revive players, teleport players, name spoofing, possession, aimbot with smoothness and range settings, **bullet track** (host/solo; redirects `ItemGun.ShootBullet`).
- **Enemies**: blind/freeze/kill/despawn enemies via the game's own EnemyDirector/EnemyParent APIs, disable traps (`Trap` base type), **teleport enemies** (`EnemyTeleported`, guest-safe), enemy spawner (host only, `PhotonNetwork.InstantiateRoomObject`), **easy grab / spawn close / no spawn pause** debug flags.
- **Items**: item spawner from `StatsManager.itemDictionary` (host: room objects; **guest: `PhotonNetwork.Instantiate`**, may despawn on leave). Item teleport (`SetPositionRPC`), value inflation (host), duplicate held item, **charge held battery**, remote sell, auto pickup, auto sell.
- **Teleport**: crosshair teleport, extraction teleport, random teleport, named waypoints, synced through the game's own `PhysGrabObject.Teleport` API.
- **World**: zero haul goal / **low-haul debug flag**, auto-complete round, chat commands (`!help`), shop tools (run currency via `SetRunStatSet` plus **cheap shop prices**, host), extra lives, unlock extraction, **request extraction** (`RoundDirector.RequestExtractionPointActivation`, guest-safe), discover valuables (`DiscoverRPC`), fill valuables next level, level transitions via `RunManager.ChangeLevel` plus **named level picker** (`debugLevel`). Native wrappers live in `World/NativeGameApi.cs`.
- **Cosmetics**: unlock all cosmetics (game-native `CosmeticUnlockAll` + save), random outfit, rainbow cosmetics cycle - all through the game's own MetaManager/PlayerCosmetics pipeline with lobby sync.
- **Room**: host takeover (no leftover local-master fake after Auto), identity watchdog (does not rewrite `IsMasterClient`), ownership tools that skip player avatars, honest RPC injector, lobby discovery/creation, anti-kick, anti-crash protection. Use **Reset identity** if other tabs desync. Verify network modules in private lobbies only.
- **Game localization (off by default)**: Simplified Chinese rendering for the game UI through the game's own Unity.Localization string tables, with a CJK font fallback. See [Localization](#game-localization).

## Usage

1. Download `DarkMenu.Injector.exe` from [Releases](https://github.com/Mai-xiyu/r.e.p.o-cheat/releases), or build it from source below.
2. Start R.E.P.O. normally.
3. Run `DarkMenu.Injector.exe` and approve its Windows UAC prompt. It targets only `repo.exe`; the payload DLL is embedded, so no separate DLL or `smi.exe` is required.
4. Open/close the menu with **Delete** (rebindable in the Hotkeys tab). **F5** reloads the menu, **F10** unloads the cheat.
5. Launcher diagnostics are written to `%LOCALAPPDATA%\DarkMenu\injector.log`; initialization and Harmony patch results remain in `C:\temp\inject_debug.txt`.

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

Requirements: .NET SDK 8 or newer (the game reference assemblies in `libs/` are the only binaries required). Clone with submodules so the open-source injector dependency is available.

```
dotnet build "r.e.p.o cheat.sln" -c Release
dotnet publish "RepoInjector/RepoInjector.csproj" -c Release -o artifacts/injector
```

The launcher output is `artifacts\injector\DarkMenu.Injector.exe`. It requests administrator rights through its application manifest, embeds the payload DLL, and bundles the MIT-licensed SharpMonoInjector core. The payload DLL remains available at `r.e.p.o cheat\bin\Release\r.e.p.o cheat.dll` for development.

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

## Changelog

### [v2.3.0](https://github.com/Mai-xiyu/r.e.p.o-cheat/releases/tag/v2.3.0) — 2026-08-16

- Guest-safe paths for unlimited battery (held items), extraction request, item spawn/duplicate, valuable discovery, and item teleport. Host-only controls are labeled in the menu.
- `NativeGameApi` wraps live v0.4.4.3 debug/host APIs (enemies, map, haul, shop, levels, battery fill).
- Room tab: Force Host no longer leaves a local master fake that breaks battery/guns/haul. Shadow Host is an identity watchdog. Authority tools skip player PhotonViews. RPC injector no longer spoofs the host sender.
- Self/combat extras: ignore death pit, spectate battery, bullet track (host/solo).
- Tests: `powershell -ExecutionPolicy Bypass -File run-tests.ps1` (21/21).

### [v2.2.0](https://github.com/Mai-xiyu/r.e.p.o-cheat/releases/tag/v2.2.0)

Cosmetics suite and game-API feature overhaul for v0.4.4.3.

## License

[MIT](LICENSE)
