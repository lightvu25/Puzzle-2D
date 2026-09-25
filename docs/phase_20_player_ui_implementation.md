# Phase 20: Player-Facing Core UI Implementation

## Overview
This document summarizes the architectural design and implementation details for the Phase 20 Core UI layer in HOUSEFLOW. The goal was to provide an interactive presentation layer without duplicating any persistence, currency management, or reward calculation logic that belongs in the existing backend services.

## Canvas & UI Hierarchy
A standard top-level `Canvas` component in the `GameScene` serves as the root container. The following major panels reside directly underneath, managing state via `LevelFlowController` and specific service singletons.

1. **MainMenuPanel (`MainMenuUI.cs`)**
   - Serves as the entry point when `LevelFlowController.CurrentState == Idle`.
   - Connects to Shop, House Progression, Cosmetics, and Settings.
   - For currently unimplemented or future phases (Shop/House/Cosmetics), triggers a "Coming Soon" modal to prevent locking the game loop while fulfilling the design requirements.
   - **Play Flow**: Resolves the highest unlocked world and level via `ProgressionManager`, or resumes a cached `GameSession.pendingPuzzleLevel`, bypassing hardcoded rules.

2. **CampaignMapPanel (`CampaignMapUI.cs`)**
   - Interacts with `ProgressionManager` and its associated `CampaignDatabase` ScriptableObject.
   - Features dynamic instantiation of `LevelNodeWidget` prefabs per level.
   - Provides a structured World selection interface leading to specific Level grids.
   - Does NOT store progression state itself.

3. **GameplayHUDPanel (`GameplayHUD.cs`)**
   - Automatically synchronizes with `EconomyManager` events (`OnCoinsChanged`, `OnGemsChanged`, `OnShowcaseAcclaimChanged`) to track currency dynamically.
   - Listens to `ObjectiveSystem` events to update Objective description formats and real-time fluid delivery progress bars.

4. **RewardOverlayUI (`RewardOverlayUI.cs`)**
   - Located as a child panel inside GameplayHUDPanel.
   - Operates strictly on presentation layer. Subscribes to `RewardService.OnRewardGranted`.
   - Never calculates its own rewards. Merely prints the data encapsulated within `RewardBundle`.
   - Incorporates `AdRewardService` to execute the Level Clear 2x Coin Doubler functionality without bypassing the existing economy flow.

5. **ToolTrayUI (`ToolTrayUI.cs`)**
   - Located as a child panel inside GameplayHUDPanel.
   - Enforces the strict consumption flow: `CanActivate(root)` → `Activate()` → `ToolInventory.ConsumeTool()` upon success callback.
   - Does not decrement tools instantly on UI click, ensuring protection against wasted inventory usage.
   - Temporarily excludes `RewindTool` until its Box2D physics history API (`IPhysicsHistoryProvider`) matures into a deterministic state.

## Integration & Wiring
Due to Unity editor YAML corruption risks, standard UI wiring was conducted using `Object.FindAnyObjectByType` paired with C# `SerializedObject` injection directly into the Scene, rather than manual prefab overrides.
- Required Singletons (`EconomyManager`, `RewardService`, `ProgressionManager`, `ToolInventory`, `CosmeticInventory`, `ShopManager`, `AdRewardService`, `IAPManager`) are mounted securely to the `PERSISTENT DATA` GameObject.
- Data Catalogs (Tools, Shop Items, Features) are initialized dynamically in `Assets/Data/` via `AssetDatabase`.

## Security & State Integrity
- **No Negative Balances:** Guaranteed by the underlying `EconomyManager`.
- **No UI-Driven Writes:** The UI never calls `Coins += x`, maintaining the integrity of `AddCoins` services.
- **Persistent Data Protection:** Confirmed exactly one runtime instance for all manager classes, avoiding duplicative object generation during scene transitions.
