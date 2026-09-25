# Phase 21 — Shop UI & Player Access — Implementation Report

**Date:** 2026-09-23
**Unity:** 6000.4.0f1
**Scene:** `Assets/Scenes/GameScene.unity`
**Test env:** Unity Editor Play Mode, Game View 1080×1920 portrait, real EventSystem raycast clicks (`ExecuteEvents` — no `Button.onClick.Invoke` shortcuts)

---

## 1. Phase 21 Scope

Turn the existing Shop backend (`ShopManager`, `ShopDatabase`, `ShopItemDefinition`, `EconomyManager`, `ToolInventory`, `IAPManager`/`MockIAPService`, `RewardService`, `SaveManager`) into a real, player-accessible Shop UI reachable from the Main Menu. No new economy/shop logic — UI only calls existing public APIs. No Phase 22 work, no House/Cosmetics UI, no production IAP.

## 2. Existing Shop Architecture Discovered

- `ShopManager` (singleton, `PERSISTENT DATA`): `CanPurchase(item)` gates one-time flags, cosmetic ownership, and affordability; `PurchaseItem(item, onComplete)` routes Coins → `EconomyManager.SpendCoins`, Gems → `SpendGems`, RealMoney → `IAPManager.Purchase(productId, cb)`; `FinalizePurchase` grants rewards via `RewardService.GrantRewardBundle` and persists one-time IDs via `profile.purchasedShopItemIDs`.
- `ShopItemDefinition`: `itemId`, `displayName`, `description`, `costType` (Coins/Gems/RealMoney), `costAmount`, `realMoneyProductId`, `isOneTimePurchase`, `rewards` (`List<RewardItem>`).
- `IAPManager` → `MockIAPService` (`IIAPService`): `Purchase` only grants entitlement on `IAPResult.IsSuccess`; records to `profile.purchasedProductIDs`.
- **Pre-existing gap found:** `ShopManager.database` was unassigned in the scene (shop was never reachable in Phase 20). Assigned `ShopDatabase_Main`.
- **Pre-existing data gap found:** all 4 shop items had empty `rewards` lists — purchases deducted currency but granted nothing. Populated from each item's own identity and matching `ToolDefinition` prices (see §6).

## 3. Files Created

| File | Purpose |
|---|---|
| `Assets/Scripts/HouseFlow/UI/ShopUI.cs` (~281 lines) | Shop screen controller: lifecycle, currency header, data-driven card spawn, purchase routing, feedback |
| `AgentScripts/BuildShopUI.cs` | One-shot editor builder: constructs `ShopPanel` hierarchy under Canvas, wires serialized refs, saves scene (outside `Assets/`, test tooling only) |

## 4. Files Modified

| File | Change |
|---|---|
| `Assets/Scripts/HouseFlow/UI/MainMenuUI.cs` | SHOP button routes to `ShopUI.OpenShop()` (subscribes `OnShopClosed` → show menu); falls back to existing coming-soon modal only if `shopUI` ref is null. All other routes unchanged. |
| `Assets/Scripts/HouseFlow/Economy/EconomyManager.cs` | **Bug fix** — `SaveToProfile` now loads fresh profile from disk instead of reusing `GameSession.currentProfile` (see §7). |
| `Assets/Data/Shop/ShopItem_*.asset` (4) | Populated empty `rewards` lists (data fill, no price/rule changes). |
| `Assets/Scenes/GameScene.unity` | Added `ShopPanel` under Canvas; assigned `ShopManager.database` = `ShopDatabase_Main`; wired `MainMenuUI.shopUI`. |

## 5. UI Hierarchy

```
Canvas
└── ShopPanel                        (ShopUI; active in scene, self-hides in Start)
    ├── BackBtn                      ("BACK")
    ├── TitleText                    ("SHOP")
    ├── CurrencyRow                  (COINS / GEMS / ACCLAIM live readouts)
    ├── ItemsContainer               (VerticalLayoutGroup, spacing 25)
    │   ├── ShopItemCard             (INACTIVE template)
    │   │   ├── NameText / DescText / ContentsText / PriceText
    │   │   └── BuyBtn ("BUY" / "OWNED")
    │   └── ShopCard_<itemId> ×4     (spawned at runtime from template)
    └── FeedbackText                 ("Purchased X!" / "Not enough Coins" / …)
```

Cards are fully data-driven from `ShopManager.Database.Items` — no hardcoded names, prices, or IDs.

## 6. Shop Item Data (populated `rewards`)

| Item | Cost | Rewards | Notes |
|---|---|---|---|
| Fix-It Pack | 100 Coins | 1× FixItTool | Matches `ToolDefinition` coinPrice=100 |
| Vacuum Pack | 75 Coins | 1× VacuumPump | Matches `ToolDefinition` coinPrice=75 |
| Coin Stash | 5 Gems | 250 Coins | |
| Starter Pack | $4.99 (mock IAP, `starter_pack`) | 500 Coins, 10 Gems, 1× FixItTool | `isOneTimePurchase=false` — repeatable per existing data |

