# r.e.p.o-cheat
made by D4rkks (and community) (second repo bc first one i fucked up)

**🌐 [中文文档 / Chinese Version](README_CN.md)**

> [!WARNING]
> THIS IS A OPEN-SOURCE PROJECT! ITS NOT INTENDED TO BE SOLD OR TO BE THE ULTIMATE LAST R.E.P.O CHEAT, EVERYONE CAN USE IT AND FEEL FREE TO CONTRIBUTE!

Basic C# Mono open-source cheat for a new lethal like game called R.E.P.O.

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/45f59200-fed3-4c40-b13b-5d4c86e12ca8" />

---

## 🚀 Features

### 👤 Self
- **God Mode / Infinite Health**: Never die.
- **Infinite Stamina**: Run forever.
- **NoClip**: Fly through walls.
- **Customizable Stats**: Adjust your Speed, Strength, Jump Force, Gravity, Throw Strength, and Grab Range in real-time.
- **No Weapon Recoil/Cooldown**: Fire weapons without limitations.
- **Grab Through Walls**: Interactive with objects anywhere.
- **RGB Player**: Cycle through colors.
- **No Fog**: Clear visibility in all levels.
- **Custom FOV**: Change your Field of View from 60 to 120.
- **Auto Dodge**: Automatically dodge when enemies approach, with configurable trigger distance.
- **Self Revive**: Revive yourself when downed.
- **Heal & Revive Teammates**: Heal or revive other players.
- **Creative Mode**: One toggle to enable God Mode + Flight + NoClip + Infinite Stamina + Grab Through Walls + No Cooldown + No Fog simultaneously, with WASD flight controls.
- **One-Click Max Upgrades**: Max all 6 skill upgrades instantly (Grab Strength, Throw, Sprint, Range, Jump, Tumble).
- **Game Speed Control**: Adjust game speed from 0.1x to 5x with slider and quick buttons (0.5x/1x/2x/3x).
- **Network Stealth**: Disable network synchronization to become invisible to other players (anti-cheat bypass).

### 👁️ Visuals (ESP)
- **Modern ESP**: Clean TMP-based ESP labels with CJK font support, yellow for items, red for enemies.
- **Enemy ESP**: See enemies through walls with Boxes, Names, Distance, and Health.
- **Item ESP**: Track valuables with Box, Value, and Name. Includes a minimum value filter and level whitelist.
- **Player ESP**: Track allies/others with HP and Distance.
- **Extraction ESP**: Find exits easily.
- **Chams**: Customizable 3D Chams for both items and enemies.
- **Map Info**: Displays total value of items on the map and player status list.
- **Trace Lines**: Draw lines from screen bottom to enemies, items, or players.
- **ESP Presets**: 6 one-click presets (Custom / High Value / Enemy Only / Player Only / Everything / Stealth).
- **Mini Radar**: Circular radar overlay showing enemies (red), players (green), items (yellow), and extraction points (blue).

### ⚔️ Combat & Players
- **Player Interaction**: Damage, heal, kill, or revive any player in the lobby.
- **Teleportation**: Move players to yourself, to each other, or into "The Void".
- **Social**: Spoof names or use a Rainbow Name effect.
- **Player Possession**: Take control of another player's movement.
- **Aimbot**: Auto-aim at enemies when holding a gun + right mouse button, with adjustable smoothness and range.

### 👾 Enemies
- **Enemy Control**: Blind all enemies or kill them instantly.
- **Freeze All Enemies**: Freeze all enemies in place (persists across new spawns).
- **Disable All Traps**: Deactivate all traps and hazards on the map.
- **Teleport Enemies**: Bring monsters to specific players.
- **Mob Spawner**: Spawn any enemy in the game (Host/MasterClient required). Fallback to Resources if EnemySpawner unavailable.

