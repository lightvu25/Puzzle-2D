# Phase 22 — House + Cosmetics UI & Player Access — Implementation Report

**Date:** 2026-09-23
**Unity:** 6000.4.0f1
**Scene:** `Assets/Scenes/GameScene.unity`
**Test env:** Unity Editor Play Mode, Game View 1080×1920 portrait, real EventSystem raycast clicks (`ExecuteEvents` — no `Button.onClick.Invoke` shortcuts)

---

## 1. Phase 22 Scope

Expose the existing House Progression and Cosmetics backends through real, player-accessible UI from the Main Menu. Reuse `HouseProgressionManager`, `HouseProgressionDatabase`, `CosmeticInventory`, `CosmeticEquipService`, `EconomyManager`, `ProgressionManager`, `SaveManager`. No new progression/equipment systems, no fabricated content, no Phase 23 work.

## 2. Existing House Architecture Discovered

- `HouseProgressionManager` (singleton, `PERSISTENT DATA`): `Database`, `IsFeatureUnlocked(id)`, `CheckAndUnlockFeatures(completedLevels, acclaim)` — **auto-unlock evaluation, no purchase path exists**, `UnlockFeature(feature)`, `OnHouseUnlockChanged` event, persists to `profile.unlockedHouseFeatureIDs`.
- `HouseFeatureDefinition`: `featureId`, `displayName`, `description`, `icon`, `requiredCompletedLevels`, `requiredAcclaim`, `visitorAppraisalNotice`. `MeetsRequirements(completed, acclaim)` is the data-driven rule.
- **Pre-existing gaps found:**
  1. `HouseProgressionManager.database` was unassigned in the scene (`{fileID: 0}`) — assigned `HouseProgressionDatabase_Main`.
  2. `CheckAndUnlockFeatures` had **zero callers** in the codebase — the unlock evaluation was never wired to any event. `HouseUI.RefreshAll` now calls it on open (the designed evaluation entry point).

## 3. Existing Cosmetics Architecture Discovered

- `CosmeticCategory` enum (6): Furniture, RoomTheme, ArchitecturalFinish, FluidSkin, HardwareFinish, SteamVFX.
- `CosmeticDefinition`: `cosmeticId`, `category`, `displayName`, `description`, `previewSprite`, `colorTint`, `materialOverride`, `requiredAcclaim`, `coinPrice`, `gemPrice`.
- `CosmeticInventory` (singleton): owned-ID set, `OwnsCosmetic`, `GrantCosmetic` → `profile.ownedCosmeticIDs`.
- `CosmeticEquipService` (singleton): per-category equip map, `EquipCosmetic(def)` (ownership-gated), `UnequipCosmetic(cat)`, `GetEquippedCosmeticId(cat)` → `profile.equippedCosmetics`.
- **Pre-existing gap found:** `CosmeticEquipService` was **not present in the scene at all** — `Instance` was null at runtime. Added to `PERSISTENT DATA`.
- **No cosmetic purchase backend exists** — `CosmeticDefinition` has price fields but nothing consumes them. UI therefore exposes LOCKED/EQUIP/UNEQUIP/EQUIPPED only; no purchase button was invented.

## 4. Existing Data/Content Discovered

| Content | State |
|---|---|
| `HouseProgressionDatabase_Main` | 2 features: **Porch Restoration** (req: 1 completed + 10 Acclaim), **Kitchen Retrofit** (req: 2 completed + 20 Acclaim) |
| Cosmetic definitions | **ZERO assets exist** — no `.asset` of type `CosmeticDefinition` in the project. Catalog is empty; UI is fully built but shows per-category empty state. |
| Cosmetic catalog/registry type | Does not exist — `CosmeticInventory` tracks owned IDs only. `CosmeticsUI` takes a serialized `List<CosmeticDefinition>` catalog (empty until content is authored). |
| Icons/previews | None assigned on any feature definition. |

## 5. Files Created

