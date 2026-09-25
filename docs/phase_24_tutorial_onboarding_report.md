# Phase 24 — Tutorial / Onboarding — Implementation Report

**Date:** 2026-09-24
**Unity:** 6000.4.0f1
**Scene:** `Assets/Scenes/GameScene.unity`
**Test env:** Editor Play Mode, 1080×1920 portrait, real EventSystem raycasts + `ExecuteEvents` clicks for UI; real Input System device injection for the gameplay tap (no `Interact()` / `onClick.Invoke` shortcuts)

---

## 1. Phase 24 Scope

First-time-player onboarding over the **real Level 1 gameplay flow**: overlay appears only for uncompleted profiles on Level 1 (`level_001`, "Stage 1: Main Valve"), instructs the player to tap the real valve, advances on the actual `Valve.IsOpen` transition, lets normal water-flow completion run, persists `tutorialCompleted`, then shows the standard `LevelCompletedPanel` reward overlay. No fake mechanics, no duplicate tutorial level, no gameplay-path modification.

## 2. Design

`TutorialUI` is a pure **observer** of `LevelFlowController`:

- Subscribes to `OnLevelStarted`, `OnLevelCompleted`, `OnStateChanged`, `OnReturnToMapRequested`.
- `TryBegin()` gates on `CurrentLevelData == tutorialLevel` (serialized `LevelData1`) **and** `ProfileData.tutorialCompleted == false` — the flag is read fresh from `SaveManager.loadProfile()` at level start, avoiding the stale-profile pattern fixed in Phase 21.
- Step 0: "Tap the valve to open it." + a world-space-anchored highlight ring over `LevelRoot.Valves[0]`; `Update()` polls `Valve.IsOpen` to advance.
- Step 1: "Water is flowing — deliver it to the drain!" — waits for `OnLevelCompleted`.
- `HandleLevelCompleted` → `MarkTutorialCompleted()` (load-fresh-then-save) + hide.
- `HandleStateChanged(Playing)` re-arms step 0 on **restart** (valve resets to closed; `OnLevelStarted` does not refire on reset).
- `HandleStateChanged(Idle)` / `OnReturnToMapRequested` → hide without marking.
- SKIP button → `CompleteAndHide()` — marks completed so it never reappears.

**Input transparency:** every tutorial visual has `raycastTarget=false` except `SkipBtn`, verified by raycast — taps pass through the overlay and reach the valve through the unchanged `GameInput → IsPointerOverUI → Physics2D.Raycast → IInteractable` chain.

## 3. Files Created / Modified

| File | Change |
|---|---|
| `Assets/Scripts/HouseFlow/UI/TutorialUI.cs` | New — observer overlay described above |
| `Assets/Scripts/Data/ProfileData.cs` | Added `public bool tutorialCompleted` |
| `AgentScripts/BuildTutorialUI.cs` | One-shot builder: `TutorialOverlay` hierarchy under Canvas, refs, `tutorialLevel=LevelData1`, scene save |
| `Assets/Scenes/GameScene.unity` | `TutorialOverlay` (+`TutorialUI`) serialized under Canvas |

Overlay hierarchy:

```
Canvas
└── TutorialOverlay              (TutorialUI; root always active, Content self-hides in Start)
    ├── Content                  (full-screen dim, raycastTarget=false)
    │   ├── TitleText            "HOW TO PLAY"
    │   ├── InstructionText
    │   ├── StepText             "1 / 2" | "2 / 2"
    │   └── SkipBtn              (only raycast-blocking element)
    └── Highlight                (pulsing ring positioned over the valve)
```

## 4. Runtime Test Results

