# Islands of Memory

<p align="center">
  <img src="Assets/UI/game%20background.png" width="900" alt="Islands of Memory logo"/>
</p>

**Islands of Memory** is a 3D narrative exploration game developed in Unity for CMP-6056B Game and Mobile App Development at the University of East Anglia.

The game follows an elderly couple through a dreamlike world of seasonal islands. Across the journey, the player travels by boat, restores lost memory fragments, and slowly rebuilds a shared life through symbolic interactions and quiet exploration.

---

## Story

The player enters the Islands of Memory after falling asleep beside their beloved. Each island represents a season, a moment, and a fragile part of a relationship that must be remembered again.

The final restored memory is presented as a letter:

> My dearest,  
> Sometimes, you still forget me.  
> But that is all right.  
> I will stay beside you, until you remember me again, and again, and again.  
> So we will live through every season once more.

---

## Gameplay Overview

Players explore four seasonal islands and collect four memory puzzle pieces:

1. **Winter Memory**: discover and collect the first puzzle piece.
2. **Autumn Memory**: collect leaves and open the glowing chest to reveal the second memory.
3. **Summer Memory**: complete a timing-based fishing minigame.
4. **Final Memory**: reach the final island and restore the last fragment.

When all four pieces are restored, the ending sequence begins and the final letter scrolls on screen.

---

## Memory Pieces

<p align="center">
  <img src="Assets/UI/Puzzles/WinterPuzzle.png" width="170" alt="Winter puzzle"/>
  <img src="Assets/UI/Puzzles/AutumnPuzzle.png" width="170" alt="Autumn puzzle"/>
  <img src="Assets/UI/Puzzles/SummerPuzzle.png" width="170" alt="Summer puzzle"/>
  <img src="Assets/UI/Puzzles/LastPuzzle.png" width="170" alt="Final puzzle"/>
</p>

Each recovered piece updates the puzzle HUD and triggers a short reward animation.

---

## Controls

| Input | Action |
| --- | --- |
| WASD | Move |
| Space | Jump / fishing timing |
| E | Interact, board, disembark, collect |
| A / D | Row left and right while in the boat |
| Esc | Pause |

<p align="center">
  <img src="Assets/UI/Image/2.png" width="760" alt="Controls screen"/>
</p>

---

## Implemented Features

### Exploration and Boat Travel

- Third-person player movement.
- Boat boarding and disembarking.
- Alternating oar input using **A** and **D**.
- Dock zones for controlled island arrival.
- Camera mode switching between walking and boat travel.

### Puzzle and Memory System

- Four indexed memory puzzle pieces.
- Puzzle HUD with locked and unlocked states.
- Trigger-based pickup interactions.
- Glow feedback for important interactable objects.
- Reward animation when a memory piece is restored.

### Autumn Leaf Game

<p align="center">
  <img src="Assets/UI/clct.png" width="760" alt="Autumn leaf tutorial"/>
</p>

- Leaf collection challenge.
- Instruction UI and completion feedback.
- Glowing chest interaction after the leaf activity.
- Puzzle reward appears after the Autumn memory is restored.

### Summer Fishing Minigame

<p align="center">
  <img src="Assets/UI/fishingmemory.png" width="760" alt="Fishing tutorial"/>
</p>

- First-person fishing view.
- Tutorial screen before the minigame starts.
- Timing bar with a moving marker.
- Randomized success zone after each attempt.
- Five required catches, with speed increasing after success.
- Fish images appear one by one as catches are completed.
- Dedicated fishing background music and sound effects.

<p align="center">
  <img src="Assets/UI/Fish/fish1.png" width="110" alt="Fish 1"/>
  <img src="Assets/UI/Fish/fish2.png" width="110" alt="Fish 2"/>
  <img src="Assets/UI/Fish/fish3.png" width="110" alt="Fish 3"/>
  <img src="Assets/UI/Fish/fish4.png" width="110" alt="Fish 4"/>
  <img src="Assets/UI/Fish/fish5.png" width="110" alt="Fish 5"/>
</p>

### Menus and Interface

- Start menu with logo and custom button art.
- Pause menu with resume, controls, volume slider, and quit.
- On-screen pause icon during gameplay.
- Controls panel using a visual tutorial image.
- Final ending panel with scrolling letter text.

---

## Visual Direction

<p align="center">
  <img src="Assets/UI/Image/1.png" width="760" alt="Story image"/>
</p>

The project uses a low-poly visual style, seasonal colors, soft UI overlays, and symbolic environmental design. The goal is to create a calm, reflective game about love, memory, and returning to important moments together.

---

## Built With

- Unity
- Universal Render Pipeline
- C#
- TextMeshPro
- Unity UI
- World Space UI

---

## Project Status

The current build includes the complete core gameplay loop:

- Start and pause menus.
- Four island memory progression.
- Boat travel and dock interactions.
- Winter, Autumn, Summer, and Final memory pieces.
- Autumn leaf activity.
- Summer fishing minigame.
- Puzzle reward animations.
- Final restored-memory ending.

---

## Author

Chenxu Yang  
University of East Anglia  
Student ID: 100512107
