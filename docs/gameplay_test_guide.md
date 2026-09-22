# HOUSEFLOW Gameplay Test Guide

## 1. Test Environment

- **Unity Version:** UNKNOWN - VERIFY IN UNITY
- **Expected Project State:** Phase 1-3 gameplay architecture implemented (Water, Mechanical, Thermal, Airflow, Pneumatics).
- **Required Scenes:** At least one Gameplay scene that holds the `LevelFlowController`, `LevelLoader`, `ObjectiveSystem`, and `GameInput` singletons.
- **Temporary Test Scene:** Tests can be performed in a dedicated temporary test scene (e.g., `Assets/Scenes/Testing/GameplaySystemsTest.unity`), provided it has the necessary manager singletons and a configured `LevelData` assigned to the `LevelFlowController`.
- **LevelData Configuration:** Almost all runtime objects rely on the physics and objectives defined in a `LevelData` asset. You must assign a valid `LevelData` asset to test these behaviors.

## 2. Test Result Format

When executing tests, record results using the following status tags:
- **PASS**: The system behaves exactly as expected.
- **FAIL**: The system throws an error or fails to behave as expected.
- **BLOCKED**: Cannot test due to prerequisite failures.
- **NOT IMPLEMENTED**: The feature does not currently exist in the codebase.
- **NOT TESTABLE**: Impossible to verify with current tools/setup.

**Format Example:**
### ID-000 - Test Name
**Preconditions:** Setup required.
**Steps:** 
1. Step one.
2. Step two.
**Expected:** What should happen.
**Actual:** What did happen.
**Status:** [TAG]
**Notes:** Any bugs or observations.

---

## 3. Core Fluid Tests

### WATER-001 - Water Source Emits Particles
**Preconditions:** Test layout contains `WaterSource` set to *Starts Emitting = true*. `LevelData` assigned.
**Steps:**
1. Enter Play Mode.
2. Observe the `WaterSource`.
**Expected:** Particles spawn continuously at the configured emission rate, direction, and force.
**Status:** NOT TESTED

### WATER-002 - Fluid Target Detection & Delivery
**Preconditions:** Particle falls into a `FluidTarget` collider (trigger) matching the particle's fluid type. Target is not at capacity.
**Steps:**
1. Allow water particle to enter the `FluidTarget`.
**Expected:** Particle is marked delivered, current count increases, `OnParticleDelivered` fires. The particle does not trigger duplicate counting on subsequent frames (prevented by internal Instance ID hashset).
**Status:** NOT TESTED

### WATER-003 - Fluid Target Rejection
**Preconditions:** Particle falls into a `FluidTarget` expecting `Steam`, but particle is `Water`.
**Steps:**
1. Allow water particle to enter the target.
**Expected:** Target ignores particle. Count does not increase.
**Status:** NOT TESTED

---

## 4. Mechanical Gameplay Tests

### MECH-001 - Valve Toggling
**Preconditions:** `Valve` connected to a `WaterSource` via the `controlledSources` array.
**Steps:**
1. Enter Play Mode.
2. Tap the `Valve` object using the mouse/touch.
**Expected:** Visual flap/wheel rotates to target angle. `WaterSource` toggles on/off.
**Status:** NOT TESTED

### MECH-002 - Player Raycast Respects UI
**Preconditions:** A UI button overlays an interactable `Valve` in world space.
**Steps:**
1. Tap the UI button.
**Expected:** The UI button clicks, but the `Valve` behind it does *not* toggle (`LevelFlowController` respects `IsPointerOverGameObject()`).
**Status:** NOT TESTED

---

## 5. Thermal / Heat / Steam Tests

### THERM-001 - HeatSource Activation & Propagation
**Preconditions:** `HeatSource` intersecting a `ThermalBody`.
**Steps:**
1. Ensure `HeatSource` is active.
2. Observe `ThermalBody`'s internal temperature variable.
**Expected:** `ThermalBody` temperature rises according to `HeatSource`'s heating rate minus the default ambient cooling rate.
**Status:** NOT TESTED

