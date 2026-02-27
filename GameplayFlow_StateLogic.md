# Gameplay Flow & State Logic

## Table of Contents
1. [Game States Overview](#1-game-states-overview)
2. [Player State Machine](#2-player-state-machine)
3. [Boat State Machine](#3-boat-state-machine)
4. [Camera State Transitions](#4-camera-state-transitions)
5. [Puzzle Collection Flow](#5-puzzle-collection-flow)
6. [NPC Interaction Flow](#6-npc-interaction-flow)
7. [Complete Gameplay Loop](#7-complete-gameplay-loop)
8. [State Transition Rules](#8-state-transition-rules)

---

## 1. Game States Overview

### 1.1 Global Game States

```
┌─────────────────────────────────────────────────────────┐
│                   Game State Hierarchy                   │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────┐                                       │
│  │   Intro      │  (Game Start)                         │
│  │              │                                       │
│  │ - Show slides│                                       │
│  │ - Time.scale │                                       │
│  │   = 0        │                                       │
│  └──────┬───────┘                                       │
│         │                                                │
│         ▼                                                │
│  ┌──────────────┐                                       │
│  │   Playing    │  (Main Gameplay)                      │
│  │              │                                       │
│  │ ┌──────────┐ │                                       │
│  │ │ Walking  │ │ ◀─┐                                   │
│  │ └────┬─────┘ │   │                                   │
│  │      │       │   │                                   │
│  │      ▼       │   │                                   │
│  │ ┌──────────┐ │   │                                   │
│  │ │ Boating  │ │ ──┘                                   │
│  │ └──────────┘ │                                       │
│  └──────────────┘                                       │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 1.2 State Variables

| State Variable | Type | Description |
|---------------|------|-------------|
| `inBoat` | bool | Player is in boat |
| `playerInRange` | bool | Player near boat |
| `currentDock` | DockZone | Current dock reference |
| `unlocked[]` | bool[4] | Puzzle unlock states |
| `state` | GuideOrbAI.State | Guide orb AI state |
| `mode` | TopDownCamera.Mode | Camera mode |

---

## 2. Player State Machine

### 2.1 Player Movement States

```
┌─────────────────────────────────────────────────────────┐
│              Player Movement State Machine               │
├─────────────────────────────────────────────────────────┤
│                                                          │
│         ┌──────────────┐                                │
│    ┌───│  IDLE_STAND  │◀──┐                            │
│    │   └──────────────┘   │                            │
│    │          │            │                            │
│    │          │ Move Input │                            │
│    │          ▼            │                            │
│    │   ┌──────────────┐   │ Stop                       │
│    │   │WALK_FORWARD  │───┘                            │
│    │   └──────┬───────┘                                │
│    │          │                                         │
│    │          │ Hold Shift                              │
│    │          ▼                                         │
│    │   ┌──────────────┐                                │
│    │   │ RUN_FORWARD  │                                │
│    │   └──────┬───────┘                                │
│    │          │                                         │
│    │          │ Release Shift                           │
│    │          └──────────────┐                         │
│    │                         │                         │
│    │ Space (Grounded)        │                         │
│    │          │               │                         │
│    │          ▼               ▼                         │
│    │   ┌──────────────┐   ┌──────────────┐            │
│    └──▶│     JUMP     │   │     FALL     │            │
│        └──────┬───────┘   └──────┬───────┘            │
│               │                   │                     │
│               │ velocity.y < 0    │                     │
│               └──────────┬────────┘                     │
│                          │                              │
│                          ▼                              │
│                   ┌──────────────┐                     │
│                   │     LAND     │                     │
│                   └──────┬───────┘                     │
│                          │                              │
│                          │ landLockTimer expires        │
│                          ▼                              │
│                   ┌──────────────┐                     │
│                   │  IDLE_STAND  │                     │
│                   └──────────────┘                     │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Player State Transitions

| Current State | Trigger | Next State | Conditions |
|--------------|---------|------------|------------|
| IDLE_STAND | Movement Input | WALK_FORWARD | input.magnitude > 0.001 |
| WALK_FORWARD | Hold Shift | RUN_FORWARD | sprint = true |
| WALK_FORWARD | Stop Input | IDLE_STAND | input.magnitude < 0.001 |
| RUN_FORWARD | Release Shift | WALK_FORWARD | sprint = false |
| Any Grounded | Space Key | JUMP | grounded = true |
| JUMP | velocity.y < 0 | FALL | velocity.y < 0.05 |
| FALL | Touch Ground | LAND | grounded = true |
| LAND | Timer Expires | IDLE_STAND | landLockTimer <= 0 |

### 2.3 Animation State Mapping

```
State ID    │ Animation State    │ Trigger Condition
────────────┼───────────────────┼──────────────────────────
11          │ IDLE_STAND        │ No input, grounded
21          │ WALK_FORWARD      │ Moving, not sprinting
31          │ RUN_FORWARD       │ Moving + Shift key
41          │ JUMP              │ velocity.y > 0.05
61          │ FALL              │ velocity.y < 0.05
71          │ LAND              │ Just landed, timer active
```

---

## 3. Boat State Machine

### 3.1 Boat Control States

```
┌─────────────────────────────────────────────────────────┐
│               Boat Controller State Flow                 │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────┐                                       │
│  │   DISABLED   │  (Player not in boat)                 │
│  └──────┬───────┘                                       │
│         │                                                │
│         │ Player Boards (Press E)                        │
│         ▼                                                │
│  ┌──────────────┐                                       │
│  │   ENABLED    │  (Player in boat)                     │
│  │              │                                       │
│  │  ┌────────────────────────────────┐                 │
│  │  │  Input Processing Loop         │                 │
│  │  │                                │                 │
│  │  │  ┌──────────────┐             │                 │
│  │  │  │    IDLE      │             │                 │
│  │  │  └──────┬───────┘             │                 │
│  │  │         │                      │                 │
│  │  │    A/D Key Press               │                 │
│  │  │         │                      │                 │
│  │  │         ▼                      │                 │
│  │  │  ┌──────────────┐             │                 │
│  │  │  │   STROKING   │             │                 │
│  │  │  │              │             │                 │
│  │  │  │ - Add forward│             │                 │
│  │  │  │   velocity   │             │                 │
│  │  │  │ - Check turn │             │                 │
│  │  │  │ - Trigger oar│             │                 │
│  │  │  │   animation  │             │                 │
│  │  │  └──────┬───────┘             │                 │
│  │  │         │                      │                 │
│  │  │         │ minStrokeInterval    │                 │
│  │  │         ▼                      │                 │
│  │  │  ┌──────────────┐             │                 │
│  │  │  │   COOLDOWN   │             │                 │
│  │  │  └──────┬───────┘             │                 │
│  │  │         │                      │                 │
│  │  │         │ Timer expires        │                 │
│  │  │         └──────────────────────┘                 │
│  │  │                                                   │
│  │  └───────────────────────────────────┐              │
│  │                                      │              │
│  └──────────────────────────────────────┘              │
│         │                                               │
│         │ Player Exits (Press E at dock)                │
│         ▼                                               │
│  ┌──────────────┐                                      │
│  │   DISABLED   │                                      │
│  └──────────────┘                                      │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Boat Turning Logic

```
Stroke Sequence Analysis:
─────────────────────────────────────────────────────

Stroke 1 (Left):   sameSideCount = 1, lastSide = -1
                   → Forward only

Stroke 2 (Left):   sameSideCount = 2, lastSide = -1
                   → Forward + Turn RIGHT

Stroke 3 (Left):   sameSideCount = 3, lastSide = -1
                   → Forward + Turn RIGHT (stronger)

Stroke 4 (Right):  sameSideCount = 1, lastSide = +1
                   → Forward only (reset turn)

Stroke 5 (Right):  sameSideCount = 2, lastSide = +1
                   → Forward + Turn LEFT
```

### 3.3 Boat Physics Update

```
Every FixedUpdate():
┌────────────────────────────────────────┐
│ 1. Damping                             │
│    forwardVel → 0 (forwardDamping)     │
│    yawVelTarget → 0 (yawDamping)       │
├────────────────────────────────────────┤
│ 2. Smooth Turning                      │
│    yawVel = Lerp(yawVel, yawVelTarget) │
├────────────────────────────────────────┤
│ 3. Calculate New Position              │
│    newPos = pos + forward * vel * dt   │
├────────────────────────────────────────┤
│ 4. Calculate New Rotation              │
│    newRot = rot * yaw rotation         │
├────────────────────────────────────────┤
│ 5. Apply to Rigidbody                  │
│    rb.MovePosition(newPos)             │
│    rb.MoveRotation(newRot)             │
└────────────────────────────────────────┘
```

---

## 4. Camera State Transitions

### 4.1 Camera Mode State Machine

```
┌─────────────────────────────────────────────────────────┐
│              Camera Mode State Machine                   │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────────────────────────────┐              │
│  │         WALK MODE                    │              │
│  │                                      │              │
│  │  Target: Player                      │              │
│  │  Height: 16 units                    │              │
│  │  Back: 16 units                      │              │
│  │  Pitch: 45°                          │              │
│  │  Smooth: 0.12s                       │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Player Boards Boat                        │
│             │ (BoatBoarding.Board())                    │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │   TRANSITION (Snap)                  │              │
│  │                                      │              │
│  │  - Set new target (Boat)             │              │
│  │  - Set boat forward ref              │              │
│  │  - Instant position update           │              │
│  │  - Reset velocity                    │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │         BOAT MODE                    │              │
│  │                                      │              │
│  │  Target: Boat                        │              │
│  │  Height: 8 units                     │              │
│  │  Back: 10 units                      │              │
│  │  Pitch: 20°                          │              │
│  │  Smooth: 0.03s                       │              │
│  │  Follow: Boat forward direction      │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Player Exits Boat                         │
│             │ (BoatBoarding.TryExit())                  │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │   TRANSITION (Snap)                  │              │
│  │                                      │              │
│  │  - Set new target (Player)           │              │
│  │  - Instant position update           │              │
│  │  - Reset velocity                    │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │         WALK MODE                    │              │
│  └──────────────────────────────────────┘              │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 4.2 Camera Update Logic

```
LateUpdate() Flow:
┌────────────────────────────────────────┐
│ 1. Check Current Mode                  │
├────────────────────────────────────────┤
│ 2. Compute Desired Position            │
│    - Walk: Fixed angle behind player   │
│    - Boat: Follow boat direction       │
├────────────────────────────────────────┤
│ 3. Compute Desired Rotation            │
│    - Walk: Fixed pitch + optional yaw  │
│    - Boat: Follow boat yaw + offset    │
├────────────────────────────────────────┤
│ 4. Smooth Position                     │
│    - SmoothDamp with mode-specific time│
├────────────────────────────────────────┤
│ 5. Smooth Rotation                     │
│    - Slerp with exponential decay      │
└────────────────────────────────────────┘
```

---

## 5. Puzzle Collection Flow

### 5.1 Puzzle State Machine

```
┌─────────────────────────────────────────────────────────┐
│            Puzzle Collection State Flow                  │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Island: Winter/Autumn/Summer/Last                       │
│                                                          │
│  ┌──────────────────────────────────────┐              │
│  │   PUZZLE LOCKED (Initial State)      │              │
│  │                                      │              │
│  │   UI Slot:                           │              │
│  │   - Locked sprite                    │              │
│  │   - Alpha: 0.35                      │              │
│  │   - Button: Disabled                 │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Player Enters Trigger Zone                │
│             │ (PuzzlePickup.OnTriggerEnter)             │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │   PLAYER IN RANGE                    │              │
│  │                                      │              │
│  │   playerInside = true                │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Press E                                   │
│             │ (PuzzlePickup.DoPickup)                   │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │   PICKUP TRIGGERED                   │              │
│  │                                      │              │
│  │   1. Call PuzzleProgress.UnlockPiece │              │
│  │   2. Deactivate pickup object        │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │   PUZZLE UNLOCKED                    │              │
│  │                                      │              │
│  │   UI Slot:                           │              │
│  │   - Unlocked sprite                  │              │
│  │   - Alpha: 1.0                       │              │
│  │   - Button: Enabled                  │              │
│  │                                      │              │
│  │   unlocked[index] = true             │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Player Clicks Slot Button                 │
│             │ (PuzzleUIManager.ShowPuzzle)              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │   PREVIEW DISPLAYED                  │              │
│  │                                      │              │
│  │   - Show preview panel               │              │
│  │   - Display full puzzle image        │              │
│  │   - Enable close button              │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Click Close Button                        │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │   PREVIEW CLOSED                     │              │
│  │                                      │              │
│  │   Puzzle remains unlocked            │              │
│  └──────────────────────────────────────┘              │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 5.2 Puzzle Unlock Validation

```
ShowPuzzle(index) Logic:
┌────────────────────────────────────────┐
│ 1. Validate Index (0-3)                │
│    if (index < 0 || index >= 4)        │
│       return                            │
├────────────────────────────────────────┤
│ 2. Check Unlock Status                 │
│    if (!progress.IsUnlocked(index))    │
│       return (prevent viewing)          │
├────────────────────────────────────────┤
│ 3. Validate References                 │
│    if (panel == null || image == null) │
│       return                            │
├────────────────────────────────────────┤
│ 4. Display Preview                     │
│    previewImage.sprite = puzzles[index]│
│    previewPanel.SetActive(true)        │
└────────────────────────────────────────┘
```

---

## 6. NPC Interaction Flow

### 6.1 Guide Orb AI State Machine

```
┌─────────────────────────────────────────────────────────┐
│            Guide Orb AI State Machine                    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────────────────────────────┐              │
│  │      FOLLOW_PLAYER (Default)         │              │
│  │                                      │              │
│  │  Behavior:                           │              │
│  │  - Calculate position behind player  │              │
│  │  - Set NavMesh destination           │              │
│  │  - Maintain follow distance          │              │
│  │  - Adjust height                     │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ LeadTo(target) Called                     │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │      LEAD_TO_TARGET                  │              │
│  │                                      │              │
│  │  Behavior:                           │              │
│  │  - Set destination to target         │              │
│  │  - Move towards target               │              │
│  │  - Check arrival distance            │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Reached Target                            │
│             │ (remainingDistance <= arriveDistance)     │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │      WAIT_AT_TARGET                  │              │
│  │                                      │              │
│  │  Behavior:                           │              │
│  │  - Stop NavMeshAgent                 │              │
│  │  - Idle at target position           │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Manual State Change                       │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │      PROMPT_INTERACT                 │              │
│  │                                      │              │
│  │  Behavior:                           │              │
│  │  - Stop movement                     │              │
│  │  - Wait for player interaction       │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ BackToPlayer() Called                     │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │      FOLLOW_PLAYER                   │              │
│  └──────────────────────────────────────┘              │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 6.2 NPC Facing Behavior

```
NPCFacingHint Update Loop:
┌────────────────────────────────────────┐
│ 1. Check Player In Range               │
│    if (!playerInRange)                 │
│       Hide hint, return                │
├────────────────────────────────────────┤
│ 2. Show Hint UI                        │
│    hintCanvas.enabled = true           │
│    hintText.text = inRangeText         │
├────────────────────────────────────────┤
│ 3. Calculate Direction to Player       │
│    dir = player.pos - npc.pos          │
│    dir.y = 0 (flatten)                 │
├────────────────────────────────────────┤
│ 4. Smooth Rotation                     │
│    targetRot = LookRotation(dir)       │
│    npc.rot = Slerp(current, target)    │
└────────────────────────────────────────┘
```

---

## 7. Complete Gameplay Loop

### 7.1 Main Game Flow

```
┌─────────────────────────────────────────────────────────┐
│              Complete Gameplay Loop                      │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  START                                                   │
│    │                                                     │
│    ▼                                                     │
│  ┌──────────────────────────────────────┐              │
│  │  1. INTRO SEQUENCE                   │              │
│  │     - Display slides                 │              │
│  │     - Pause game (Time.scale = 0)    │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  2. EXPLORATION (Walking)            │              │
│  │     - Move with WASD                 │              │
│  │     - Jump with Space                │              │
│  │     - Follow guide orb               │              │
│  │     - Interact with NPCs             │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  3. FIND BOAT                        │              │
│  │     - Approach boat                  │              │
│  │     - See "Press E" hint             │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Press E                                   │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  4. BOARD BOAT                       │              │
│  │     - Disable walking controls       │              │
│  │     - Enable boat controls           │              │
│  │     - Switch camera mode             │              │
│  │     - Show boat hint                 │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  5. NAVIGATE TO ISLAND               │              │
│  │     - Row with A/D keys              │              │
│  │     - Follow zone hints              │              │
│  │     - Reach island dock              │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ At Dock + Press E                         │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  6. EXIT BOAT                        │              │
│  │     - Enable walking controls        │              │
│  │     - Disable boat controls          │              │
│  │     - Switch camera mode             │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  7. EXPLORE ISLAND                   │              │
│  │     - Find puzzle piece              │              │
│  │     - Trigger collection zone        │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             │ Press E on Puzzle                         │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  8. COLLECT PUZZLE                   │              │
│  │     - Unlock puzzle piece            │              │
│  │     - Update UI slot                 │              │
│  │     - Enable preview button          │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  9. REPEAT FOR OTHER ISLANDS         │              │
│  │     - Return to boat                 │              │
│  │     - Navigate to next island        │              │
│  │     - Collect remaining puzzles      │              │
│  └──────────┬───────────────────────────┘              │
│             │                                           │
│             ▼                                           │
│  ┌──────────────────────────────────────┐              │
│  │  10. VIEW COLLECTED PUZZLES          │              │
│  │      - Click UI slots                │              │
│  │      - View full images              │              │
│  │      - Complete memory collection    │              │
│  └──────────────────────────────────────┘              │
│                                                          │
│  END                                                     │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 7.2 Island Visit Sequence

```
For Each Island (Winter → Autumn → Summer → Last):

┌────────────────────────────────────────┐
│ 1. Board Boat at Main Dock             │
├────────────────────────────────────────┤
│ 2. Navigate to Island                  │
│    - Follow zone hints                 │
│    - Use A/D to row                    │
│    - Turn by repeating same side       │
├────────────────────────────────────────┤
│ 3. Dock at Island                      │
│    - Approach dock zone                │
│    - Exit boat (Press E)               │
├────────────────────────────────────────┤
│ 4. Explore Island                      │
│    - Walk around environment           │
│    - Find puzzle pickup                │
├────────────────────────────────────────┤
│ 5. Collect Puzzle                      │
│    - Trigger pickup zone               │
│    - Press E to collect                │
│    - See UI update                     │
├────────────────────────────────────────┤
│ 6. Return to Boat                      │
│    - Walk back to dock                 │
│    - Board boat (Press E)              │
├────────────────────────────────────────┤
│ 7. Navigate to Next Island             │
│    (Repeat until all 4 collected)      │
└────────────────────────────────────────┘
```

---

## 8. State Transition Rules

### 8.1 Boat Boarding Rules

| Condition | Action | Result |
|-----------|--------|--------|
| Player in range + Press E + Not in boat | Board() | Enter boat mode |
| In boat + Press E + At dock | TryExit() | Exit boat mode |
| In boat + Press E + Not at dock + onlyExitAtDock = true | Blocked | Stay in boat |
| In boat + Press E + Not at dock + onlyExitAtDock = false | TryExit() | Exit at fallback point |

### 8.2 Puzzle Interaction Rules

| Condition | Action | Result |
|-----------|--------|--------|
| Player in trigger + Press E + Not unlocked | DoPickup() | Unlock puzzle |
| Player in trigger + Press E + Already unlocked | Ignored | No action |
| Click UI slot + Unlocked | ShowPuzzle() | Display preview |
| Click UI slot + Locked | Blocked | No action |

### 8.3 Camera Mode Rules

| Trigger | From Mode | To Mode | Method |
|---------|-----------|---------|--------|
| Board boat | Walk | Boat | SetMode(Boat) + Snap() |
| Exit boat | Boat | Walk | SetMode(Walk) + Snap() |
| Game start | N/A | Walk | Start() initialization |

### 8.4 Guide Orb Behavior Rules

| Current State | Trigger | Next State | Condition |
|--------------|---------|------------|-----------|
| FollowPlayer | LeadTo() called | LeadToTarget | targetPoint != null |
| LeadToTarget | Reach target | WaitAtTarget | remainingDistance <= arriveDistance |
| LeadToTarget | Target null | FollowPlayer | targetPoint == null |
| WaitAtTarget | BackToPlayer() | FollowPlayer | Always |
| Any | BackToPlayer() | FollowPlayer | Always |

---

## Summary

This document outlines the complete state logic and gameplay flow for the Islands of Memory game. The game uses multiple interconnected state machines:

1. **Player State Machine**: Handles movement, jumping, and animation states
2. **Boat State Machine**: Manages boat control, rowing, and turning mechanics
3. **Camera State Machine**: Switches between walk and boat modes with smooth transitions
4. **Puzzle State Machine**: Tracks collection and unlock status of 4 puzzle pieces
5. **Guide Orb State Machine**: Controls AI behavior for player guidance
6. **NPC Interaction**: Manages NPC facing and hint display

The gameplay loop follows a clear progression:
- Start with intro
- Explore on foot
- Board boat
- Navigate to islands
- Collect puzzles
- Return and repeat

All state transitions are rule-based and deterministic, ensuring consistent and predictable gameplay behavior.
