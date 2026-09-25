# Phase 20 Remediation Report — Fix Player-Facing UI

**Date**: 2026-09-23
**Scope**: Repair/integration pass only. No Phase 21 work. No backend rewrites — `EconomyManager`, `RewardService`, `RewardCalculator`, `ProgressionManager`, `GameSession`, `LevelFlowController`, `ObjectiveSystem`, `ToolInventory`, `SaveManager` were preserved (only additive changes noted below).
**Validation method**: Real Unity Play-Mode sessions driven through the `unity-cli` MCP pipeline. All UI interactions were performed via `EventSystem.RaycastAll` + `ExecuteEvents` pointer-down/up/click on the top raycast hit — i.e. the real `GraphicRaycaster → Button → handler` chain — not `onClick.Invoke()`. Screenshots captured via the Game View render path at 1080×1920.

---

## 1. Bugs Fixed

| # | Defect | Fix |
|---|--------|-----|
| 1 | Canvas `RectTransform.localScale = (0,0,0)` — entire UI invisible | Canvas scale restored; `CanvasScaler` added (`ScaleWithScreenSize`, 1080×1920, match 0.5); `ScreenSpaceCamera` mode with `Main Camera` as render camera |
| 2 | All scene buttons were `Transform`+`Button` only — invisible and non-raycastable | Every affected button rebuilt with `RectTransform` + `Image` (raycastTarget) + `Text` label; LayoutGroups added to menu/world/level/tool containers |
| 3 | `GameplayHUDPanel` inactive at load → `Awake/Start` never ran → HUD/tool tray/reward/failure UI permanently dead | Panel activated in scene; `GameplayHUD` reconciles visibility with `LevelFlowController.CurrentState` in `Start()` (`HandleStateChanged(Idle)` hides it) |
| 4 | Baked runtime clones in `GameScene.unity`: 12 `LevelNodeWidget(Clone)`, 4 `WorldButtonPrefab(Clone)`, 5 duplicate `Layout_level_001` roots, 1 stray `LayoutLevel` prefab instance | All removed from the scene file. `LevelLoader` creates/destroys the layout at runtime; `CampaignMapUI` owns all map nodes |
| 5 | `PopulateLevels`/`RefreshUI` only cleared tracked runtime children → stale nodes persisted | `PopulateLevels` now destroys **all** non-prefab children of the grid before spawning; `RefreshUI` world-button clearing changed to backwards iteration (live `foreach` over `Transform` while `Instantiate` appends children is unsafe) |
| 6 | Enter Play Mode Options disabled domain+scene reload → stale in-memory state leaked between tests | `EditorSettings.enterPlayModeOptionsEnabled = false` (domain+scene reload active) — serialized in `EditorSettings.asset` |
| 7 | Reward overlay showed placeholder values; `lastBundle` stayed null; DoubleCoins never activated | `RewardOverlayUI` subscribed to `RewardService.OnRewardGranted` in `Start`, but the HUD parent could deactivate it before `Start` ran. Now subscribes retry-safely from `Awake`, `Start`, and `HandleLevelStarted` (level start always precedes the grant event) |
| 8 | `CampaignMapUI` subscribed `OnReturnToMapRequested` in `Start` — but `Start` was **cancelled** when the map opened and closed within one synchronous call chain (`OnLevelNodeSelected → StartLevel → CloseMap`), so reward/gameplay MAP returned to a blank screen | Subscription moved to `OnEnable` (runs synchronously inside `SetActive(true)` during `OpenMap`) with a `subscribedToFlow` guard; `OnDestroy` unsubscribes |
| 9 | `OnLevelNodeSelected` closed the map **before** starting the level → `OnMapClosed` fired while state was still `Idle` → `MainMenuUI` re-opened the menu during gameplay | Order reversed: `StartLevel` first, then `CloseMap` |
| 10 | HUD title/objective elements positioned at x=545 (half offscreen on the 1080-wide canvas) | Re-centered: `LevelTitle`, `PrimaryObjDesc`, `OptionalObjDesc`, `ObjProgressText`, `ObjProgressBar` |
| 11 | `LevelFlowController.Start` auto-started a level whenever `levelData` was assigned | Gated behind `autoStartLevel` (default false); explicit `pendingPuzzleLevel` still honored; otherwise enters `Idle` |
| 12 | `GameSession.Awake` called `DontDestroyOnLoad` unconditionally (editor-mode side effects) | Guarded by `Application.isPlaying` |
| 13 | Fresh profile had no tool grant path | Data-driven `starterTools` array on `ToolInventory` (FixItTool ×1 in scene), granted only when `SaveManager.HasSavedProfile()` is false — cannot touch existing saves |
| 14 | Fluid particles spawned at scene root **before** `SetParent`, and could stay there permanently if `Pooled Objects` was missing (parenting silently skipped) → loose particles duplicated in and outside the pool | `ObjectPoolManager` now lazily recreates `Pooled Objects`/`Particles` via `EnsureContainers()`, and `Instantiate` receives the parent directly — a particle can never exist outside the pool |
| 15 | Unlimited particle emission → unbounded pool growth → lag/GC pressure | `LevelPhysicsConfig.maxActiveParticles` (serialized per level, 0 = uncapped). `WaterSource` tracks emitted particles and, once at the cap, recycles the oldest live particle back to the pool before each spawn (steal-oldest, no allocation). `LevelData1`/`LevelData2` set to **150** |
| 16 | No cleanup for particles that escaped the level (fell below the floor forever) | New `HouseFlow.Level.DeadZone` component: a `BoxCollider2D` trigger at y≈-6.5 (30×1.5) inside `LayoutLevel.prefab`. Pooled objects crossing it are returned to the pool; non-pooled strays are destroyed |