### STEAM-001 - Water → Steam Transition
**Preconditions:** Water particle enters a `HeatSource` trigger and surpasses the boiling threshold.
**Steps:**
1. Drop water into heat.
**Expected:** Particle flips to `Steam` fluid type, visual color/sprite changes, gravity scale inverts (rises), and steam lifetime timer starts.
**Status:** NOT TESTED

### STEAM-002 - Steam 6-Second Lifetime Expiration
**Preconditions:** `LevelPhysicsConfig.steamLifetimeSec` is set to exactly `6.0f` (the GDD default).
**Steps:**
1. Generate steam.
2. Allow steam to float freely for > 6 seconds without hitting a cooling source.
**Expected:** Precisely at 6 seconds, the steam particle disappears and gracefully returns to the `ObjectPoolManager`.
**Status:** NOT TESTED

### STEAM-003 - Steam Condensation & Lifetime Cancellation
**Preconditions:** Steam generated. `CoolingSource` positioned above.
**Steps:**
1. Steam enters `CoolingSource` before 6 seconds elapse.
2. Steam cools below boiling threshold.
**Expected:** Steam transitions back to water. The 6-second expiration timer is completely cancelled, preventing the water particle from suddenly despawning.
**Status:** NOT TESTED

---

## 6. Airflow Tests

### AIR-001 - Open-Room Airflow Direction & Force
**Preconditions:** `AirflowSource` active in the room. Rigidbodies (balloons or steam) present.
**Steps:**
1. Enter Play Mode.
2. Place object in the fan's trigger zone.
**Expected:** Unity's `AreaEffector2D` applies force in the exact angle specified by `ForceAngle` and `ForceMagnitude`.
**Status:** NOT TESTED

### AIR-002 - Open-Room Airflow Attenuation/Spread
**Preconditions:** Reviewing GDD implementation against current code.
**Expected:** GDD describes 35-degree conical dissipation.
**Actual:** The current code uses a standard Unity `BoxCollider2D` or `PolygonCollider2D` mapped to an `AreaEffector2D`. Conical attenuation is solely dependent on how the designer shapes the polygon collider. True mathematical 1D distance dissipation is *not* implemented inside the open-room effector.
**Status:** NOT IMPLEMENTED (Conical attenuation mathematical falloff is missing; relies on standard Unity physics).

---

## 7. Duct Tests

### DUCT-001 - Fan → Duct Physics Formula
**Preconditions:** `AirflowSource` linked to a `DuctSegment`. `LevelPhysicsConfig.ductFrictionFactor` = 0.05.
**Steps:**
1. Place balloon near the duct exit nozzle.
2. Activate fan.
**Expected:** 
- `v_in` = `upstreamProvider.ExitVelocity` (NOT ForceMagnitude!).
- `v_out` = `v_in * (1 - mu * (L/D))`. 
- Balloon is propelled by a velocity-targeting PD controller up to exactly `v_out`.
**Status:** NOT TESTED

### DUCT-002 - Duct Chaining (Fan → Duct → Duct)
**Preconditions:** `DuctSegment` B is configured with `upstreamProviderRef` pointing to `DuctSegment` A.
**Steps:**
1. Activate upstream Fan.
**Expected:** Duct A calculates `v_out`. Fires `OnFlowChanged` event. Duct B instantly receives event, reads Duct A's `v_out` as its new `v_in`, recalculates, and correctly applies friction twice down the chain.
**Status:** NOT TESTED

### DUCT-003 - Fan → Duct → Gate → Duct
**Preconditions:** Duct B has `upstreamGate` assigned. Gate is closed.
**Steps:**
1. Activate fan.
**Expected:** Duct A is active. Duct B detects gate is closed, outputs `ExitVelocity` = 0.
2. Open Gate.
**Expected:** Event cascade causes Duct B to instantly receive airflow and resume `ExitVelocity` > 0.
**Status:** NOT TESTED

---

## 8. Pneumatic Gate Tests

### GATE-001 - Player Interaction & Airflow Block
**Preconditions:** Gate placed in a duct chain.
**Steps:**
1. Tap gate to close it.
**Expected:** Visual flap rotates. Downstream ducts drop to 0 velocity.
**Status:** NOT TESTED

