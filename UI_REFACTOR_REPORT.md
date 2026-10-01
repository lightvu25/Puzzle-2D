# UI Refactor Report — Kinetic Maze

Scope: clean/reorganize/refactor the UI system only. No gameplay systems were modified.

---

## 1. Files Moved

All moves preserved `.meta` GUIDs — zero serialized references were broken by the moves.

### `Assets/Scripts/UI/Core/`
| File | From |
|---|---|
| `UIManager.cs` | `UI/` |
| `UIPanelType.cs` | `UI/` |
| `UIPanelAnimator.cs` | `UI/` |
| `IUIPanel.cs` | `Core/Interfaces/` |

### `Assets/Scripts/UI/Panels/`
| File | From |
|---|---|
| `MainMenuUI.cs` | `UI/MainMenu/` |
| `CampaignMapUI.cs` | `UI/Campaign/` |
| `InGameHUD.cs` | `UI/Gameplay/` |
| `PauseMenuUI.cs` | `UI/Gameplay/` (contents untouched) |
| `ResultPanelUI.cs` | `UI/Gameplay/` |
| `ShopUI.cs` | `UI/Shop/` |
| `PowerupPanelUI.cs` | `UI/MainMenu/` |
| `MissionPanelUI.cs` | `UI/MainMenu/` |
| `DailyRewardUI.cs` | `UI/Rewards/` |
| `CoinBonusUI.cs` | `UI/Rewards/` |
| `RewardOverlayUI.cs` | `UI/Rewards/` |
| `BlindBoxUI.cs` | `UI/Gameplay/` |
| `TutorialUI.cs` | `UI/Tutorial/` |
| `PostGameAdsUI.cs` | `UI/Gameplay/` |
| `LoginUI.cs` | `UI/` |
| `SimplePanelUI.cs` | **new** |

### `Assets/Scripts/UI/Widgets/`
| File | From |
|---|---|
| `LevelNodeWidget.cs` | `UI/Campaign/` |

Old empty folders (`Campaign/`, `Gameplay/`, `MainMenu/`, `Rewards/`, `Shop/`, `Tutorial/`) removed with their `.meta` files. `Core/Interfaces/IInteractable.cs` stays — it's not UI.

## 2. Files Modified

