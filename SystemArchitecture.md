# Islands of Memory - System Architecture

## 1. Overall System Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                     Islands of Memory Game                       │
│                      Unity 3D Game System                        │
└─────────────────────────────────────────────────────────────────┘
                                │
                ┌───────────────┼───────────────┐
                │               │               │
        ┌───────▼──────┐ ┌─────▼─────┐ ┌──────▼──────┐
        │ Core Systems │ │ Gameplay  │ │ UI Systems  │
        │              │ │ Systems   │ │             │
        └──────────────┘ └───────────┘ └─────────────┘
```

---

## 2. Core Systems Architecture

### 2.1 Player Control System
```
┌──────────────────────────────────────────────────────┐
│              Player Control System                    │
├──────────────────────────────────────────────────────┤
│                                                       │
│  ┌─────────────────┐        ┌──────────────────┐   │
│  │ PlayerMovement  │───────▶│ CharacterController│  │
│  │                 │        │                   │   │
│  │ - Walk/Run      │        │ - Physics         │   │
│  │ - Jump          │        │ - Collision       │   │
│  │ - Rotation      │        │                   │   │
│  │ - Animation     │        └──────────────────┘   │
│  └────────┬────────┘                               │
│           │                                         │
│           ▼                                         │
│  ┌─────────────────┐                               │
│  │   Animator      │                               │
│  │                 │                               │
│  │ - Idle/Walk/Run │                               │
│  │ - Jump/Fall     │                               │
│  │ - Land          │                               │
│  └─────────────────┘                               │
└──────────────────────────────────────────────────────┘
```

### 2.2 Camera System
```
┌──────────────────────────────────────────────────────┐
│              Camera System                            │
├──────────────────────────────────────────────────────┤
│                                                       │
│  ┌─────────────────────────────────────────────┐    │
│  │         TopDownCamera                       │    │
│  │                                             │    │
│  │  ┌──────────────┐      ┌──────────────┐   │    │
│  │  │  Walk Mode   │      │  Boat Mode   │   │    │
│  │  │              │      │              │   │    │
│  │  │ - Fixed Angle│      │ - Follow Boat│   │    │
│  │  │ - Follow     │      │ - Dynamic    │   │    │
│  │  │   Player     │      │   Rotation   │   │    │
│  │  └──────────────┘      └──────────────┘   │    │
│  │                                             │    │
│  │  Features:                                  │    │
│  │  - Smooth Position Transition               │    │
│  │  - Smooth Rotation Lerp                     │    │
│  │  - Mode Switching                           │    │
│  │  - Snap Function                            │    │
│  └─────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────┘
```

---

## 3. Gameplay Systems Architecture

### 3.1 Boat System
```
┌──────────────────────────────────────────────────────────────┐
│                    Boat System                                │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌────────────────┐      ┌────────────────┐                 │
│  │ BoatController │◀────▶│   OarSystem    │                 │
│  │                │      │                │                 │
│  │ - Input (A/D)  │      │ - Visual Anim  │                 │
│  │ - Movement     │      │ - Splash VFX   │                 │
│  │ - Turning      │      │ - Audio SFX    │                 │
│  │ - Physics      │      └────────────────┘                 │
│  └────────┬───────┘                                          │
│           │                                                   │
│           ▼                                                   │
│  ┌────────────────┐                                          │
│  │   Rigidbody    │                                          │
│  │  (Kinematic)   │                                          │
│  └────────────────┘                                          │
│                                                               │
│  ┌────────────────────────────────────────────────┐         │
│  │         BoatBoarding                           │         │
│  │                                                │         │
│  │  - Player Enter/Exit                           │         │
│  │  - Control Transfer                            │         │
│  │  - Camera Mode Switch                          │         │
│  │  - Dock Detection                              │         │
│  │  - Parent/Unparent Player                      │         │
│  └────────────────────────────────────────────────┘         │
└──────────────────────────────────────────────────────────────┘
```

### 3.2 Puzzle System
```
┌──────────────────────────────────────────────────────────────┐
│                   Puzzle System                               │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐                                        │
│  │  PuzzlePickup    │                                        │
│  │  (World Objects) │                                        │
│  │                  │                                        │
│  │  - Winter Island │                                        │
│  │  - Autumn Island │                                        │
│  │  - Summer Island │                                        │
│  │  - Last Island   │                                        │
│  └────────┬─────────┘                                        │
│           │ Trigger                                           │
│           ▼                                                   │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │ PuzzleProgress   │◀────▶│ PuzzleUIManager  │            │
│  │                  │      │                  │            │
│  │ - 4 Slot States  │      │ - Preview Panel  │            │
│  │ - Lock/Unlock    │      │ - Image Display  │            │
│  │ - Visual Update  │      │ - Close Button   │            │
│  └──────────────────┘      └──────────────────┘            │
│                                                               │
│  Data Flow:                                                   │
│  Pickup → Unlock → UI Update → Player Preview               │
└──────────────────────────────────────────────────────────────┘
```

### 3.3 NPC & Guide System
```
┌──────────────────────────────────────────────────────────────┐
│                NPC & Guide System                             │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐                                        │
│  │  GuideOrbAI      │                                        │
│  │                  │                                        │
│  │  States:         │                                        │
│  │  ┌─────────────────────────────────────┐                │
│  │  │ 1. FollowPlayer                     │                │
│  │  │ 2. LeadToTarget                     │                │
│  │  │ 3. WaitAtTarget                     │                │
│  │  │ 4. PromptInteract                   │                │
│  │  └─────────────────────────────────────┘                │
│  │                  │                                        │
│  │  Uses NavMeshAgent for pathfinding                       │
│  └──────────────────┘                                        │
│                                                               │
│  ┌──────────────────┐                                        │
│  │ NPCFacingHint    │                                        │
│  │                  │                                        │
│  │ - Face Player    │                                        │
│  │ - Show Hint UI   │                                        │
│  │ - Smooth Rotation│                                        │
│  └──────────────────┘                                        │
└──────────────────────────────────────────────────────────────┘
```

---

## 4. UI Systems Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                     UI System                                 │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌────────────────────────────────────────────┐             │
│  │         HUD Elements                       │             │
│  │                                            │             │
│  │  ┌──────────────────┐                     │             │
│  │  │ PuzzleProgress   │  (Top-right corner) │             │
│  │  │ - 4 Slot Display │                     │             │
│  │  └──────────────────┘                     │             │
│  │                                            │             │
│  │  ┌──────────────────┐                     │             │
│  │  │ BoatHintTimed    │  (Temporary hints)  │             │
│  │  │ - Timed Display  │                     │             │
│  │  └──────────────────┘                     │             │
│  │                                            │             │
│  │  ┌──────────────────┐                     │             │
│  │  │ ZoneHint         │  (Area triggers)    │             │
│  │  │ - Location Hints │                     │             │
│  │  └──────────────────┘                     │             │
│  └────────────────────────────────────────────┘             │
│                                                               │
│  ┌────────────────────────────────────────────┐             │
│  │         Popup Panels                       │             │
│  │                                            │             │
│  │  ┌──────────────────┐                     │             │
│  │  │PuzzleUIManager   │  (Full screen)      │             │
│  │  │ - Preview Panel  │                     │             │
│  │  │ - Close Button   │                     │             │
│  │  └──────────────────┘                     │             │
│  └────────────────────────────────────────────┘             │
│                                                               │
│  ┌────────────────────────────────────────────┐             │
│  │         Intro System                       │             │
│  │                                            │             │
│  │  ┌──────────────────┐                     │             │
│  │  │  IntroFlow       │  (Game start)       │             │
│  │  │ - Slide Display  │                     │             │
│  │  │ - Time Control   │                     │             │
│  │  └──────────────────┘                     │             │
│  └────────────────────────────────────────────┘             │
└──────────────────────────────────────────────────────────────┘
```

