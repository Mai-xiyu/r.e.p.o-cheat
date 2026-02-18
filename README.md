# r.e.p.o-cheat
made by D4rkks (and community) (second repo bc first one i fucked up)

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

### 👁️ Visuals (ESP)
- **Modern UI**: Clean and intuitive menu design.
- **Enemy ESP**: See enemies through walls with Boxes, Names, Distance, and Health.
- **Item ESP**: Track valuables with Box, Value, and Name. Includes a minimum value filter.
- **Player ESP**: Track allies/others with HP and Distance.
- **Extraction ESP**: Find exits easily.
- **Chams**: Customizable 3D Chams for both items and enemies.
- **Map Info**: Displays total value of items on the map and player status list.

### ⚔️ Combat & Players
- **Player Interaction**: Damage, heal, kill, or revive any player in the lobby.
- **Teleportation**: Move players to yourself, to each other, or into "The Void".
- **Social**: Spoof names or use a Rainbow Name effect.

### 👾 Enemies
- **Enemy Control**: Blind all enemies or kill them instantly.
- **Teleport Enemies**: Bring monsters to specific players.
- **Mob Spawner**: Spawn any enemy in the game (Host/MasterClient required).

### 📦 Items
- **Item Spawner**: Spawn valuables with custom values (Host/MasterClient required).
- **Item Teleport**: Pull all items to your position.
- **Remote Sell**: Sell items without going back to the ship.

### 🛠️ Misc & Server
- **Server Browser**: Powerful lobby search with filters (Region, Players, Hidden full lobbies).
- **Hotkey Manager**: Bind any cheat feature to your preferred keys.
- **Config System**: Save and load your cheat settings automatically.
- **Trolling Features**: Lobby Crasher, Fake Player Spawner, and NPC Spam.

---

## 🛠️ Requirements
- Any Mono Injector (like [SMI](https://github.com/wh0am15/SharpMonoInjector))
- The game **R.E.P.O**

## 🏗️ How to Build
1. Open the `.sln` or `.csproj` in **Visual Studio**.
2. Set configuration to **Release**.
3. Build Solution (Ctrl+Shift+B).
4. The DLL will be in `bin/Release/r.e.p.o cheat.dll`.

## 💉 How to Inject
Use the provided `start_and_inject.bat` or your favorite injector with:
- **Namespace**: `r.e.p.o_cheat`
- **Class**: `Loader`
- **Method**: `Init`

---

*Enjoy responsibility! Or not. It's a cheat after all.*
