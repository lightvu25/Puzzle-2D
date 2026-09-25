# HOUSEFLOW — Devin Handoff Audit

**Date**: 2026-09-23
**Auditor**: Devin (independent verification of the Antigravity Phase 20 report)
**Unity**: 6000.4.0f1 (Unity 6.4), URP 17.4.0, Input System 1.19.0
**Method**: filesystem inspection + live Unity Editor via `unity-cli` MCP (scene hierarchy, serialized fields, console) + `dotnet build` + **two real Play-Mode sessions** driven programmatically.

> **Verdict up front:** The Phase 20 *code and serialized wiring* exist and the *backend/services* genuinely work end-to-end (verified in Play Mode with real logs). However, the previous agent's claim that the UI was "runtime-tested end-to-end" does **not** hold up: **the entire Canvas is serialized with `localScale = (0,0,0)`, so no UI renders at all**, and every Button was built without `RectTransform`/`Graphic`, making them invisible and unclickable. Additionally `GameplayHUDPanel` ships inactive and can never activate itself, and dozens of runtime-instantiated objects are baked into the scene file.

---

## 1. Project State

- Path `D:\UnityProject\Puzzle 2D` (contains a space — generates a permanent MCP-plugin console error, cosmetic only).
- This is a **repurposed codebase** ("Project Echoes" roguelike): `GameManager`, `GameSession`, `RunManager`, `PlayerStats`, `MindPlayerMovement`, `Dialogue*`, `BaseLevelGenerator` etc. are legacy. HOUSEFLOW systems live under `Assets/Scripts/HouseFlow/`.
- Build Settings contains **only** `Assets/Scenes/GameScene.unity`. `LoadingScene.unity` exists but is not in the build list.
- `ProjectSettings/EditorSettings.asset`: `m_EnterPlayModeOptionsEnabled: 1`, `m_EnterPlayModeOptions: 0` — **Enter Play Mode disables both domain AND scene reload**. This caused observable stale-state leakage (see §11). *(Remediation: set to `0` — full domain+scene reload now active.)*

## 2. Phase 1–19 Status

Source-level review confirms the previous audit (`docs/phase_1_19_final_audit.md`) is accurate:

- Implemented and wired: fluid system (`WaterSource`/`FluidParticle`/`FluidTarget`), `Valve`, `ObjectiveSystem`/`DeliverFluidObjective`, `LevelFlowController`/`LevelLoader`/`LevelRoot`, `GameInput` (touch + UI-raycast blocking), `SaveManager`/`ProfileData` (persists to `profile.json`, not `profile.dat` as the old doc says), `ObjectPoolManager`.
- Implemented as backend services, present on `PERSISTENT DATA` in scene: `EconomyManager`, `RewardService`, `ToolInventory`, `CosmeticInventory`, `HouseProgressionManager`, `DailyRewardService`, `VisitorTipService`, `AdRewardService` + `MockRewardedAdService`, `IAPManager` + `MockIAPService`, `ShopManager`, `ProgressionManager` (+ `CampaignDatabase_Main`).
- Ads and IAP are confirmed **mock-only** (`MockRewardedAdService`, `MockIAPService`). No production SDK.
- `RewindTool` correctly absent from scene (`RewindTool.cs` exists; 0 instances — verified). `MaxSpillObjective` absent (deferred per spec). `IPhysicsHistoryProvider` is only an interface.
- Systems still not player-accessible regardless of UI: no thermal/airflow/electricity/pneumatic level content (prefabs exist but no level uses them).

## 3. Phase 20 Status

All six UI scripts exist and are serialized-wired correctly in `GameScene.unity`:

- `MainMenuUI` on `/Canvas/MainMenuPanel` — all 6 buttons + modal refs valid.
- `CampaignMapUI` on `/Canvas/CampaignMapPanel` — containers, prefab refs (`LevelNodeWidget.prefab`, `WorldButtonPrefab` template), buttons valid.
- `GameplayHUD` on `/Canvas/GameplayHUDPanel` — all text/slider/button refs valid.
- `ToolTrayUI` on `GameplayHUDPanel/ToolTrayPanel` — 4 slots (FixItTool, VacuumPump, Magnet, BlueprintHint), matching `ToolType` enum names.
- `RewardOverlayUI` on `GameplayHUDPanel/LevelCompletedPanel` — refs valid.
- `LevelFailedPanel` — refs wired through `GameplayHUD`.

