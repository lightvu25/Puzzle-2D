# HOUSEFLOW: Level Design Guide

Welcome to the HOUSEFLOW puzzle system! This guide explains how to create new puzzle levels from scratch using the tools and components provided in the Unity Editor.

## 🏗️ Core Architecture (Data vs. Layout)

In HOUSEFLOW, a "Level" is split into two pieces:
1. **Layout Prefab:** A Unity Prefab containing the actual physical puzzle (walls, pipes, valves, targets).
2. **Level Data:** A ScriptableObject containing the rules, physics settings, and objectives for that layout.

You do **not** create a new Unity Scene for every puzzle. All puzzles load dynamically as Prefabs within the main puzzle scene.

---

## 🛠️ Step-by-Step: Creating a New Level

### Step 1: Create the Layout Prefab
1. In the Hierarchy, create an Empty GameObject. Name it something like `Layout_Level_01`.
2. Add the **`LevelRoot`** component to this root object.
3. Build your level geometry as children of this root object.
   * *Important:* All walls and floors must have 2D Colliders and be set to the **`HF_LevelGeometry`** layer.
4. Drag your finished hierarchy from the scene into your `Assets/Prefabs/HouseFlow/Levels/` folder to make it a Prefab.
5. Delete it from the scene (we only load it via data!).

### Step 2: Create the Level Data
1. In your Project window, right-click and go to: `Create > HOUSEFLOW > Level > Level Data`.
2. Name the file `Level_01_Data`.
3. Select the file and look at the Inspector:
   * **Level ID:** Give it a unique string (e.g., `level_01`).
   * **Display Name:** e.g., `Level 1: The Basics`.
   * **Layout Prefab:** Drag the Prefab you made in Step 1 here.
   * **Physics Config:** Tweak gravity and fluid mass here if this specific level needs unique physics.
4. **Set the Objective:** 
   * Under Primary Objective, set type to `DeliverFluidToContainer`.
   * Set the **Target ID** (e.g., `basin_main`).
   * Set **Required Particle Count** (e.g., `50`).

### Step 3: Validate Your Level
1. Select your `LevelData` asset.
2. At the bottom of the Inspector, click the **[✓ Validate Level]** button.
3. The tool will scan your Prefab and Data to ensure everything is hooked up correctly. Fix any red `[Error]` messages!

---

## 🧩 Component Reference

Add these components to child objects inside your Layout Prefab to build the puzzle.

### 💧 Water Source (`WaterSource.cs`)
Spawns fluid particles. 
* **Starts Emitting:** Check this if water should flow immediately when the level starts.
* **Emission Rate:** Particles spawned per second.
* **Emission Force / Direction:** How hard the water shoots out, and in what direction.
* **Fluid Particle Prefab:** You **must** assign the blue `FluidParticle` prefab here!

### ⚙️ Valve (`Valve.cs`)
A mechanical wheel the player can tap to turn water sources on/off.
* **Controlled Sources:** Add a new row to this list and drag your `WaterSource` into it. The valve will now control that source.
* **Starts Open:** Does this valve start in the ON position?
* **Rotation Amount:** How many degrees the sprite visually rotates when tapped (e.g., 90).

### 🎯 Fluid Target (`FluidTarget.cs`)
The basin/drain where particles need to go.
* **Target ID:** A unique string (e.g., `basin_main`). **This must match the Target ID in your LevelData objective!**
* **Collider:** Must have a `BoxCollider2D` or `CircleCollider2D` with **Is Trigger** checked, and be on the **`HF_Sensor`** layer.

---

## 🎮 Testing Your Level

**Entering from the Main Menu:**
Since we integrated the puzzle system into the main game architecture, your UI programmer can load any level from the menu by simply setting:
`GameSession.Instance.pendingPuzzleLevel = myLevelData;`
And then loading the Game Scene!

**Developer Debug UI:**
When running the game in the Unity Editor or a Development Build, press **F1** to toggle the Debug UI. It shows:
* Current Level ID and Game State
* Total Active Particles in the scene
* Real-time objective progress (e.g., `35 / 50 Particles Delivered`)
* FPS Counter

## ⚠️ Important Rules to Remember
1. **Physics Layers:** 
   * Water Particles must be on Layer `HF_WaterParticle`.
   * Walls must be on Layer `HF_LevelGeometry`.
   * Targets/Sensors must be on Layer `HF_Sensor`.
   *(If these are missing, go to Edit > Project Settings > Tags and Layers and add them at layers 8, 9, and 10).*
2. **Never instantiate particles via code.** The `WaterSource` automatically pulls from the global `ObjectPoolManager` for performance.
3. **Save your Prefab:** Always remember to apply your changes to the Layout Prefab before hitting Play!
