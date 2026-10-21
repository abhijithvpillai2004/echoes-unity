# Echoes: Dungeon of Loops — Complete Project Chronicle & Game Design Document

> **Project Name:** Echoes: Dungeon of Loops  
> **Engine:** Unity 6.0.0f1 (C#)  
> **Genre:** 2D Turn-Based Grid Tactics Puzzle / Dungeon Explorer  
> **Aesthetic:** Dark Fantasy Pixel Art, Ancient Runic Parchment, Ethereal Echo VFX  
> **Document Scope:** Full Record of Ideation, Architecture, Mechanics, Level Designs, Bug Fixes, UI Evolution, and Presentation Guide  

---

## Table of Contents
1. [Executive Summary & Core Philosophy](#1-executive-summary--core-philosophy)
2. [Core Game Theory: The Loop & Echo Rule](#2-core-game-theory-the-loop--echo-rule)
3. [Complete Level Walkthroughs & Solutions](#3-complete-level-walkthroughs--solutions)
   - [Level 1: The Entrance](#level-1-the-entrance-2-loops-1-echo)
   - [Level 2: Trap Corridor / The Blocker](#level-2-trap-corridor--the-blocker-3-loops-2-echoes)
   - [Level 3: Slime Patrol / Triple Tier Fortress](#level-3-slime-patrol--triple-tier-fortress-4-loops-3-echoes)
   - [Level 4: Grand Armory](#level-4-grand-armory-5-loops-4-echoes)
   - [Level 5: Citadel of Time](#level-5-citadel-of-time-6-loops-5-echoes)
4. [Combat, Health & Entity Mechanics](#4-combat-health--entity-mechanics)
5. [Scene Flow & Meta-Progression Architecture](#5-scene-flow--meta-progression-architecture)
   - [Title Screen](#titlescreenunity)
   - [Temporal Adventure Map](#levelselectunity)
   - [Level Complete Celebration](#levelcompleteunity)
6. [Script Directory & Technical Blueprint](#6-script-directory--technical-blueprint)
7. [Chronological Development & Iteration Log](#7-chronological-development--iteration-log)
8. [Controls & Player Reference Guide](#8-controls--player-reference-guide)

---

## 1. Executive Summary & Core Philosophy

**Echoes: Dungeon of Loops** is a turn-based grid puzzle game that redefines death and repetition as cooperative puzzle-solving tools. 

### The Core Loop
```
[Start Loop] ──> [Player Takes Turns (Move / Wait / Interact)] ──> [Actions Recorded]
      ▲                                                                     │
      │                                                                     ▼
[Loop Resets] <── [Previous Runs Replay as Ghost Echoes] <── [Loop Limit / Advance]
```

- **Tagline:** *"You can't escape alone. Good thing you're never truly gone."*
- **Premise:** In the cursed Citadel, a lone mage is trapped in temporal loops. Instead of resetting progress upon failure or turn exhaustion, previous runs materialize as translucent **Echoes** that replay exact movements, activating switches, blocking hazards, and serving as decoys against dungeon terrors.

---

## 2. Core Game Theory: The Loop & Echo Rule

The fundamental rule governing every level's design is:

$$\text{For Level } N \implies \text{Total Loops} = N + 1 \quad \text{and} \quad \text{Total Past Echoes} = N$$

| Level | Level Title | Loops | Echoes | Primary Innovation |
| :---: | :--- | :---: | :---: | :--- |
| **1** | The Entrance | 2 | 1 | Basic Loop recording, pressure plates, and toggle doors |
| **2** | Trap Corridor | 3 | 2 | Enemy path blocking (Crab) and synchronized gating |
| **3** | Slime Patrol | 4 | 3 | Multi-tiered corridor traversal & obstacle timing |
| **4** | Grand Armory | 5 | 4 | Real-time combat, weapon collection, HP hearts, decoy sacrifices |
| **5** | Citadel of Time | 6 | 5 | 5 simultaneous Echo duties, hazard barriers, sentry elimination, Ghost Boss battle, glowing Golden Chest |

### Non-Redundancy Principle
Every single Echo must have an indispensable, deliberate task. No loop is wasted; skipping or idling without purpose violates the game theory. Echoes open doors, lure sentries, disarm hazards, or act as sacrificial human shields.

---

## 3. Complete Level Walkthroughs & Solutions

### Level 1: "The Entrance" (2 Loops, 1 Echo)
- **Grid Coordinates:** Player starts at `(-4, -4)`. Goal Chest at `(0, 3)`.
- **Turns per Loop:** 12
- **Objective:** Master pressure plates and door toggling.
- **Walkthrough:**
  1. **Loop 1 (Echo 1):** Walk right across the lower corridor to `(0, -2)` and stand directly on the Pressure Plate. Wait/Fast-Forward until loop ends.
  2. **Loop 2 (Live Player):** While Echo 1 stands on the plate holding the door open at `(0, 0)`, walk through the open gate straight up to `(0, 3)` to open the chest.

---

### Level 2: "Trap Corridor / The Blocker" (3 Loops, 2 Echoes)
- **Grid Layout:** 8×8 grid with bottom chamber, stone wall with double door at $Y=0$, crab patrol lane at $Y=1$, fence with single gap at $(0, 2)$, and chest at $(0, 3)$.
- **Turns per Loop:** 12
- **Walkthrough:**
  1. **Loop 1 (Echo 1):** Walk right to `(0, -2)` and stand on the Pressure Plate to hold the main door open.
  2. **Loop 2 (Echo 2):** Move through the door into the patrol lane at `(-1, 1)` or `(1, 1)` to intercept and block the patrolling Crab away from the fence gap.
  3. **Loop 3 (Live Player):** Echo 1 holds the door; Echo 2 traps the Crab. Player walks cleanly through the open door, through the fence gap at `(0, 2)`, and into the chest at `(0, 3)`.

---

### Level 3: "Slime Patrol / Triple Tier Fortress" (4 Loops, 3 Echoes)
- **Grid Layout:** Three distinct tiers separated by walls and doors, guarded by patrolling sentries.
- **Turns per Loop:** 20
- **Walkthrough:**
  1. **Loop 1 (Echo 1):** Unlocks Tier 1 door and secures the lower passage.
  2. **Loop 2 (Echo 2):** Passes into Tier 2 and locks down the secondary pressure sensor.
  3. **Loop 3 (Echo 3):** Navigates to Tier 3 to disarm the upper barrier and block the sentry's return path.
  4. **Loop 4 (Live Player):** Moves freely through all 3 tiers with all Echoes maintaining clear passage to the chest.

---

### Level 4: "Grand Armory" (5 Loops, 4 Echoes)
- **Grid Layout:** Armory with weapon pedestal, potion niche, twin Slime sentries, and 3-Heart combat system.
- **Turns per Loop:** 24
- **Walkthrough:**
  1. **Loop 1 (Echo 1):** Steps on Plate 1 at `(2, -2)` to open Door 1.
  2. **Loop 2 (Echo 2):** Steps on Plate 2 at `(-2, 0)` to open Door 2, then moves into sentry aggro range to serve as a decoy.
  3. **Loop 3 (Echo 3):** Moves into position to hold the tertiary gate and distract the second Slime.
  4. **Loop 4 (Echo 4):** Navigates to secure the potion room and reinforce decoy positions.
  5. **Loop 5 (Live Player):**
     - Navigates through open gates to grab the **Enchanted Weapon** (`E`).
     - Attacks distracted Slime 1 (`X`) $\to$ Slime 1 slain.
     - Attacks distracted Slime 2 (`X`) $\to$ Slime 2 slain.
     - (Optional: collects Health Potion at `(-3, 1)`).
     - Steps into the Grand Armory chest to clear the level.

---

### Level 5: "Citadel of Time" (6 Loops, 5 Echoes)
- **Grid Layout:** Full Citadel fortress with lower spike corridor, armory, west courtyard (Demon), east courtyard (Spider), central throne hazard barrier, and Ghost Boss chamber.
- **Turns per Loop:** 50 turns
- **Walkthrough:**
  1. **Loop 1 (Echo 1 — Spike & Door 1 Keeper):**
     - Press `D` 8 times to walk right and step on **Plate 1 at `(4, -5)`**.
     - Fast-forward (`Enter` / `Q` / `Tab`).
     - *Result:* Disarms bottom 3 spike traps & opens Door 1 at `(1, -4)`.
  2. **Loop 2 (Echo 2 — Armory Master):**
     - Press `D` 3 times, press `Space` 6 times (waiting for spikes to retract).
     - Move `D` 3 times through Door 1, then `W` 3 times, `A` 6 times to stand on **Plate 2 at `(-4, -2)`**.
     - Fast-forward.
     - *Result:* Opens Armory Door 2 at `(2, -3)`.
  3. **Loop 3 (Echo 3 — West Demon Sentry Decoy):**
     - Move through Door 1, up into courtyard (`W` 6 times), then `A` 2 times to stand on **Plate 3 at `(-2, 1)`**.
     - Fast-forward.
     - *Result:* Opens West Gate Door 3 and lures West Demon into attacking Echo 3.
  4. **Loop 4 (Echo 4 — East Spider Sentry Decoy):**
     - Move through Door 1, up into courtyard, then right `D` 4 times, `W` 1 time to stand on **Plate 4 at `(2, 1)`**.
     - Fast-forward.
     - *Result:* Opens East Gate Door 4 and lures East Spider into attacking Echo 4.
  5. **Loop 5 (Echo 5 — Hazard Barrier Disarmer & Ghost Boss Decoy):**
     - Move through Door 1, up into Central Courtyard to stand on **Plate 5 at `(0, 0)`**.
     - Fast-forward.
     - *Result:* Disarms upper courtyard spikes in front of throne room and lures Ghost Boss to `(0, 0)`.
  6. **Loop 6 (Live Player — Grand Finale):**
     - Enter Armory room at `(2, -3)` and grab the **Golden Weapon** at `(4, -3)` (`E`).
     - Slay distracted West Demon (`X` or `F`).
     - Slay distracted East Spider (`X` or `F`).
     - Both sentries slain $\to$ Throne Gate (Door 5) automatically opens and Ghost Boss emerges.
     - Flank Ghost Boss (locked on Echo 5) and strike **twice** (`X` or `F`) $\to$ **Ghost Boss Slain!**
     - Walk through throne gate to open the glowing **Golden Chest at `(0, 5)`** $\to$ **GAME WON!**

---

## 4. Combat, Health & Entity Mechanics

### Health & Heart UI (`PlayerHealth.cs`)
- **Capacity:** 3 HP displayed as pixel hearts in the upper-left HUD.
- **Sprites:** Full Heart (`heart_full.png`) and Broken Heart (`heart_broken.png`).
- **Context-Aware Visibility:** Health UI is dynamically displayed **only in combat levels** (Levels 4 and 5) where enemies and weapons exist. In pure puzzle levels (Levels 1, 2, and 3), the heart container automatically hides to keep the screen clean.
- **Sacrificial Mechanics:** If the player takes lethal damage before the final loop, they are sacrificed as a decoy for the next loop rather than triggering an immediate Game Over. Game Over only occurs if HP reaches zero during the final loop.

### Combat Enemy AI (`CombatEnemy.cs`)
- **Target Priority:** Prioritizes adjacent Echo decoys over the live player, allowing clever players to flank enemies.
- **Pathfinding:** Breadcrumb BFS pathfinding with item avoidance (cannot walk over active weapon tiles).
- **Damage Flash & Death:** Enemies flash red on hit and execute a smooth alpha/scale death fade upon defeat.
- **Boss Enemy:** Ghost Boss has 2 HP, white ethereal glow (`EntityGlow.cs`), and custom luring logic.

---

## 5. Scene Flow & Meta-Progression Architecture

### Scene Sequence in Build Settings:
```
[0] TitleScreen ──> [1] LevelSelect ──> [2] Level1 ──> [3] level2 ──> [4] Level3 ──> [5] Level4 ──> [6] Level5 ──> [7] LevelComplete
         ▲                  │                                                                                             │
         └──────────────────┴─────────────────────────────────────────────────────────────────────────────────────────────┘
```

### TitleScreen.unity
- Cinematic dark atmosphere with glowing golden Title logo (`ECHOES: DUNGEON OF LOOPS`).
- Smooth alpha sine wave pulse on `[ PRESS ANY KEY TO BEGIN ]`.
- Fades out seamlessly and transitions to `LevelSelect`.

### LevelSelect.unity (Temporal Adventure Map)
- **Map Layout:** Stylized dungeon parchment map featuring 5 sequential nodes connected by dynamic dotted routes (gold for unlocked routes, subtle stone grey for locked routes).
- **Animated Mage Pawn (`MapPlayerPawn.cs`):**
  - Floats above the current level node with idle breathing bob and cyan runic aura.
  - When clicking any unlocked destination, smoothly traverses intermediate dotted waypoints.
  - Flips sprite horizontally toward travel direction and leaves a trail of fading cyan **Echo Footprints**.
  - Plays celebratory arrival hop before loading the scene.
- **Node Visuals:**
  - **Unlocked:** Glowing emblem with crisp 11pt nameplate.
  - **Locked:** Studded obsidian medallion (`ui_node_locked_cool.png`) with heavy gothic padlock (`ui_icon_lock_cool.png`).
  - **Level 5 Boss:** Dedicated amber `★ BOSS` crown badge.
- **Interactive Info Panel:** Hovering over nodes shows level name, objective, status (`[ COMPLETED ]` / `[ READY TO ENTER ]`), and contextual click prompts.
- **Persistence:** Progress saved using `PlayerPrefs` (`HighestUnlockedLevel`, `LevelCompleted_X`).

### LevelComplete.unity
- Intermediary celebratory scene displaying the Purple Mage running across the screen, followed by $N$ translucent cyan Echoes matching the completed level count.
- Large gold header (`Level X Complete!`) with randomized encouraging quotes.
- Automatically transitions to the next level or back to the Adventure Map.

---

## 6. Script Directory & Technical Blueprint

| Script Name | Responsibility |
| :--- | :--- |
| `LoopManager.cs` | Turn execution, command submission, loop resets, echo spawning, win/loss triggers |
| `Unit.cs` | Grid-based character component for player and echoes |
| `ActionRecorder.cs` | Turn-by-turn action recording for replay fidelity |
| `EchoController.cs` | Replays recorded `ICommand` sequences for past ghosts |
| `GridManager.cs` | World-to-grid mapping, walkable cell registry, barrier lookups |
| `Commands.cs` | Encapsulates `MoveCommand`, `WaitCommand`, `AttackCommand`, and `InteractCommand` |
| `PressurePlate.cs` | Sensor stepping logic, triggering linked doors and traps |
| `ToggleWall.cs` | Wall/door open-close toggling and grid walkability updates |
| `CombatEnemy.cs` | Enemy movement, BFS chasing, attack prioritization, HP, and death fades |
| `PlayerHealth.cs` | 3-heart health system, conditional HUD display, healing and damage handling |
| `PlayerCombat.cs` | Weapon inventory check, weapon pickup visual, attacking adjacent enemies |
| `LevelSelectManager.cs` | Adventure map progression, PlayerPrefs unlock logic, node generation, audio |
| `MapPlayerPawn.cs` | Animated mage traversal, hop bounce cycles, and echo footprint trails |
| `LevelNode.cs` | Node states (unlocked, completed, locked), hover scaling, nameplates |
| `TitleScreenController.cs` | Title screen fading, pulsing prompts, transition to LevelSelect |
| `LevelCompleteController.cs` | Running party animation and level progression sequencing |
| `HelpButton.cs` | In-game HUD control overlay toggle (`i` button and Escape key) |
| `EntityGlow.cs` | Dynamic sine-wave procedural lighting aura for chest and boss |

---

## 7. Chronological Development & Iteration Log

### Phase 1: Project Intake & Core Mechanics (Steps 0 – 900)
- Ingested existing Unity MCP project `Echoes`.
- Debugged `EnemyPatrol.cs` boundary freezing bugs and grid alignment offsets.
- Standardized player starting coordinates to bottom-left `(-4, -4)` across all levels.
- Replaced visual-only wall gaps with logic-blocking `PermanentWall` and `ToggleWall` structures.

### Phase 2: Game Theory Standardization & Level Progression (Steps 900 – 1400)
- Established the **$N+1$ Loops / $N$ Echoes** game theory rule across the campaign.
- Built Level 2 ("The Blocker") requiring Echo 1 door holding and Echo 2 crab pinning.
- Configured Level 3 ("Triple Tier Fortress") with multi-tier sentries and precision timing.
- Added level name banners outside the map boundaries and victory celebration screens.
- Created `TitleScreen.unity` with cinematic pixel art styling.

### Phase 3: Combat, Weapons & Help System (Steps 1400 – 2100)
- Created `LevelComplete.unity` running party transition scene.
- Introduced weapon collection (`E`), attack command (`X`/`F`), and enemy death fade animations in Level 4.
- Implemented 3-heart HP system with full/broken heart states and blue potion healing (`tilemap_packed_116`).
- Created `HelpButton.cs` with unobtrusive `i` button and semi-transparent controls guide popup.
- Fixed enemy movement stalls by preventing enemies from stepping onto collectible item tiles.

### Phase 4: Level 5 Citadel of Time & Boss Design (Steps 2100 – 2800)
- Redesigned Level 5 into the grand climax: 6 Loops, 5 Echoes, 50-turn allowance.
- Created the 5 dedicated Echo roles: Door 1/spikes, Armory door, West Demon decoy, East Spider decoy, Courtyard hazard barrier & Ghost Boss decoy.
- Introduced the Ghost Boss (`tilemap_packed_121`) with 2 HP and custom white aura glow.
- Created procedural golden glowing treasure chest (`EntityGlow.cs`).
- Replaced levers and spikes with high-detail custom pixel art sprites (`lever_custom.png`, `spike_custom.png`).

### Phase 5: Temporal Adventure Map & UI Polish (Steps 2800 – 3200)
- Created `LevelSelect.unity` world map with 5 interconnected nodes and PlayerPrefs persistence.
- Added animated `MapPlayerPawn.cs` allowing the mage to run along dotted routes with echo footprints.
- Designed custom runic obsidian locked nodes (`ui_node_locked_cool.png`) and gothic padlocks (`ui_icon_lock_cool.png`).
- Fixed text overlap issues and tuned UI typography to clean, compact sizes (11pt nameplates, 24pt headers).
- Proportionally scaled node circles to 54×54 px for optimal visual balance.
- Resolved missing Door 1 gap in Level 5, scaled levers to 0.7, and made HP Heart UI conditional only on combat levels.

---

## 8. Controls & Player Reference Guide

| Key | Action | Context |
| :---: | :--- | :--- |
| **W / A / S / D** | Move Up / Left / Down / Right | In-game movement on 1-tile grid |
| **Space** | Wait (Skip Turn) | Advances turn without moving |
| **E** | Interact / Collect | Pick up weapon or health potion |
| **X** or **F** | Attack | Strike adjacent enemy (requires weapon) |
| **Enter** / **Q** / **Tab** | Fast-Forward Loop | Fills remaining turns with wait commands |
| **Escape** | Close Help / Menu | Toggles in-game controls overlay |
| **Left Click** | Select Node / Navigate | Adventure map selection and UI interaction |

---

*Document compiled and preserved directly from the complete Antigravity development trajectory.*