| File | Purpose |
|---|---|
| `Assets/Scripts/HouseFlow/UI/HouseUI.cs` (~200 lines) | House screen: stats header, data-driven feature cards, unlock evaluation via `CheckAndUnlockFeatures`, `OnHouseClosed` event |
| `Assets/Scripts/HouseFlow/UI/CosmeticsUI.cs` (~290 lines) | Cosmetics screen: enum-driven category tabs, catalog-driven cards, equip/unequip via `CosmeticEquipService`, `OnCosmeticsClosed` event |
| `AgentScripts/BuildHouseCosmeticsUI.cs` | One-shot editor builder: constructs both panels (with ScrollRects), wires all serialized refs, assigns house DB, saves scene (test tooling, outside `Assets/`) |

## 6. Files Modified

| File | Change |
|---|---|
| `Assets/Scripts/HouseFlow/UI/MainMenuUI.cs` | HOUSE → `HouseUI.OpenHouse()`, COSMETICS → `CosmeticsUI.OpenCosmetics()`; shared `RestoreMainMenu()` for all three close handlers; coming-soon fallback kept only if refs are null. SETTINGS/SHOP unchanged. |
| `Assets/Scenes/GameScene.unity` | Added `HousePanel` + `CosmeticsPanel` under Canvas; `HouseProgressionManager.database` = `HouseProgressionDatabase_Main`; added missing `CosmeticEquipService` component to `PERSISTENT DATA`; wired `MainMenuUI.houseUI`/`cosmeticsUI`. |

## 7. House UI Hierarchy

```
Canvas
└── HousePanel                       (HouseUI; active in scene, self-hides in Start)
    ├── BackBtn                      ("BACK")
    ├── TitleText                    ("HOUSE")
    ├── StatsRow                     (LEVELS DONE n / ACCLAIM n — live values)
    ├── FeaturesScroll               (ScrollRect + RectMask2D)
    │   └── FeaturesContainer        (VLG + ContentSizeFitter)
    │       ├── HouseFeatureCard     (INACTIVE template: NameText/DescText/ReqText/StateText)
    │       └── HouseCard_<featureId>×2  (runtime-spawned)
    └── FeedbackText
```

## 8. Cosmetics UI Hierarchy

```
Canvas
└── CosmeticsPanel                   (CosmeticsUI; same lifecycle pattern)
    ├── BackBtn / TitleText ("COSMETICS") / StatsRow (ACCLAIM)
    ├── CategoryTabs                 (HLG; CategoryTab template + 6 spawned enum tabs)
    ├── ItemsScroll → ItemsContainer (ScrollRect; CosmeticCard template + runtime cards)
    ├── EmptyText                    ("No items in <Category> yet.")
    └── FeedbackText
```

## 9. House Interaction Flow

Open → `OnEnable` → `CheckAndUnlockFeatures(GetCompletedLevelCount(), ShowcaseAcclaim)` → backend unlocks eligible features → `OnHouseUnlockChanged` → feedback + card rebuild → `SaveManager` persists `unlockedHouseFeatureIDs`.

**Observed live:** profile (1 completed, 30 acclaim) → **Porch Restoration unlocked on first open** (`UNLOCKED`, green), Kitchen Retrofit correctly `LOCKED` with "Requires: 2 completed levels + 20 Acclaim". `profile.json` shows `unlockedHouseFeatureIDs: ["porch_restoration"]`.

No purchase/action buttons — the backend has no per-feature player action beyond evaluation. This is the real behavior, not a stub.

## 10. Cosmetics Equip Flow

Card states: `EQUIPPED` → UNEQUIP button; `OWNED` → EQUIP → `CosmeticEquipService.EquipCosmetic` (ownership-gated); not owned → non-interactable LOCKED (no purchase backend exists).

**Service-level verification** (ephemeral runtime `CosmeticDefinition` — no project asset fabricated): equip without ownership → rejected (`Cannot equip ... does not own it`); grant → equip → `GetEquippedCosmeticId` returns ID, `profile.equippedCosmetics` written; unequip → cleared. Probe ID cleaned from profile afterward.

## 11. Persistence Behavior