### GATE-002 - Physical One-Way Passage
**Preconditions:** Gate `isOpen` = false. `PlatformEffector2D` added to the `flapCollider` gameObject by the developer in the Inspector.
**Steps:**
1. Drop a Rigidbody2D object into the gate from the "allowed" direction.
2. Drop an object from the "blocked" direction.
**Expected:** From the allowed direction, object passes through the collider (one-way physics). From the blocked direction, object bounces off. Airflow remains blocked in both directions.
**Status:** NOT TESTED
**Note:** If the developer forgets to add the `PlatformEffector2D` component to the flap in Unity, the gate will act as a solid two-way wall. 

---

## 9. Inflatables Tests

### BALLOON-001 - Balloon Flotation
**Preconditions:** `Balloon` placed in scene. `AirflowSource` overlaps it.
**Steps:**
1. Fan active.
**Expected:** `Balloon` script detects fan trigger, interpolates `gravityScale` from deflated (positive) to inflated (negative), causing buoyancy.
**Status:** NOT TESTED

### BLADDER-001 - Counterweight Bladder Inflation
**Preconditions:** `CounterweightBladder` linked to a `DuctSegment`. Bladder has a `HingeJoint2D` attaching it to a mechanical arm.
**Steps:**
1. Activate duct airflow.
**Expected:** Bladder subscribes to airflow events. Applies strictly clamped `AddForce` along its local `inflationAxis`, actuating the mechanical arm.
**Status:** NOT TESTED

---

## 10. Steam + Airflow Integration

### INTEG-001 - Airflow affects Steam
**Preconditions:** Fan blowing horizontally. Water boils into steam inside fan zone.
**Steps:**
1. Boil water.
**Expected:** Steam rises (gravity inverted), but is pushed horizontally by `AreaEffector2D`.
**Status:** NOT TESTED

### INTEG-002 - Airflow + Heat Convection
**GDD Requirement:** "ConvectiveThermalZone: Airflow pushes heat".
**Expected:** Airflow extends heat triggers.
**Actual Code:** No `ConvectiveThermalZone` class exists. No wind-heat interaction logic exists in `ThermalBody` or `AirflowSource`.
**Status:** NOT IMPLEMENTED

---

## 11. Level Loading Tests

### LOAD-001 - Level Double-Loading Race Condition
**Preconditions:** `LevelLoader` has `initialLevelData` assigned. `LevelFlowController` has the same `LevelData` assigned.
**Steps:**
1. Enter play mode.
2. Observe Hierarchy and Console.
**Expected:** Only one Layout prefab is spawned.
**Actual Risk:** `LevelLoader.Start()` and `LevelFlowController.Start()` both attempt to load the level if execution order is not strictly defined in Unity. If `LevelLoader` executes first, it loads the level, then `LevelFlowController` loads it *again* (destroying the first instance).
**Status:** UNKNOWN - VERIFY IN UNITY

---

## 12. Objective System Tests

### OBJ-001 - Primary Objective: DeliverFluidToContainer
**Preconditions:** `LevelData` assigned `DeliverFluidToContainer` objective with Target ID = "main_drain", Count = 5.
**Steps:**
1. Deliver 5 particles to target.
**Expected:** `ObjectiveSystem` fires `OnPrimaryObjectiveCompleted`. `LevelFlowController` transitions state to `LevelState.Completed`.
**Status:** NOT TESTED

### OBJ-002 - Optional Objectives / Fail States
**GDD Requirement:** Optional objectives and Failure conditions (e.g., Max Spill).
**Actual Code:** `ObjectiveSystem.cs` ONLY reads `LevelData.PrimaryObjective`. No logic exists to read or evaluate optional objectives or failure objectives.
**Status:** NOT IMPLEMENTED (Configured in data but not currently supported at runtime).

---

## 13. Reset / Deterministic State Test

### RESET-001 - Global State Flush
**Preconditions:** Play level. Spawn water, boil into steam, toggle valves, toggle pneumatic gates.
**Steps:**
1. Call `LevelFlowController.ResetLevel()`.
**Expected:** 
- Particles despawn.
- Valves snap back to default rotations.
- Thermal bodies instantly revert to ambient temp.
- Pneumatic Gates snap to default states.
- Ducts explicitly recalculate their downstream velocity dynamically (via `ForceRecalculate` sweep).
- Steam lifetime timers abort completely.
- Next playthrough is mathematically identical to the first.
**Status:** NOT TESTED