---

## 5. Audio System

```
┌──────────────────────────────────────────────────────────────┐
│                   Audio System                                │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐                                        │
│  │   BGMManager     │  (Singleton)                           │
│  │                  │                                        │
│  │ - Background     │                                        │
│  │   Music          │                                        │
│  │ - Volume Control │                                        │
│  │ - DontDestroy    │                                        │
│  │   OnLoad         │                                        │
│  └──────────────────┘                                        │
│                                                               │
│  ┌──────────────────┐                                        │
│  │  SFX System      │                                        │
│  │                  │                                        │
│  │ - Oar Splash     │  (3D Audio)                           │
│  │ - Dynamic        │                                        │
│  │   AudioSource    │                                        │
│  │ - Pitch Variation│                                        │
│  └──────────────────┘                                        │
└──────────────────────────────────────────────────────────────┘
```

---

## 6. Utility Systems

```
┌──────────────────────────────────────────────────────────────┐
│                  Utility Systems                              │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐                                        │
│  │BillboardToCamera │                                        │
│  │                  │                                        │
│  │ - Always face    │                                        │
│  │   main camera    │                                        │
│  │ - Used for UI    │                                        │
│  │   elements       │                                        │
│  └──────────────────┘                                        │
│                                                               │
│  ┌──────────────────┐                                        │
│  │   DockZone       │                                        │
│  │                  │                                        │
│  │ - Exit points    │                                        │
│  │ - Boat docking   │                                        │
│  └──────────────────┘                                        │
│                                                               │
│  ┌──────────────────┐                                        │
│  │   ZoneHint       │                                        │
│  │                  │                                        │
│  │ - Trigger zones  │                                        │
│  │ - Context hints  │                                        │
│  └──────────────────┘                                        │
└──────────────────────────────────────────────────────────────┘
```

