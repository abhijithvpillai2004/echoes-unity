# Echoes: Dungeon of Loops

[![Engine](https://img.shields.io/badge/Unity-6000.0.80f1%20(Unity%206)-blue.svg)](https://unity.com/)
[![Language](https://img.shields.io/badge/C%23-10.0%2B-green.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Genre](https://img.shields.io/badge/Genre-2D%20Turn--Based%20Grid%20Tactics%20Puzzle-purple.svg)]()
[![Platform](https://img.shields.io/badge/Platform-PC%20%2F%20Windows-lightgrey.svg)]()

> *"You can't escape alone. Good thing you're never truly gone."*

**Echoes: Dungeon of Loops** is a 2D turn-based grid tactics puzzle game where death and repetition are transformed into cooperative puzzle-solving instruments. Trapped inside an ancient, cursed Citadel, a lone mage manipulates time: each completed run records your exact actions, spawning ghostly **Echoes** on subsequent loops to hold switches, disarm deadly traps, pin patrol monsters, and serve as sacrificial decoys against dungeon terrors.

---

## 🎮 The Core Game Theory: The Loop & Echo Rule

In **Echoes**, time moves only when you move. Every turn is deterministic, grid-locked, and precisely recorded.

```
[Start Loop] ──> [Player Takes Turns (Move / Wait / Interact / Attack)] ──> [Actions Recorded]
      ▲                                                                                 │
      │                                                                                 ▼
[Loop Resets] <── [Previous Runs Replay as Ghost Echoes] <── [Loop Limit / Fast-Forward]
```

### The $N + 1$ Principle
Every level follows a strict temporal design formula:
$$\text{For Level } N \implies \text{Total Loops} = N + 1 \quad \text{and} \quad \text{Total Past Echoes} = N$$

| Level | Title | Loops | Echoes | Primary Innovation & Mechanics |
| :---: | :--- | :---: | :---: | :--- |
| **1** | **The Entrance** | 2 | 1 | Basic loop recording, pressure plates, and toggle doors |
| **2** | **Trap Corridor** | 3 | 2 | Sentry path pinning (Crab) and synchronized barrier gating |
| **3** | **Slime Patrol** | 4 | 3 | Multi-tiered fortress corridors & precision hazard timing |
| **4** | **Grand Armory** | 5 | 4 | Weapon collection, 3-heart combat system, and decoy sacrifices |
| **5** | **Citadel of Time** | 6 | 5 | 5 concurrent Echo roles, spike corridors, sentry elimination, and Ghost Boss battle |

> **Non-Redundancy Principle:** Every single Echo must fulfill an indispensable, deliberate task. No loop is wasted; skipping turns without purpose breaks the chain required to conquer the Citadel.

---

## 🕹️ Controls & Player Reference

| Key | Action | Context |
| :---: | :--- | :--- |
| <kbd>W</kbd> <kbd>A</kbd> <kbd>S</kbd> <kbd>D</kbd> / Arrow Keys | **Move** (Up, Left, Down, Right) | Grid-based navigation (1 tile per turn) |
| <kbd>Space</kbd> | **Wait / Skip Turn** | Pass a turn without changing position |
| <kbd>E</kbd> | **Interact / Collect** | Pick up weapons or health potions |
| <kbd>X</kbd> or <kbd>F</kbd> | **Attack** | Strike an adjacent enemy (requires weapon) |
| <kbd>Enter</kbd> / <kbd>Q</kbd> / <kbd>Tab</kbd> | **Fast-Forward Loop** | Auto-fills remaining turns with wait commands |
| <kbd>Esc</kbd> / `[i]` HUD Button | **Toggle Help Overlay** | In-game controls and tips display |
| **Mouse Left-Click** | **Navigate / Select** | Temporal Adventure Map level selection & UI |

---

## 🗺️ Campaign Levels Overview

### 🏛️ Level 1: The Entrance (2 Loops, 1 Echo)
- **Objective:** Learn the fundamentals of time loops.
- **Strategy:** Echo 1 steps onto the pressure plate to hold the gate open. In Loop 2, the live mage walks past the open gate to claim the chest.

### 🦀 Level 2: Trap Corridor / The Blocker (3 Loops, 2 Echoes)
- **Objective:** Learn sentry manipulation and body-blocking.
- **Strategy:** Echo 1 holds the main gate. Echo 2 navigates into the corridor to physically block the patrolling Crab from reaching the corridor choke point. Loop 3 slips through the opening.

### 🧪 Level 3: Slime Patrol / Triple Tier Fortress (4 Loops, 3 Echoes)
- **Objective:** Coordinate multi-tier passages and synchronized timing.
- **Strategy:** Echoes 1, 2, and 3 each navigate into separate defensive tiers to hold sequential switches and draw sentries away, allowing the live runner to reach the inner sanctum.

### ⚔️ Level 4: Grand Armory (5 Loops, 4 Echoes)
- **Objective:** Weapon collection and tactical decoy combat.
- **Strategy:** Introduce the 3-heart HP system. Earlier echoes open the armory gates and position themselves in sentry aggro lanes. The live player collects the Enchanted Weapon, flanks the distracted sentries, and slays them.

### 👑 Level 5: Citadel of Time (6 Loops, 5 Echoes)
- **Objective:** The Grand Climax against the Citadel's forces and the Ghost Boss.
- **Strategy:**
  1. *Echo 1:* Disarms the lower spike field and unlocks Door 1.
  2. *Echo 2:* Waits for spike cycles and opens the Armory Door.
  3. *Echo 3:* Acts as decoy for the West Demon Sentry.
  4. *Echo 4:* Acts as decoy for the East Spider Sentry.
  5. *Echo 5:* Disarms courtyard hazard barriers and locks the Ghost Boss in place.
  6. *Live Mage (Loop 6):* Equips the Golden Weapon, eliminates both flanking sentries, enters the inner chamber, and strikes down the 2-HP Ghost Boss to claim the Golden Chest!

---

## 🛠️ Architecture & Script Structure

The codebase is built on clean, decoupled C# patterns (Command Pattern, State Pattern, and Event-driven UI):

```
Assets/Scripts/
├── ActionRecorder.cs       # Turn-by-turn input & command recording for replay fidelity
├── AudioManager.cs         # Sound effects and ambient audio controller
├── CollectibleItem.cs      # Collectible weapons and health potions on the grid
├── CombatEnemy.cs          # Breadcrumb BFS pathfinding, aggro priority, and attack logic
├── Commands.cs             # Command pattern implementations (Move, Wait, Attack, Interact)
├── EchoController.cs       # Replays recorded ICommand sequences for past ghost entities
├── EntityGlow.cs           # Procedural sine-wave dynamic lighting for chests & bosses
├── GridManager.cs          # World-to-grid mapping, walkable cell registry, and barrier lookups
├── HelpButton.cs           # In-game control HUD overlay toggle
├── ICommand.cs             # Core command interface for deterministic replaying
├── Level5BossController.cs # Custom state logic for the Citadel Ghost Boss
├── LevelCompleteController.cs # Celebratory party scene runner and progression handler
├── LevelManager.cs         # Per-scene state, chest triggers, and victory validation
├── LevelNode.cs            # World map node visual states (unlocked, completed, locked)
├── LevelSelectManager.cs   # Temporal Adventure Map manager with PlayerPrefs persistence
├── LoopManager.cs          # Turn execution orchestrator, loop resets, and win/loss rules
├── MapPlayerPawn.cs        # Animated world map pawn with waypoint traversal and echo footprints
├── PermanentWall.cs        # Solid physical obstacle definition
├── PlayerCombat.cs         # Weapon inventory management and attack execution
├── PlayerHealth.cs         # 3-heart HP management and context-sensitive HUD visibility
├── PlayerInputHandler.cs   # Keyboard and gamepad input routing
├── PressurePlate.cs        # Sensor stepping logic linked to gates and hazards
├── TitleScreenController.cs# Title screen cinematic transitions and effects
├── ToggleWall.cs           # Dynamic open/closed walls updating grid walkability
├── TrapTile.cs             # Retractable spike traps and hazard logic
└── Unit.cs                 # Base grid-aligned unit controller (Player & Echoes)
```

---

## 🚀 Getting Started

### Prerequisites
- **Unity Engine:** `6000.0.80f1` (Unity 6) or compatible newer release.
- **Git** with **Git LFS** enabled (recommended for asset handling).

### Installation & Setup
1. **Clone the repository:**
   ```bash
   git clone https://github.com/abhijithvpillai2004/echoes-unity.git
   cd echoes-unity
   ```

2. **Open in Unity Hub:**
   - Launch **Unity Hub**.
   - Click **Add** $\to$ **Add project from disk**.
   - Select the cloned `echoes-unity` folder.
   - Ensure the editor version is set to **Unity 6 (6000.0.x)**.

3. **Playing the Game:**
   - In the Project window, navigate to `Assets/Scenes/`.
   - Open **`TitleScreen.unity`**.
   - Hit the **Play** button in the Unity Editor toolbar.

### Scene Build Order
Ensure the scenes in **File $\to$ Build Settings** are in the following order:
```
[0] Assets/Scenes/TitleScreen.unity
[1] Assets/Scenes/LevelSelect.unity
[2] Assets/Scenes/Level1.unity
[3] Assets/Scenes/level2.unity
[4] Assets/Scenes/Level3.unity
[5] Assets/Scenes/Level4.unity
[6] Assets/Scenes/Level5.unity
[7] Assets/Scenes/LevelComplete.unity
```

---

## 🎨 Asset Attributions
- **Audio:** Kenney Audio Packs (1-bit / RPG audio effects under CC0).
- **Fonts & UI:** TextMeshPro (Liberation Sans, Anton, Bangers, Roboto, Oswald).
- **Sprites & Tiles:** Custom dark fantasy pixel art tilesets, runes, and character sprites.

---

## 📜 License
This project is licensed under the [MIT License](LICENSE) (or project default). Feel free to use, learn from, and build upon the mechanics!