---

## 14. GDD vs Implementation Gap Table

| System | GDD Requirement | Current Code | Testable? | Status | Notes |
|---|---|---|---|---|---|
| Water | Physics, spawning, triggers | `WaterSource`, `FluidParticle` | Yes | IMPLEMENTED | Fully architecture-complete. |
| Heat/Cooling | Temperature, state transitions | `HeatSource`, `ThermalBody` | Yes | IMPLEMENTED | |
| Steam | 6s lifetime, buoyancy | `FluidParticle` Update loop | Yes | IMPLEMENTED | Verified in code. |
| Airflow | Open-room pushing | `AirflowSource` | Yes | PARTIAL | Conical mathematical attenuation missing. |
| Ducting | Formula `v_in * (1-u*L/D)` | `DuctSegment` PD Controller | Yes | IMPLEMENTED | Supports chained topologies natively. |
| Pneumatic Gate | One-way flap blocking | `PneumaticGate` | Yes | PARTIAL | Physical one-way requires manual `PlatformEffector2D` Unity setup. |
| Mechanics | Valves, interactables | `Valve`, `PneumaticGate` | Yes | IMPLEMENTED | |
| Convection | Wind spreads heat | N/A | No | NOT IMPLEMENTED | Explicitly deferred. |
| Electricity | Circuits, power logic | N/A | No | NOT IMPLEMENTED | Explicitly deferred. |
| Objectives | Primary fluid delivery | `ObjectiveSystem` | Yes | IMPLEMENTED | |
| Objectives | Optional / Max Spill | N/A | No | NOT IMPLEMENTED | Stubbed in config, missing at runtime. |
| Loading | Level flow & data mapping | `LevelFlowController` | Yes | IMPLEMENTED | Potential race condition in `Start()`. |
| Progression | Map, Level Select, Saves | N/A | No | NOT IMPLEMENTED | |

---

## 15. Recommended Test Order

1.  **Project compile** (Ensure Unity resolves `.csproj` cache for new files).
2.  **Level loading** (Verify no double-spawns via Hierarchy check).
3.  **Core fluid** (Spawning, gravity, pooling).
4.  **Mechanical** (Tap valves to ensure input works).
5.  **Thermal & Steam** (Verify 6.0s lifetime exactly).
6.  **Airflow & Duct** (Build a `Fan -> Duct -> Duct` chain and verify velocity propagates).
7.  **Pneumatic Gate** (Assign `PlatformEffector2D` and verify balloons can pop out but not re-enter).
8.  **Counterweight Bladder** (Check physics stability when pushing arms).
9.  **Integration tests** (Steam + Fan).
10. **Level reset** (Critical: mutate state wildly, then reset and verify clean slate).
11. **Objective system** (Win the level).
12. **Performance / Console** (Check for leak logs).

*Note: Tests 5-11 are BLOCKED if Test 2 (Level Loading) or Test 3 (Fluid) fail to initialize properly.*

---

## 16. Current Test Coverage Summary

- **Can Currently Be Tested:** All core physics, fluid, thermal, steam, airflow, duct routing, level resetting, and primary win conditions. 
- **Cannot Yet Be Tested:** Main menu level selection, global game saving, UI panels (HUD).
- **Partially Implemented:** Open-room airflow (relies on Unity colliders rather than raw math).
- **Missing entirely:** Electricity, Convective Thermal Zones, Max Spill Objectives.

### Critical Bugs To Check First
1. **LevelLoader vs LevelFlowController Start() Race Condition:** Both singletons attempt to load the `LevelData` in Unity's `Start()` loop. This may cause double-instantiation of the layout prefab.
2. **PlatformEffector2D Setup:** If designers forget to add `PlatformEffector2D` to `PneumaticGate` colliders, the gate will fail to act as a one-way flap and will hard-block objects in both directions.

### Recommended Next Implementation Work
1. Unity Script Execution Order configuration to resolve the `Start()` race condition.
2. Electricity Subsystem (World 2).
3. Optional Objectives / Max Spill fail states runtime execution.
4. Level Progression / Save Data structures.