**But the UI is not functional for a real player** (see §5/§6/§11). There is **no `ModalsPanel`** — a `ComingSoonModal` lives inside `MainMenuPanel` instead.

## 4. Filesystem / Code Findings

- `Assets/Scripts/HouseFlow/`: 60+ scripts across Fluid, Mechanical, Thermal, Electricity, Objective, Level, Progression, Economy, Rewards, Tools, Monetization, Shop, Meta, House, Cosmetics, UI, Editor (`LevelDataValidator`).
- `Assets/Data/`: `CampaignDatabase_Main` (World 1 = 2 levels; World 2 = 0 levels, requires 1 completed), `LevelData1`/`LevelData2` (both `world 1, tier 1`, same layout, same objective: deliver 5 water to `main_drain`), 4 `ToolDefinition`s, 4 `ShopItem`s, `HouseProgressionDatabase` + 2 features.
- `Assets/Prefabs/`: `LayoutLevel.prefab` (WaterSource/Valve/FluidTarget/Floor, `targetId: main_drain`), 5 mechanical prefabs, 5 particle prefabs, `UI/LevelNodeWidget.prefab` (proper UI structure).
- Code quality observations (not blocking): UI scripts correctly avoid business logic; consumption-after-activation pattern in `ToolTrayUI` is correct; `AdRewardService` in-flight request guard is correct; `RewardOverlayUI` duplicate-claim guard is correct.
- `LevelData2` is a near-duplicate of `LevelData1` (same tier `1`, same objective params) — flagged, not necessarily a bug.

## 5. Unity Scene Findings

GameScene has 15 roots: `Main Camera`, `CinemachineCamera`, `PERSISTENT DATA` (18 service components — complete), `CORE SYSTEM` (Music/Sound), `LOCAL GAME MANAGER` (GameManager, LevelFlowController, LevelLoader, ObjectiveSystem, GameInput, Tools child with 4 tool components), `ObjectPoolManager`, `Canvas`, `EventSystem` (InputSystemUIInputModule), `UIManager`, `CutsceneManager`, `BlurVolume`, `Global Light 2D`, `Global Volume`, `LoadingScene`, `LayoutLevel`.

**Defects found in the serialized scene:**

1. **Canvas scale = (0,0,0)** — `RectTransform m_LocalScale: {x:0,y:0,z:0}` on the Canvas GameObject (fileID 1293010735). Nothing under the Canvas renders. Verified visually: Play-Mode Game View shows the puzzle scene with zero UI.
2. **Buttons are plain `Transform` + `Button`** — no `RectTransform`, no `Image`/`CanvasRenderer`, no `Text` children (PlayBtn, MapBtn, ShopBtn, HouseBtn, CosmeticsBtn, SettingsBtn, WorldButtonPrefab, Restart/Map/Next/Retry/DoubleCoins buttons). With no `Graphic`, `GraphicRaycaster` cannot hit them → **invisible and unclickable by real input**. Programmatic `onClick.Invoke()` does work.
3. **Runtime-spawned objects baked into the scene file** (16 `(Clone)` objects): 4 `WorldButtonPrefab(Clone)` under `WorldButtonContainer`, 12 `LevelNodeWidget(Clone)` under `LevelGridContainer` — including baked-in `LockOverlay` active states. Also **5 duplicate `Layout_level_001` LevelRoot instances** under `LOCAL GAME MANAGER` (plain objects, not prefab instances) plus a separate root `LayoutLevel` prefab instance → **6 unmanaged, always-active level layouts** in the scene.
4. `GameplayHUDPanel` serialized `m_IsActive: 0`; `CampaignMapPanel` `m_IsActive: 0`; `LevelSelectPanel` 0; `WorldMapPanel` 1; `MainMenuPanel` 1. (Correct initial states — but see HUD defect below.)

## 6. Canvas / UI Hierarchy