| Test | Result |
|---|---|
| A — Real nav path → tutorial appears | PASS — Main Menu → PLAY → Campaign Map → World 1 → Level 1 via real raycast clicks; overlay shows "HOW TO PLAY / Tap the valve to open it. / 1 / 2 / SKIP", highlight over valve |
| B — Real valve tap advances | PASS — synthetic `Pointer` device injected a genuine press at the valve's screen position; `wasPressedThisFrame=True` → `LevelFlowController.Update` → `Physics2D.Raycast` → `IInteractable.Interact()` → `Valve.IsOpen=True`; tutorial advanced to "Water is flowing — deliver it to the drain! / 2 / 2" |
| C — Normal completion + persistence + reward overlay | PASS — water flowed to drain, `state=Completed`, `tutorialCompleted=true` in `profile.json`, coins 2180→2195 (+15), `level_001` recorded; `LevelCompletedPanel` active with "Stage 1: Main Valve Complete!", 1★, +15, CONTINUE/RESTART/MAP/2X COINS buttons all interactable |
| D — Fresh session suppression | PASS — new Play Mode session loaded `tutorialCompleted=true`; real navigation to Level 1 → `state=Playing`, `tutorialContentActive=False` |
| E — Skip | PASS — flag reset → re-entered Level 1 → overlay shown → real SKIP click → `tutorialContentActive=False`, `tutorialCompleted=true`; re-entering Level 1 shows no tutorial |
| F — Restart re-arms | PASS — flag reset → Level 1 → HUD `RestartBtn` click → valve reset closed, tutorial re-armed at step "1 / 2" |
| G — Non-tutorial level | PASS — Level 2 entered with flag=false → no tutorial |
| H — Return-to-map cleanup | PASS — `MapBtn` → `state=Idle`, overlay hidden, flag untouched |

## 5. Console Results

- **After clear + 30 live frames on Level 2:** 0 errors, 0 warnings, 0 logs.
- **Historical noise:** `InvalidOperationException` bursts from a diagnostic `onAfterUpdate` eval hook querying a removed synthetic touchscreen (`PipelineEval_*` assembly) — test tooling, cleared by domain reload; none from game code.

## 6. Diagnostic Cleanup

All temporary instrumentation removed/reverted: injected `PlayerLoop` subsystems removed (EarlyUpdate back to 34), synthetic devices (`myTouch*`, `myPtr4`) removed — device list back to `Keyboard, Mouse`; `InputSystem.settings.updateMode=ProcessEventsInDynamicUpdate` and `editorInputBehaviorInPlayMode=PointersAndKeyboardsRespectGameViewFocus` both at original values.

## 7. Technical Notes

- Editor does not tick between MCP eval calls (`runInBackground` off) — all frame advancement via `EditorApplication.Step()`.
- UI clicks verified through `EventSystem.RaycastAll` + `ExecuteEvents.pointerClickHandler` at computed screen positions.
- The valve tap was the hard part: `Step()` runs zero input updates and editor-type input updates never satisfy `wasPressedThisFrame` in a Dynamic context. Solved by adding a fresh `Pointer` device (clean front/back state buffers) and injecting a 0→1 press transition so the stepped frame's `Update` observed a real press — the full production input chain executed, not a shortcut.
- `LevelSelectPanel` node positions: Level 1 node at ~(380,1575), Level 2 at ~(700,1575); map world buttons at (350,1060)/(730,1060).

## 8. Known Limitations

- Highlight ring positions via `Camera.main.WorldToScreenPoint` each frame — correct while the level camera is static during Level 1; no camera-follow test needed at this scope.
- Tutorial is single-level by design (`tutorialLevel` serialized); extending to more levels is a data change, not code.
- `capture_game_view` squashes portrait output into a landscape image (tool artifact); layout verified via raycast coordinates.

## 9. Frozen-Phase Regression Check

- Reward overlay, economy grants, persistence pipeline, campaign map, HUD buttons, tool slots — all exercised and unchanged (Phases 20–23 untouched).
- `profile.json` verified on disk: `coins`, `completedLevelIDs`, `tutorialCompleted`, streak/tip fields coexist.

## 10. Final Verdict

**READY — PHASE 24 COMPLETE**

First launch → Main Menu → PLAY → World 1 → Level 1 → tutorial overlay → real valve tap through the production input path → step advance → water flow → normal completion → `tutorialCompleted` persisted → standard reward overlay → fresh session suppression → skip/restart/non-tutorial-level paths all verified at runtime.