---

## 7. System Interaction Flow

### 7.1 Player Walking Flow
```
Player Input → PlayerMovement → CharacterController → Animation
                    ↓
              TopDownCamera (Walk Mode)
```

### 7.2 Boat Boarding Flow
```
Player Near Boat → Press E → BoatBoarding
                                  ↓
                    ┌─────────────┴─────────────┐
                    ▼                           ▼
          Disable PlayerMovement      Enable BoatController
                    ▼                           ▼
          Parent to Seat              Switch Camera to Boat Mode
                    ▼
              Show Boat Hint
```

### 7.3 Puzzle Collection Flow
```
Player Trigger → PuzzlePickup → PuzzleProgress.UnlockPiece()
                                        ↓
                                Update UI Slot
                                        ↓
                                Enable Preview Button
                                        ↓
                          Player Click → PuzzleUIManager.ShowPuzzle()
```

### 7.4 Boat Movement Flow
```
Input (A/D) → BoatController.Stroke()
                    ↓
        ┌───────────┴───────────┐
        ▼                       ▼
  Update Velocity         OarSystem.Stroke()
        ▼                       ▼
  Apply Motion           Visual Animation
        ▼                       ▼
  Rigidbody Move         Splash VFX + Audio
```

---

## 8. Component Dependencies

### High-Level Dependencies
```
PlayerMovement ──┐
                 ├──▶ TopDownCamera
BoatController ──┘

BoatBoarding ────┬──▶ PlayerMovement
                 ├──▶ BoatController
                 ├──▶ TopDownCamera
                 └──▶ BoatHintTimed

BoatController ──▶ OarSystem

PuzzlePickup ────▶ PuzzleProgress

PuzzleProgress ──▶ PuzzleUIManager

GuideOrbAI ──────▶ NavMeshAgent
```

---

## 9. Key Design Patterns

### 9.1 Singleton Pattern
- **BGMManager**: Ensures only one background music manager exists

### 9.2 State Machine Pattern
- **GuideOrbAI**: Four distinct states for AI behavior
- **TopDownCamera**: Two camera modes (Walk/Boat)

### 9.3 Component-Based Architecture
- Each system is a separate MonoBehaviour component
- Loose coupling through public references
- Unity's component system for modularity

### 9.4 Observer Pattern (Implicit)
- UI updates when puzzle state changes
- Camera responds to player/boat state changes

---

## 10. Technical Stack

### Core Technologies
- **Engine**: Unity 2022.3.x
- **Language**: C# (.NET)
- **Physics**: Unity CharacterController, Rigidbody
- **Navigation**: Unity NavMesh
- **Rendering**: Universal Render Pipeline (URP)
- **UI**: Unity UI (uGUI), TextMeshPro

### Key Unity Systems Used
- CharacterController (Player movement)
- Rigidbody (Boat physics)
- NavMeshAgent (AI pathfinding)
- Animator (Character animations)
- Particle System (VFX)
- Audio System (BGM & SFX)
- UI System (Canvas, Image, Button, Text)

---

## 11. Scene Structure

```
Main Scene
├── Player
│   ├── PlayerMovement
│   ├── CharacterController
│   └── Animator
├── Boat
│   ├── BoatController
│   ├── BoatBoarding
│   ├── OarSystem
│   └── Rigidbody
├── Camera
│   └── TopDownCamera
├── UI Canvas
│   ├── PuzzleProgress
│   ├── PuzzleUIManager
│   └── BoatHintTimed
├── Islands (4x)
│   └── PuzzlePickup (on each)
├── GuideOrb
│   ├── GuideOrbAI
│   └── NavMeshAgent
├── NPCs
│   └── NPCFacingHint
├── Audio
│   └── BGMManager
└── Environment
    ├── NavMesh
    ├── DockZones
    └── ZoneHints
```

---

## 12. Data Flow Summary

```
┌─────────────┐
│   Input     │
└──────┬──────┘
       │
       ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Gameplay   │────▶│   State     │────▶│     UI      │
│  Systems    │     │  Management │     │   Update    │
└─────────────┘     └─────────────┘     └─────────────┘
       │                    │                    │
       ▼                    ▼                    ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Physics    │     │  Animation  │     │   Audio     │
│  Update     │     │   Update    │     │   Playback  │
└─────────────┘     └─────────────┘     └─────────────┘
```

---

## Summary

This architecture demonstrates a modular, component-based design typical of Unity games. The system is divided into clear subsystems:

1. **Core Systems**: Player control and camera
2. **Gameplay Systems**: Boat mechanics, puzzles, NPCs
3. **UI Systems**: HUD, popups, hints
4. **Audio Systems**: Background music and sound effects
5. **Utility Systems**: Helper components

Each system is designed to be relatively independent while communicating through well-defined interfaces, making the codebase maintainable and extensible.