```
Canvas (scale 0,0,0 — DEFECT)              [active]
├─ MainMenuPanel                           [active]  MainMenuUI
│  ├─ PlayBtn / MapBtn / ShopBtn / HouseBtn / CosmeticsBtn / SettingsBtn   (Transform+Button only, no graphics)
│  └─ ComingSoonModal                      [inactive] (Text + CloseBtn)
├─ CampaignMapPanel                        [inactive] CampaignMapUI
│  ├─ WorldMapPanel                        [active]
│  │  └─ WorldButtonContainer              → 4 baked WorldButtonPrefab(Clone)
│  ├─ LevelSelectPanel                     [inactive]
│  │  ├─ WorldTitleText / BackBtn
│  │  └─ LevelGridContainer                → 12 baked LevelNodeWidget(Clone)
│  ├─ WorldButtonPrefab (template)         [inactive]
│  └─ CloseBtn
└─ GameplayHUDPanel                        [inactive] GameplayHUD   ← never self-activates
   ├─ LevelTitle / PrimaryObjDesc / OptionalObjDesc / ObjProgressText / ObjProgressBar(Slider)
   ├─ RestartBtn / MapBtn / CoinsText / GemsText / AcclaimText
   ├─ ToolTrayPanel (ToolTrayUI, 4 slots with Qty+Icon)
   ├─ LevelCompletedPanel                  [inactive] RewardOverlayUI (+DoubleCoinsBtn inactive)
   └─ LevelFailedPanel                     [inactive]
```

## 7. ScriptableObject / Prefab Findings

- All required SOs exist and resolve: `CampaignDatabase_Main`, `LevelData1/2` (layoutPrefab → `LayoutLevel.prefab`), `Tool_*` ×4, `ShopItem_*` ×4, `ShopDatabase_Main`, `HouseProgressionDatabase_Main`, `House_Kitchen`, `House_Porch`.
- `LevelFlowController.levelData` → LevelData1, `levelLoader`/`objectiveSystem` → same GameObject. `ProgressionManager.campaignDatabase` → CampaignDatabase_Main. All valid.
- `LevelNodeWidget.prefab` has proper `RectTransform`/`Text`/`Image`/`Button` structure — the *prefab* is fine; only ad-hoc scene objects are broken.
- No missing scripts found on inspected objects; no null required references on the UI controllers.

## 8. Persistent Services

`PERSISTENT DATA` carries all 18 managers listed in §2. Singleton counts verified at runtime: GameSession=1, LevelFlowController=1, EconomyManager=1. `GameSession` uses `DontDestroyOnLoad` in play. Persistence file: `persistentDataPath/profile.json` — observed real data (coins 155→185, `completedLevelIDs:[level_001]`, `unlockedLevelIDs:[level_002]`, stars [1]).

## 9. Unity Console Findings

- 1 persistent error: MCP plugin warning that project path contains spaces. Not a game defect.
- SignalR/SocketException noise when the IvanMurzak MCP server is unreachable — infrastructure noise, not gameplay.
- **Zero game exceptions** during both Play-Mode sessions (no NullReference, no UI event errors). All gameplay log lines were clean.

## 10. Build Findings

- `dotnet build Assembly-CSharp.csproj`: **0 errors, 21 warnings** — all benign (CS0618 `FindFirstObjectByType`/`GetInstanceID`/`FindObjectsSortMode` deprecations, CS0067 unused events, CS0414 unused field).
- Unity compilation: clean (`compilationFailed=false`).

## 11. Runtime Findings

Two Play-Mode sessions were driven (button `onClick.Invoke()`, `SelectWorld`, `Valve.Interact`, `ReturnToMap`, service calls).

**Session A (inherited stale editor state, no scene reload):** level_001 **auto-started** on play (`StartLevel` from `LevelFlowController.Start` line 86 — `pendingPuzzleLevel` was stale in memory because Enter Play Mode Options disable scene reload). MainMenuPanel and CampaignMapPanel were both active. This is an artifact of the editor setting + leftover in-memory state — **not** reproducible from a clean scene load, but it means the previous agent's "testing" was done against contaminated state.

**Session B (clean reload from disk):**