| File | Change |
|---|---|
| `UI/Core/UIManager.cs` | Removed auto-`Instantiate` of non-scene panels (warns + skips instead). Removed dead `lastPanelCloseFrame`. Reordered `CloseCurrentPanel` to clear `currentActivePanel`/`timeScale` **before** `Hide()` — prevents infinite recursion when a `Hide()` callback re-opens a panel (e.g. popup close → re-open Map tab). |
| `UI/Core/UIPanelType.cs` | Appended `DailyReward`, `CoinBonus`. Existing enum ints unchanged. |
| `UI/Panels/ResultPanelUI.cs` | Implements `IUIPanel`. Completion/failure events set a pending state then route through `UIManager.OpenPanel(Result)`; `Show()` displays whichever result is pending. Buttons close the panel via `ClosePanelIfOpen` before calling `LevelFlowController`, so `timeScale` is always restored. |
| `UI/Panels/CampaignMapUI.cs` | Implements `IUIPanel` (`Show`→`OpenMap`, `Hide`→`CloseMap`). `OnReturnToMapRequested` now opens the map through `UIManager`; new `OnLevelStarted` handler closes the map as a safety net for any non-node start path; `RequestClose()` closes via `UIManager` when it's the active panel. |
| `UI/Panels/ShopUI.cs` | Implements `IUIPanel` (`Show`→`OpenShop`, `Hide`→`CloseShop`). |
| `UI/Panels/PowerupPanelUI.cs` | Implements `IUIPanel` (`Show`/`Hide` toggle the GameObject; `OnEnable` refreshes). |
| `UI/Panels/MissionPanelUI.cs` | Same as above. |
| `UI/Panels/DailyRewardUI.cs` | Implements `IUIPanel`. Removed unused `Instance` singleton. |
| `UI/Panels/CoinBonusUI.cs` | Same. Removed unused `Instance` singleton. |
| `UI/Panels/MainMenuUI.cs` | Tab selection + Daily Reward / Coin Bonus buttons now route through `UIManager.OpenPanel` (direct `SetActive` calls kept as fallback when a panel isn't registered). Sub-panel close handlers re-select the current tab. New `OnStateChanged` handler hides the menu shell during `Playing` and restores it on `Idle`. |
| `UI/Panels/InGameHUD.cs` | Menu button opens Pause via `UIManager.OpenPanel(Pause)` (direct `pauseMenu.Show()` fallback kept). Added optional `levelText`, `movesText` (`OnTileCrossed`), `tierText` (`OnTierChanged`) display slots — consume existing public `PlayerMovement`/`LevelFlowController` APIs only. |
| `UI/Panels/BlindBoxUI.cs` | Reveal uses `WaitForSecondsRealtime` + `SetUpdate(true)` tweens so it works while `Result` freezes `timeScale`. |

## 3. Files Intentionally Unchanged

- **`PauseMenuUI.cs`** — per instruction. Note: it declares matching `public void Show()`/`Hide()` methods but does **not** implement `IUIPanel` (class declaration can't be extended without editing it). Registered via `SimplePanelUI` on `PauseOverlay` — functionally identical since its `Show`/`Hide` only toggle `pausePanel`.
- `UIPanelAnimator.cs`, `IUIPanel.cs` — content untouched (moved only).
- `LevelNodeWidget.cs`, `LoginUI.cs`, `TutorialUI.cs`, `RewardOverlayUI.cs`, `PostGameAdsUI.cs` — moved only. These are self-driven overlays/infra, not navigable panels, so they stay unregistered and don't need `IUIPanel` (documented below).

## 4. UI Hierarchy Changes (GameScene)

```
Canvas
├── Screens
│   ├── MainMenuScreen      (was MainMenuUI — header, tab bar)
│   ├── CampaignMapScreen   (was CampaignMapPanel)
│   └── GameplayScreen      (was GameplayHUDPanel — InGameHUD, HUDContent)
├── Overlays
│   ├── TapToPlayOverlay    (was TapToPlayPanel)
│   ├── LoginOverlay        (was LoginPanel)
│   ├── TutorialOverlay
│   ├── PauseOverlay        (was PausePanel — pulled out of GameplayHUDPanel)
│   └── ResultOverlay       (new — wraps LevelCompletedPanel + LevelFailedPanel, hosts ResultPanelUI)
├── Popups
│   ├── ShopPopup, PowerupPopup, MissionPopup, SettingsPopup,
│   │   DailyRewardPopup, CoinBonusPopup, BlindBoxPopup, AdOfferPopup,
│   └── ComingSoonPopup     (ComingSoonModal — modals pulled out of MainMenu)
├── _Legacy
│   └── zz_LegacyPauseMenu_DELETE   (old duplicate pause menu, deactivated)
└── System
    ├── EventSystem
    └── UIManager
```

## 5. UIManager Changes

- Singleton, `Dictionary<UIPanelType, IUIPanel>` registry — same public API (`OpenPanel`, `CloseCurrentPanel`, `ClosePanelIfOpen`, `GetPanel<T>`, `IsPanelOpen`, `IsAnyPanelOpen`, `CurrentActivePanel`).
- **No more prefab instantiation or scene searching** — mappings must be scene GameObjects; non-scene objects log a warning and are skipped.
- `freezeTime` kept as a generic per-mapping flag. Assigned: `Pause`=true, `Result`=true, all others false.
- Reentrancy fix: `currentActivePanel` is cleared *before* `Hide()` runs, so close-events that open another panel can't recurse.

## 6. IUIPanel Changes

Contract unchanged (`Show()`/`Hide()`). Implementations added to: `CampaignMapUI`, `ResultPanelUI`, `ShopUI`, `PowerupPanelUI`, `MissionPanelUI`, `DailyRewardUI`, `CoinBonusUI`, `SimplePanelUI` (new). `PauseMenuUI` has compatible methods but no interface declaration (file protected) — bridged by `SimplePanelUI`.

## 7. Inspector References Changed (GameScene)

- `UIManager.panelMappings` rewritten — previously only 2 stale entries (`8`→MainMenuUI GO, `9`→CampaignMapPanel, which under the sequential enum meant Mission/Setting). Now 9 correct mappings (see §4 hierarchy names):
  `Map→CampaignMapScreen`, `Pause→PauseOverlay`, `Result→ResultOverlay`, `Shop→ShopPopup`, `Powerups→PowerupPopup`, `Mission→MissionPopup`, `Setting→SettingsPopup`, `DailyReward→DailyRewardPopup`, `CoinBonus→CoinBonusPopup`.
- `ResultPanelUI` component moved from `GameplayScreen` to `ResultOverlay` (all serialized fields preserved via CopyComponent) — required because multiple `IUIPanel`s can't share one mapped GameObject.
- `SimplePanelUI` added to `PauseOverlay` and `SettingsPopup`.
- **Re-wired stale refs** left over from the legacy Text→TMP conversion: `ShopUI` (coins/gems/stars/feedback), `DailyRewardUI` (all text fields + claim label), `CoinBonusUI` (all text fields + collect label), `TutorialUI` (instruction/step text + `tutorialLevel`→`Level_KM_001`), `LoginUI` (user/status text), `PauseMenuUI.levelTitleText`→`PauseOverlay/Title`.
- `InGameHUD`: `gemsText`/`movesText`/`tierText` wired to newly created `GemsText`, `MovesText`, `TierText` objects under `HUDContent` (cloned from `CoinsText`).
- Removed the dangling missing-script slot on `ToolTrayPanel` (legacy `HouseFlow.UI.ToolTrayUI` — script deleted long ago). GameObject kept.

## 8. Prefabs Changed

None.

## 9. Deleted Files

None.

## 10. Deletion Candidates

- `zz_LegacyPauseMenu_DELETE` — old duplicate pause menu GameObject + its second `PauseMenuUI` component. Deactivated under `_Legacy`; delete once confirmed nothing references it.
- `ToolTrayPanel` + `Slot_*` children — HouseFlow-era tool tray (FixItTool/VacuumPump/Magnet/BlueprintHint) with no backing script.
- `UIPanelAnimator` — exists and compiles, but nothing references it yet; keep as optional panel-transition helper or delete later.
- MainMenuScreen's disabled `PlayBtn/MapBtn/ShopBtn/SettingsBtn/TitleText` children — superseded by the tab bar.

## 11. Blocked Gameplay Dependencies (not changed — reported)

- **`CampaignDatabase` asset is missing.** `ProgressionManager.campaignDatabase` is unassigned; `Level_KM_001`'s `layoutPrefab` GUID also resolves to nothing. Result: world buttons and level nodes don't populate on the map, and `Level_KM_001` fails to start. `Level_001` (→ `Level 1.prefab`) works — verified live.
- `PauseMenuUI` doesn't declare `: IUIPanel` (can't edit it) — bridged via `SimplePanelUI` as above.

## 12. Compile Result

`recompile` → **completed, failed=false, errors=[]** (final state after all edits).

## 13. Runtime Validation Result (live play-mode session)

| Check | Result |
|---|---|
| GameScene loads, Canvas + EventSystem present | PASS |
| No Missing Script / Missing Reference | PASS (0 missing after cleanup) |
| Tap-to-play → Main Menu shows, **Map auto-opens** (`cur=Map`) | PASS |
| Tab switching Map→Shop→Missions→Settings→Powerups→Map | PASS (each opens via UIManager, previous closes) |
| Level start → HUD up, map+menu hidden, `cur=None` | PASS |
| Coins & Gems display in HUD | PASS |
| MenuBtn → Pause opens, `timeScale=0`, **HUD stays visible behind** | PASS |
| Resume → `timeScale=1`, pause closed | PASS |
| FailLevel → Result overlay (`cur=Result`, `ts=0`, failure panel shown) | PASS |
| Retry → `Playing`, result closed, `ts=1` | PASS |
| Map button → `Idle`, map + menu restored via UIManager | PASS |
| DailyReward/CoinBonus popups open over map and restore map on close | PASS |
| Chest button → BlindBox opens | PASS |
| Shop/Powerups/Missions/Tutorial/Rewards — no errors thrown | PASS |

Level-node click path unchanged (`OnLevelNodeSelected → StartLevel → RequestClose`); world-button spawning unverifiable until a CampaignDatabase asset exists (see §11).

## 14. Remaining UI Issues

- Munro font lacks `★` (U+2605) — TMP warns and substitutes `□` on `CompStars`/`StarsText`. Cosmetic: replace with `*` or add a fallback font glyph.
- `MainMenuUI.levelIconImage`, `PowerupPanelUI` catalog icons, mission icons, `ResultPanelUI` title/coin images — intentionally unassigned art slots (per your "assign images yourself" request).
- `DailyRewardPanel`/`CoinBonusPanel`/`ShopPanel`/`TutorialOverlay` are **active in the saved scene**; their `Start()` self-hides them at runtime. Harmless, but could be saved inactive for a cleaner editor view.
- One-frame edge: buttons bound in `Start()` (e.g. map chest/free-power) aren't clickable during the single frame a panel first activates — invisible to humans, only visible to synchronous eval calls.