---

## 2. Scene Hierarchy (After)

```
Canvas (scale 1,1,1; CanvasScaler 1080x1920; ScreenSpaceCamera)
├─ MainMenuPanel                    [active]  MainMenuUI
│  ├─ PlayBtn / MapBtn / ShopBtn / HouseBtn / CosmeticsBtn / SettingsBtn
│  │     (RectTransform + Image + Text label each; VerticalLayoutGroup)
│  └─ ComingSoonModal               [inactive]
├─ CampaignMapPanel                 [inactive] CampaignMapUI
│  ├─ WorldMapPanel                 [active]
│  │  └─ WorldButtonContainer       (GridLayoutGroup; empty at design time)
│  ├─ LevelSelectPanel              [inactive]
│  │  ├─ WorldTitleText / BackBtn
│  │  └─ LevelGridContainer         (GridLayoutGroup; empty at design time)
│  ├─ WorldButtonPrefab (template)  [inactive]
│  └─ CloseBtn
└─ GameplayHUDPanel                 [active]  GameplayHUD (self-hides on Idle)
   ├─ LevelTitle / PrimaryObjDesc / OptionalObjDesc / ObjProgressText / ObjProgressBar
   ├─ RestartBtn / MapBtn / CoinsText / GemsText / AcclaimText + labels
   ├─ ToolTrayPanel (ToolTrayUI; 4 slots, HorizontalLayoutGroup)
   ├─ LevelCompletedPanel           [active] RewardOverlayUI (hides on level start/idle)
   └─ LevelFailedPanel              [inactive]
```

No `(Clone)` objects, no baked `Layout_level_*` objects, no stray `LayoutLevel` root remain in the serialized scene.

## 3. Canvas Configuration

- Render mode: `ScreenSpaceCamera`, camera = `Main Camera`
- `CanvasScaler`: `ScaleWithScreenSize`, reference `1080×1920`, match `0.5`
- Verified at runtime: canvas rect `1080×1920`, `scaleFactor ≈ 1.0`, all controls onscreen in portrait
- Note: the serialized `m_LocalScale` may still read `(0,0,0)` on disk — it is a **driven property** for `ScreenSpaceCamera` canvases; Unity recomputes it every render. The audit's failure mode was the missing `CanvasScaler`/layout, not the serialized scale itself. Runtime behavior is verified, not assumed.

## 4. Play Mode Reload Configuration

`EditorSettings.asset`: `m_EnterPlayModeOptionsEnabled: 0` — Enter Play Mode now performs a full domain + scene reload. Rationale: reliable development testing; eliminates stale `pendingPuzzleLevel`/singleton leakage observed in the audit (Session A auto-start artifact).

## 5. Tool Inventory Decision

`ToolInventory` gains a serialized `starterTools` array (data-driven), granted **only** when `SaveManager.HasSavedProfile()` is false. Scene config grants `FixItTool ×1`. Existing profiles are untouched and may legitimately show quantity 0 — tool buttons remain real buttons that safely no-op at zero count (verified via real raycast: handler invoked, no state change). ToolTrayUI remains presentation-only.

## 6. LoadingScene Decision

`LoadingScene` is **unused** by the Phase 20 flow — no caller references it; `LevelLoader` does a direct `Instantiate`, not a scene transition. `LoadingBarFill` remains unassigned. Decision: **deferred** — component left in place, documented as dormant. Not a defect in the active path.

## 7. Build Result

- `dotnet build Assembly-CSharp.csproj`: **0 errors** (warnings only: Unity 6 `FindFirstObjectByType`/`GetInstanceID`/`FindObjectsSortMode` deprecations, unused events/fields — pre-existing, non-blocking).
- Unity compile after all script edits: clean; Play Mode entered without compile errors.

## 8. Unity Console Result

No game exceptions across all test sessions. Only noise: MCP-plugin path-with-spaces warning and Unity 6 deprecation warnings (listed above). Gameplay log lines were clean throughout.