## 7. Bug Found & Fixed — Profile Save Clobbering

**Defect:** `EconomyManager.SaveToProfile` wrote through `GameSession.Instance.currentProfile`, a snapshot cached at session start. Sequence observed live: mock IAP `RecordPurchase` wrote `purchasedProductIDs=["starter_pack"]` to disk → the reward grant's `AddCoins`→`SaveToProfile` then saved the **stale** cached profile, silently reverting `purchasedProductIDs` to `[]`. Result: IAP entitlements lost across sessions (verified: `starter=False` on fresh session before fix).

**Fix:** `SaveToProfile` now does `SaveManager.loadProfile()` first — identical pattern to every other service (`ToolInventory`, `IAPManager`, `ShopManager`). Verified: `purchasedProductIDs=["starter_pack"]` survives the subsequent economy save and reloads as `HasPurchased("starter_pack")=True` in a new session.

**Related latent issue (not exercised, documented only):** `GameSession.HandlePlayerDeath/AbandonRun/CompleteRun` also save the cached `currentProfile` and could revert fields written mid-session by other services. These are legacy run-mode paths not used by the HouseFlow UI flow; flagged for a future pass, not changed.

## 8. Mock IAP Behavior

`Starter Pack` → `ShopManager.PurchaseItem` → `IAPManager.Purchase("starter_pack")` → `MockIAPService.PurchaseProduct` returns `IAPResult.IsSuccess` with `Tx: mock_tx_*` → `RecordPurchase` → `FinalizePurchase` → rewards granted. No real billing, no credentials — mock path only, per spec.

## 9. Runtime Test Results (all real raycast clicks)

| Test | Result |
|---|---|
| **A — Open Shop** | PASS — Menu→SHOP opens panel; header shows live `COINS 380 / GEMS 10 / ACCLAIM 30`; 4 cards spawned from database |
| **B — Coin purchase** | PASS — Fix-It BUY: `coins 380→280`, `FixItTool 0→1`, header updated instantly, feedback "Purchased Fix-It Pack!" |
| **C — Insufficient funds** | PASS — Coin Stash ×2 (gems 10→0, coins +500); 3rd click rejected: balances unchanged, feedback "Not enough Gems" |
| **D — Mock IAP** | PASS — Starter Pack: `coins +500`, `gems +10`, `FixItTool +1`, `HasPurchased("starter_pack")=True` |
| **E — Persistence** | PASS — `profile.json`: `coins=1780, gems=20, toolInventory=[FixItTool×3], purchasedProductIDs=["starter_pack"]`; fresh session loads identical live state incl. `starter=True` |
| **F — Reopen stability** | PASS — 3× Menu→Shop→Back cycles; `shopPanels=1`, `cards=5` (1 template + 4 active) every time; no blank screen |
| **G — 1080×1920 layout** | PASS — screenshot-verified: header, currency row, 4 cards all on-screen, no overlap, BUY buttons reachable |

Transient note: reopening can briefly report 9 container children because `OnEnable` re-runs `RebuildItemCards` while `Destroy` is deferred; settles to 5 after one frame. No leak — `spawnedCards` list is cleared each rebuild.

## 10. Console Status

- **Critical errors:** none.
- **Real warnings requiring action:** none.
- **Expected/known warnings:** `[GameManager] PlayerInteract.Instance is NULL` (pre-existing, unrelated to shop); `AudioSource.time` resource warning (pre-existing); CS0618/CS0067/CS0414 compile warnings (pre-existing); `[ShopManager] Purchase rejected …` — the intended rejection path during TEST C.
- **Editor/MCP noise:** `BufferedFileLogStorage` flush warnings; one `ExecuteCommandByName` error from a mistyped test-tool call during testing (tooling artifact, not game code).

## 11. Known Limitations

- Shop has no ScrollView — 4 items fit without scrolling; needed only if catalog grows.
- No item icons displayed (`icon` field exists but assets have none assigned).
- No purchase-confirmation dialog — buys immediately on BUY (matches existing `PurchaseItem` API).
- `purchasedShopItemIDs` only records `isOneTimePurchase` items; all 4 current items are repeatable, so it stays empty by design.
- Descriptions are empty strings in current item assets (data, not UI defect).

## 12. Deferred Items

- Production Google Play Billing / Unity IAP (Phase later; mock retained).
- House / Cosmetics / Daily Reward / Visitor UIs — Phase 22+, untouched.
- `GameSession` cached-profile save paths (§7) — latent, not on the HouseFlow path.

## 13. Final Verdict

**READY — PHASE 21 COMPLETE**

Main Menu → Shop → real data-driven UI → purchase through existing `ShopManager` → currency/inventory update → `profile.json` persistence across sessions (incl. mock IAP entitlement after the `EconomyManager` save fix) → Back → reopen → no duplicate UI → no critical runtime errors.