### 📦 Items
- **Item Spawner**: Spawn valuables with custom values (Host/MasterClient required).
- **Item Teleport**: Pull all items to your position (works for both host and non-host players).
- **Value Inflation**: 10x, 100x, or MAX ($99,999) all item values on the map.
- **Duplicate Held Item**: Clone whatever you're currently holding.
- **Remote Sell**: Sell items without going back to the ship.
- **Auto Pickup**: Automatically collect nearby valuables within configurable radius and minimum value filter.
- **Auto Sell**: Teleport items to extraction point for hands-free selling.

### 🔀 Teleport+
- **Crosshair Teleport**: Instantly teleport to where you're looking.
- **Extraction Teleport**: Jump to the nearest extraction point.
- **Random Teleport**: Teleport to a random location within range.
- **Waypoint System**: Save, load, and manage named teleport waypoints.

### 🛠️ Misc & Tools
- **Zero Haul Goal**: Set the extraction haul target to zero for instant round completion.
- **Auto-Complete Round**: One-click to activate extractions, teleport all items, and zero the haul goal.
- **Chat Commands**: Type `!help` in chat to see all 15+ commands (!god, !noclip, !tp, !kill, !speed, !heal, !items, !upgrade, !freeze, !inflate, !dup, !stealth, etc.).
- **Force Host**: 6-method host takeover system (SetMaster, LowLevel, ActorSpoof, CrashHost, LocalFake, AutoAll).

### 🎨 Menu Settings
- **Background Color**: Customize the menu background color with RGBA sliders and live preview.
- **Background Image**: Load a custom background image from file or clear to solid color.
- **Reload Menu**: Reinitialize the menu styles and layout.
- **Unload Menu**: Completely remove the cheat from the game (with confirmation).
- **Return to Main Menu**: Quickly return to the game's main menu screen.
- **Quit Game**: Exit the game (with confirmation).

### 🌐 Server & Config
- **Server Browser**: Powerful lobby search with filters (Region, Players, Hidden full lobbies).
- **Hotkey Manager**: Bind any cheat feature to your preferred keys.
- **Config System**: Save and load your cheat settings automatically.
- **JSON Config**: Export/Import all settings to a JSON file for easy backup and sharing.
- **Multilingual UI**: Auto-detects all embedded i18n language packs (Chinese/English). Switch languages via dropdown in config page.
- **Trolling Features**: Lobby Crasher, Fake Player Spawner, and NPC Spam.

---

## 🛠️ Requirements
- Any Mono Injector (like [SMI](https://github.com/wh0am15/SharpMonoInjector))
- The game **R.E.P.O**

## 🏗️ How to Build
1. Open the `.sln` or `.csproj` in **Visual Studio** or use `dotnet build`.
2. Set configuration to **Release**.
3. Build Solution (`Ctrl+Shift+B` or `dotnet build "r.e.p.o cheat.sln" -c Release`).
4. The DLL will be in `bin/Release/r.e.p.o cheat.dll`.

## 💉 How to Inject
Use the provided `start_and_inject.bat` or your favorite injector with:
- **Namespace**: `r.e.p.o_cheat`
- **Class**: `Loader`
- **Method**: `Init`

---

## 🌐 Adding a New Language

1. Create a new text file at `r.e.p.o cheat/i18n/<lang_code>.txt` (e.g., `ja.txt`, `ko.txt`).
2. Copy `en.txt` as a template and translate all values.
3. Make sure the file includes `lang.name=<Display Name>` (e.g., `lang.name=日本語`).
4. Set the file's **Build Action** to **Embedded Resource** in the `.csproj`.
5. Rebuild — the language will be auto-detected and appear in the config dropdown.

---

*Enjoy responsibly! Or not. It's a cheat after all.*

---

## 📝 Postscript

Thanks to D4rkks (and the community) for creating this project.

I don't know much about C#; I only have a grasp of basic C/C++, Java, Python and Kotlin. For this project, I decompiled the DLL released from the original repository at https://github.com/D4rkks/r.e.p.o-cheat and restored the source code with the help of Claude AI. I ask the author to forgive me for doing this, especially as I'm just an incompetent sophomore majoring in Computer Science...