| Step | Result |
|---|---|
| Launch | `FlowState=Idle`, only MainMenuPanel active — correct state machine, **but Game View renders no UI at all** (Canvas scale 0 + invisible buttons). |
| Play click (programmatic) | MainMenu hides, CampaignMapPanel activates, 2 world buttons spawn (baked clones are correctly destroyed by `RefreshUI`). World buttons have **no labels** (no Text children). World 2 correctly unlocked (1 completed level in profile). |
| `SelectWorld(0)` | **14 level widgets shown — 12 stale baked clones + 2 fresh.** `PopulateLevels` only clears `spawnedLevelWidgets` (empty at Start), so baked clones persist forever with stale lock overlays (baked Stage-2 widgets show lock=True even though level_002 is unlocked). **CONFIRMED BUG.** |
| Click fresh Level-1 node | `Playing`, layout instantiated (`Layout_level_001`), map closes. **GameplayHUD stays inactive** — it's inactive at scene load so `Awake/Start` never run, it never subscribes to `OnLevelStarted`/`OnStateChanged`, and nothing else activates it. **CONFIRMED CRITICAL BUG** — also kills ToolTray, RewardOverlay, LevelCompleted/LevelFailed (their listeners never register). |
| `Valve.Interact()` | Water emits, `FluidTarget main_drain` collects → `ObjectiveSystem` completes → `State=Completed`, `OnLevelCompleted` fires. |
| Rewards | `RewardCalculator` → replay clear (+15 coins, correct — level already in `completedLevelIDs`), `RewardService` → `EconomyManager` → persisted. |
| RewardOverlayUI | **Never shown** (dead by inactive parent). Serialized text still reads baked values `+100/+0/+10`. |
| Mock 2x ad (service level) | `WatchAdForReward(LevelClearCoinDoubler,15)` → Completed → +15 coins, single fulfillment. In-flight guard + `hasClaimedDouble` flag exist. |
| Continue button | Dead — listener was never registered (inactive panel). `flowController.ReturnToMap()` called directly works: `Idle`, map opens. |
| FixIt tool (service level) | `AddTool→CanActivate→Activate→ConsumeTool` verified; qty returns to 0. ToolTrayUI itself is dead (inactive parent) and inventory starts empty (no grant path exists — `toolInventory:[]` in profile). |
| Singleton duplication | 1× each for GameSession/LevelFlowController/EconomyManager. |
| Stale layouts | 5 baked `Layout_level_001` + root `LayoutLevel` prefab instance remain active and unmanaged (LevelLoader only tracks its own instance). Their sources are passive (`startsEmitting=false`) but they render and their colliders can intercept taps. |

## 12. Previous Agent Report vs Actual State

| Feature | Previous Report | Actual Result | Evidence |
|---|---|---|---|
| GameScene launches | PASS | **VERIFIED** | Play mode runs, no game exceptions |
| Main Menu appears | PASS | **FAILED** | Panel active but Canvas scale=(0,0,0); buttons have no graphics → nothing renders |
| Play button works | PASS | **PARTIALLY** | `onClick` logic works programmatically; **unclickable** by real input (no Graphic for raycast) |
| Campaign Map opens | PASS | **PARTIALLY** | `OpenMap()` activates panel correctly; invisible to the player |
| Data-driven lock/unlock | PASS | **VERIFIED** (logic) | World 2 gate honored; level lock overlays on fresh widgets correct — but 12 stale baked widgets show wrong states |
| Level 1 selection works | PASS | **VERIFIED** (logic) | Node click → `StartLevel(level_001)` → Playing |
| Gameplay starts | PASS | **VERIFIED** | Layout instantiates, valve→water→target completes |
| HUD shows objectives/currencies | PASS | **FAILED** | `GameplayHUDPanel` inactive at load → never activates; cannot self-subscribe |
| Economy HUD reacts to events | PASS | **NOT VERIFIED** | Subscription code exists but `Start()` never runs |
| FixIt tool works | PASS | **PARTIALLY** | `FixItTool.Activate` + consume path verified at service level; ToolTray UI dead; inventory empty by default |
| Consume only after success | PASS | **VERIFIED** (code + service test) | `Activate(onComplete→ConsumeTool)` pattern correct |
| Failed Rewind doesn't consume | PASS | **VERIFIED** | 0 RewindTool instances in scene; correctly excluded |
| Level completion works | PASS | **VERIFIED** | State→Completed, progression+stars recorded, rewards granted |
| RewardOverlayUI appears | PASS | **FAILED** | Never activates (dead parent chain); observed inactive post-completion |
| Reward values from RewardService | PASS | **VERIFIED** (service) | +15 replay coins logged and persisted; overlay text shows stale baked `+100` |
| 2x mock ad flow | PASS | **VERIFIED** (service) | `Ad_CoinDoubler` +15 logged, persisted; button itself unreachable |
| Duplicate ad clicks blocked | PASS | **VERIFIED** (code) | `hasClaimedDouble` + `inFlightRequests` guards; UI untestable |
| Continue/Map returns to map | PASS | **PARTIALLY** | `ReturnToMap()` works; the Continue button never had its listener registered |
| Progression updates | PASS | **VERIFIED** | `level_002` unlocked, stars saved in profile.json |
| Persistence updates | PASS | **VERIFIED** | profile.json coins 155→185 across calls |
| Singleton duplication checked | PASS | **VERIFIED** | Exactly 1 instance of each checked manager |
| Zero console exceptions | PASS | **VERIFIED** | Only MCP-plugin noise; no game exceptions |