## 9. Runtime Verification (real `GraphicRaycaster → Button → handler` clicks)

Fresh play session, clean launch: `state=Idle`, menu visible, map+HUD hidden, 0 level roots, canvas `1080×1920`.

| Step | Method | Result |
|------|--------|--------|
| Main Menu PLAY | raycast click @ PlayBtn | Map opens, 2 world buttons with correct labels, menu hidden |
| World 1 select | raycast click @ world button | Level panel opens, **exactly 2 nodes** (Stage 1: 1★, Stage 2), no stale/duplicate nodes |
| Level 1 select | raycast click @ node | `Playing`, map+menu hidden, **HUD active**, exactly 1 `LevelRoot` |
| HUD Restart | raycast click | Level resets, still `Playing`, 1 root, HUD stays active |
| HUD Map | raycast click | `Idle`, root destroyed, map opens, HUD hidden |
| Tool slot click | raycast click @ Slot_FixItTool | Handler invoked; qty 0 → safe no-op (correct) |
| Valve open | `Valve.Interact()` (same call as tap raycast) | Water flows; after ~6 s: objective 1.00 → `Completed` |
| Reward overlay | — | Panel visible, **real bundle** (+15 replay coins / +0 / +0), "2X COINS (AD)" active |
| Double Coins | raycast click | Coins **230→245**, mock ad Completed, button hidden + non-interactable (single-claim verified) |
| Reward Restart | raycast click | `Playing`, fresh layout, overlay hidden |
| Reward Map | raycast click | `Idle`, root unloaded, **map reopens on world view** (previously dead — fixed via `OnEnable` subscription) |
| Reopened map | raycast click @ World 1 → node | Fully interactable; 2 fresh nodes; level starts again |

Deferred-`Destroy` note: stale grid children flush at the next frame boundary (verified: 4→2 after one frame). This matches Unity semantics and self-corrects on real hardware every frame.

## 9b. Particle Lifecycle Verification (runtime)

Measured in a live `Playing` session (level `level_001`, valve open, emission at 5/sec):

| Check | Result |
|-------|--------|
| Particle parenting | `Particles children=26`, `total=26`, `underPool=26`, `looseAtRoot=0` — every particle lives under `Pooled Objects/Particles` |
| Dead zone present | `DeadZone` trigger found at y=-6.5 inside the level layout |
| Dead zone recycle | Rigidbody2D teleported into the trigger → `activeSelf=false`, parent=`Pooled Objects/Particles`, `rb.simulated=false`, `looseAtRoot=0` |
| Particle cap | `maxActiveParticles` injected as 10 in the live config → after 300 frames: `active=10` (exactly at cap), `inactive=8`, `outsidePool=0` |
| Asset values | `LevelData1.asset` + `LevelData2.asset` both carry `maxActiveParticles: 150` |
| Scene cleanliness | `GameScene` contains **zero** baked `FluidParticle` instances; scene not dirty after exit |

## 10. Editor-Only Test Limitations

- The unfocused editor's play loop does not advance continuously; `EditorApplication.Step()` was used to pump frames (300–400 steps ≈ 6 s game time). Not a game defect.
- `capture_game_view` renders at its requested size; default 1280×720 flips a `ScreenSpaceCamera` canvas to landscape for that render. All captures used 1080×1920.
- `InputSystemUIInputModule` did not translate injected `simulate_pointer` clicks in this environment; `ExecuteEvents`-based clicks were used instead — they still exercise the real `GraphicRaycaster` hit test and `IPointerClickHandler` dispatch on the actual `Button`.

## 11. Test Helpers

`AgentScripts/` (project root, outside `Assets/` — never compiled into the game): `UIClick.cs` (raycast+ExecuteEvents click), `SetPortraitGameView.cs`, `StepN.cs`, `Phase20UIFix.cs` (the one-shot scene-rebuild script), assorted probes. These are dev/test harnesses; safe to keep or delete.

## 12. Remaining Issues / Notes

- `LevelData2` is a near-duplicate of `LevelData1` (same tier/objective params) — flagged, not a blocker.
- World 2 has zero levels (`requiredCompletedLevelsToUnlock: 1`); unlock flow unverifiable until content exists.
- `LevelFailedPanel` buttons are structurally valid real buttons and wired via `GameplayHUD.WireButtons()`, but the failure path was not exercised at runtime (no easy fail trigger in test level).
- Reward overlay `MapBtn` carries both `continueButton` and `completionMapButton` bindings — two listeners that both resolve to `ReturnToMap()`; harmless redundancy.
- Menu SHOP/HOUSE/COSMETICS/SETTINGS buttons route to the "coming soon" modal by design (Phase 21+ scope).
- `profile.json` persists between sessions; first-clear vs replay rewards depend on accumulated state (by design).