| State | Result |
|---|---|
| `unlockedHouseFeatureIDs` | `porch_restoration` in `profile.json`; fresh session loads `IsFeatureUnlocked=True` **before any UI opens** |
| `equippedCosmetics` | Write verified via service probe (equip→save→field present); cleared on unequip |
| `ownedCosmeticIDs` | Grant→save→load round-trip verified |

## 12. Runtime Test Results

| Test | Result |
|---|---|
| **A — House open** | PASS — raycast click; 2 DB-driven cards; stats show 1/30; no nulls |
| **B — House interaction** | PASS — open triggers real `CheckAndUnlockFeatures`; Porch auto-unlocked + persisted; Kitchen stays locked |
| **C — Cosmetics open** | PASS — 6 enum-generated tabs; empty state text; tab switch via raycast updates message |
| **D — Equip** | Service path PASS (probe); **UI path not testable — zero cosmetic assets exist** (content limitation, not a defect) |
| **E — Persistence** | PASS — `porch_restoration` survives exit + fresh session load |
| **F — Reopen stability** | PASS — 3 cycles each panel; `housePanels=1`, `cosmPanels=1`, template+N children only; transient deferred-Destroy settles in 1 frame |
| **G — Portrait layout** | PASS — screenshot-verified; ScrollRects bound both lists; long tab names wrap but stay readable/clickable |

## 13. Console Results

- **Critical errors:** none (zero error-level entries in the session sweep).
- **Real warnings requiring action:** none.
- **Expected/known warnings:** `PlayerInteract.Instance is NULL`; `AudioSource.time`; CS06xx compile warnings; `Cannot equip 'Equip Probe'` — the intended ownership guard during testing.
- **Editor/MCP noise:** `BufferedFileLogStorage` flush warnings.

## 14. Known Limitations

- **No cosmetic content exists** — equip UI is built and the backend is verified, but no cards can appear until `CosmeticDefinition` assets are authored. Populate `CosmeticsUI.catalog` in the inspector when they exist.
- House feature cards show no icons (none assigned in data).
- Cosmetic category tab labels wrap on long enum names (`ArchitecturalFinish`) — readable and functional; cosmetic polish only.
- `CheckAndUnlockFeatures` is only evaluated on House open — it is not wired to level-completion events. If features should unlock mid-session without opening the panel, hook it to `ProgressionManager`'s completion event (backend wiring, deferred).

## 15. Deferred Items

- Cosmetic content authoring (definitions, icons, previews) — content task, not code.
- Cosmetic *purchase* path — no backend exists; when designed, UI card states already support it.
- House visual representation — definitions carry an `icon` field but no art exists; functional cards only per spec.
- `GameSession` cached-profile save paths (Phase 21 report §7) — unchanged.

## 16. Bugs Found & Fixed

| Defect | Fix |
|---|---|
| `HouseProgressionManager.database` unassigned in scene | Assigned `HouseProgressionDatabase_Main` |
| `CosmeticEquipService` missing from scene (`Instance` null — equip calls would have silently failed) | Added component to `PERSISTENT DATA` |
| `CheckAndUnlockFeatures` had zero callers — features could never unlock | Wired via `HouseUI` open evaluation (the designed entry point) |

## 17. Technical Debt (documented, not modified)

- No `CosmeticDatabase`/catalog asset type exists — the UI catalog is a serialized list on `CosmeticsUI`. If content volume grows, a proper catalog SO is the clean upgrade.
- `purchasedShopItemIDs`/`purchasedProductIDs` clobber fix from Phase 21 covers the economy path; `GameSession`'s other cached-profile saves remain latent.

## 18. Final Verdict

**READY — PHASE 22 COMPLETE**

Main Menu → HOUSE → real data-driven UI → live unlock evaluation through `HouseProgressionManager` → persisted (`porch_restoration`) → Back → reopen, no duplicates. Main Menu → COSMETICS → real UI with enum-driven tabs and empty-state handling; equip backend verified end-to-end at service level — UI card interaction is **not testable with current content** (zero `CosmeticDefinition` assets), which is a content limitation, not a blocker. Scene clean and saved; console error-free. No Phase 23 work started.