## 13. Bugs / Risks

1. **CRITICAL — Canvas scale (0,0,0)**: entire UI invisible. Root cause of most UI "failures".
2. **CRITICAL — Buttons without RectTransform/Graphic**: all scene-built buttons are invisible and non-raycastable; UI is non-functional for real input even after fixing scale.
3. **CRITICAL — `GameplayHUDPanel` starts inactive**: its `Awake/Start` never execute → no event subscriptions → HUD, tool tray, reward overlay, fail panel all permanently dead; completing a level strands the player (no on-screen way back).
4. **Baked runtime objects in scene file**: 12 `LevelNodeWidget(Clone)` (stale forever — `PopulateLevels` can't see them), 4 `WorldButtonPrefab(Clone)` (self-healing), 5 `Layout_level_001` LevelRoots + 1 `LayoutLevel` prefab instance (6 unmanaged live layouts; their colliders can eat taps and their renderers stack visually).
5. **Enter Play Mode Options = no domain/scene reload**: stale in-memory state leaks into play (observed auto-start of level_001 via stale `pendingPuzzleLevel`). Makes all manual/editor testing unreliable.
6. Minor: `LevelData2` duplicates LevelData1 (same world/tier/objective); World 2 exists but has zero levels; `LoadingScene` component present but unused (`loadingScreen` ref points at its own GameObject, `LoadingBarFill` is null — will NRE if `LoadPuzzleLevel` is ever called); tool inventory starts empty with no grant path; profile.json persists across sessions (test data contaminates "first clear" behavior).

## 14. Missing Wiring

- Canvas RectTransform scale fix (set to computed SSC values / reset transform).
- Every scene button needs `RectTransform` + `Image` (or a `Graphic` target) + visible label `Text` children.
- `GameplayHUDPanel` must start **active** (it hides itself via `HandleStateChanged(Idle)` anyway) OR an external activator must exist — current design can never wake.
- Delete baked clone objects + 5 duplicate layouts from the scene file.
- `LevelSelectPanel`'s stale-widget issue disappears once baked clones are removed.
- `LoadingScene.LoadingBarFill` unassigned; `LoadingScene`/`UIManager`/`CutsceneManager` roots are present but effectively unused by Phase 20 flow.
- No tool grant path (ToolInventory always empty at fresh install).

## 15. Recommended Next Phase

Before Phase 21, a **Phase 20 remediation pass** is required — this is repair, not new features:

1. Fix Canvas scale; rebuild buttons as proper UI objects (RectTransform + Image + Text).
2. Make `GameplayHUDPanel` active at edit time (its own `HandleStateChanged` already hides it in Idle).
3. Strip all baked `(Clone)` objects and duplicate `Layout_level_001`/`LayoutLevel` instances from `GameScene.unity`.
4. Re-enable domain+scene reload for play mode (or at minimum reload scene) to prevent stale-state testing artifacts.
5. Re-run the runtime checklist end-to-end with **real input simulation** (EventSystem pointer events), not just `onClick.Invoke()`.

## 16. Exact Next Tasks

- [ ] `Canvas` RectTransform: restore scale to (1,1,1) (or let SSC recompute) in `GameScene.unity`.
- [ ] Convert all `Transform`-based button GameObjects (MainMenuPanel ×6, ComingSoon CloseBtn, WorldButtonPrefab, BackBtn, CloseBtn, HUD Restart/Map, ToolTray slots ×4, LevelCompleted Next/Restart/Map/DoubleCoins, LevelFailed Retry/Map) into proper UI buttons with `RectTransform`, `Image`, and `Text` labels.
- [ ] Set `GameplayHUDPanel` `m_IsActive: 1` in the scene.
- [ ] Delete from scene: 12 `LevelNodeWidget(Clone)`, 4 `WorldButtonPrefab(Clone)`, 5 duplicate `Layout_level_001` objects, and evaluate whether the root `LayoutLevel` prefab instance should remain (it is unmanaged by `LevelLoader`).
- [ ] `ProjectSettings/EditorSettings.asset`: enable scene+domain reload for play mode.
- [ ] Optionally: add a `PopulateLevels`/`RefreshUI` sweep that destroys ALL non-prefab children of `levelGridContainer` (defensive parity with `worldButtonContainer` behavior).
- [ ] Decide `LoadingScene` usage: assign `LoadingBarFill` or remove the unused component.
- [ ] Decide tool seeding: starter `ToolInventory` grant (e.g., 1× FixItTool) or accept empty tray.
- [ ] Re-audit in Play Mode using EventSystem `ExecuteEvents` pointer simulation to prove real clickability.

---

## 17. Remediation Status (2026-09-23)

All §16 tasks completed; details in `docs/phase_20_remediation_report.md`. Verified in Play Mode with real `GraphicRaycaster → ExecuteEvents` pointer clicks at 1080×1920 portrait.

| Audit item | Status |
|---|---|
| Canvas scale (0,0,0) | **RESOLVED** — scale restored, CanvasScaler added (1080×1920, match 0.5), UI renders |
| Buttons without RectTransform/Graphic | **RESOLVED** — all player-facing buttons rebuilt with Image + Text; real raycast clicks verified |
| GameplayHUDPanel inactive → dead UI | **RESOLVED** — panel active, self-hides on Idle via `HandleStateChanged`; HUD/tool tray/reward/fail all live |
| Baked clones + duplicate layouts | **RESOLVED** — removed from scene; runtime counts verified (1 LevelRoot while playing, 0 idle; exactly 2 nodes per refresh) |
| Enter Play Mode no reload | **RESOLVED** — `m_EnterPlayModeOptionsEnabled: 0` (domain+scene reload) |
| Stale level nodes on refresh | **RESOLVED** — `PopulateLevels` clears all non-prefab children; world buttons use backwards-iteration clear |
| Reward overlay dead/placeholder | **RESOLVED** — retry-safe `OnRewardGranted` subscription; real bundle displayed; DoubleCoins verified via raycast (230→245, single claim) |
| Return-to-map dead end | **RESOLVED** — `CampaignMapUI` subscribes in `OnEnable` (Start could be cancelled when map closed in the same synchronous chain); reward/gameplay MAP reopens map |
| Menu re-opening during gameplay | **RESOLVED** — `StartLevel` runs before `CloseMap`/`OnMapClosed` |
| Tool inventory empty | **RESOLVED** — data-driven `starterTools` (FixItTool ×1) granted to fresh profiles only |
| LoadingScene null ref | **DEFERRED** — unused by Phase 20 flow; no active path calls it |
| Level auto-start via stale state | **RESOLVED** — `autoStartLevel` gate (default off) + clean reload |

Remaining (non-blocking): `LevelData2` duplicates LevelData1; World 2 has no levels; `LevelFailedPanel` buttons structurally verified but failure path not exercised; SHOP/HOUSE/COSMETICS/SETTINGS intentionally route to "coming soon" modal.
