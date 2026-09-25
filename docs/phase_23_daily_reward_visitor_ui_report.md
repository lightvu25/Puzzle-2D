# Phase 23 — Daily Reward + Visitor UI — Implementation Report

**Date:** 2026-09-23
**Unity:** 6000.4.0f1
**Scene:** `Assets/Scenes/GameScene.unity`
**Test env:** Editor Play Mode, 1080×1920 portrait, real EventSystem raycast + `ExecuteEvents` clicks (no `onClick.Invoke` shortcuts)

---

## 1. Phase 23 Scope

Expose `DailyRewardService` and `VisitorTipService` through real player-accessible UI from the Main Menu. Reuse existing RewardService / EconomyManager / SaveManager pipeline. No new reward/economy rules, no duplicated systems, no Phase 24 work.

## 2. Existing DailyRewardService Architecture

- `CanClaimDailyReward()` — claimable when `lastClaimUtcTicks==0` or `DateTime.UtcNow.Date > lastClaimDate` (once per UTC calendar day; UTC prevents timezone manipulation).
- `ClaimDailyReward()` — backend-guarded (returns false + warning on same-day re-claim), updates streak, builds reward via `BuildStreakRewardBundle(day)`, grants through `RewardService`, persists, fires `OnDailyRewardClaimed`.
- Streak: consecutive day → `(day % 7) + 1`; any gap → reset to 1. Fixed 7-day table (50c / BlueprintHint / 75c / FixItTool / 100c / VacuumPump / 150c+2g).
- `GetTimeUntilNextDailyReward()` — TimeSpan to next UTC day; `ResetForTesting()` exists.

**Defect found & fixed:** `currentStreakDay` was *not* persisted — `ProfileData` had no field for it, so every fresh session reset the streak to 1 and a consecutive-day claim would grant the wrong day's reward (e.g. a day-5 player restarting got day-2's reward). Added `dailyRewardStreakDay` to `ProfileData` and saved/loaded it in `DailyRewardService` — completing the existing 7-day design, not inventing a rule.

**Addition:** `GetUpcomingStreakDay()` — read-only preview of what the next claim would grant (mirrors `UpdateStreakOnClaim` without mutating), so the UI can display the pending day/reward without duplicating streak logic.

## 3. Existing VisitorTipService Architecture

- `CanCollectTips()` — repeatable with a **4-hour UTC cooldown** (`TipCooldownHours=4.0`).
- `CalculateTipAmount()` = `30 + acclaim × 2` (acclaim=30 → 90 coins observed).
- `CollectTips(doubleWithAd, onComplete)` — cooldown-guarded; optional ad path via `AdRewardService.WatchAdForReward(VisitorTipMultiplier)` which grants +tip bonus through `FulfillReward` → total 2×. Ad-cancel still grants base.
- `GetTimeUntilNextTip()`, `ResetForTesting()`, persists `lastVisitorTipClaimUtcTicks`. Event `OnVisitorTipsClaimed(int)`.
- Both services already use load-fresh-then-save persistence — no stale-profile clobbering found in either path (Phase 21 `EconomyManager` fix holds; verified `coins` + both claim ticks + `dailyRewardStreakDay` coexist correctly in `profile.json`).

## 4. Existing Reward/Economy Integration

Both services grant through `RewardService.GrantRewardBundle` → `EconomyManager`/`ToolInventory`. UI contains zero reward math — it reads `CanClaim*`, `GetUpcomingStreakDay`, `BuildStreakRewardBundle`, `CalculateTipAmount`, `GetTimeUntilNext*`, and forwards clicks to `ClaimDailyReward()` / `CollectTips()`.

## 5. Existing Data Discovered

- Daily reward table is hardcoded in `DailyRewardService.BuildStreakRewardBundle` (existing design — UI previews it via the public method, nothing fabricated).
- Visitor tip has no dialogue/personality data — functional card only, per spec.
- `AdRewardService` + `MockRewardedAdService` are in the scene; `IsAdAvailable(VisitorTipMultiplier)` returns true → the 2× ad button is shown (mock ad only, no production ads).
- All services already present in scene — no wiring gaps this phase.

## 6. Files Created

| File | Purpose |
|---|---|
| `Assets/Scripts/HouseFlow/UI/DailyRewardUI.cs` | Daily Reward panel: day/streak display, reward preview, availability + countdown, claim forwarding |
| `Assets/Scripts/HouseFlow/UI/VisitorTipUI.cs` | Visitor Tip panel: tip amount, cooldown status, collect + conditional ad-double buttons |
| `AgentScripts/BuildDailyVisitorUI.cs` | One-shot builder: panels + menu buttons + ref wiring + scene save |

## 7. Files Modified

| File | Change |
|---|---|
| `Assets/Scripts/Data/ProfileData.cs` | Added `dailyRewardStreakDay` field |
| `Assets/Scripts/HouseFlow/Meta/DailyRewardService.cs` | Persist `currentStreakDay`; added `GetUpcomingStreakDay()` preview |
| `Assets/Scripts/HouseFlow/UI/MainMenuUI.cs` | `dailyRewardUI`/`visitorTipUI` refs, `DailyRewardBtn`/`VisitorBtn` handlers, close-event restore |
| `Assets/Scenes/GameScene.unity` | `DailyRewardPanel`, `VisitorPanel`, `DailyRewardBtn`, `VisitorBtn` added; refs serialized |

## 8. Daily Reward UI Hierarchy

```
Canvas
└── DailyRewardPanel                 (DailyRewardUI; active in scene, self-hides in Start)
    ├── BackBtn / TitleText
    ├── CurrencyRow                  (COINS + GEMS, bound to OnCoinsChanged/OnGemsChanged)
    ├── RewardCard                   (DayText "Day N of 7" / RewardText bundle preview / StatusText countdown)
    ├── ClaimBtn                     (interactable iff CanClaimDailyReward; label CLAIM/CLAIMED)
    └── FeedbackText
```

## 9. Visitor UI Hierarchy

```
Canvas
└── VisitorPanel                     (VisitorTipUI; same lifecycle)
    ├── BackBtn / TitleText
    ├── CurrencyRow                  (COINS)
    ├── TipCard                      (TipText "A visitor left a tip: N Coins" / StatusText)
    ├── CollectBtn                   (interactable iff CanCollectTips)
    ├── CollectAdBtn                 ("2X TIP (AD)" — only active when IsAdAvailable && collectable)
    └── FeedbackText
```

## 10. Daily Claim Flow

`DailyRewardUI` → `DailyRewardService.ClaimDailyReward()` → `UpdateStreakOnClaim` → `BuildStreakRewardBundle` → `RewardService.GrantRewardBundle` → `EconomyManager.AddCoins` → `SaveManager` → `OnDailyRewardClaimed` → UI refresh.

Observed console chain: `Claimed Daily Reward Day 1` → `RewardService granted bundle` → `EconomyManager Added 50 Coins → 1830`.

## 11. Visitor Claim Flow

`VisitorTipUI` → `VisitorTipService.CollectTips(false|true)` → `RewardService` → `EconomyManager` → `SaveManager` → `OnVisitorTipsClaimed` → UI refresh.

Observed: `Added 90 Coins (Source: VisitorTips) → 1920`.

## 12. Double-Claim Behavior

| Path | Result |
|---|---|
| Second CLAIM click | Button `interactable=false` — raycast click is a no-op |
| Direct `ClaimDailyReward()` | Returns `false`, "already claimed today" warning, coins unchanged |
| Reopen panel + attempt | `canClaim=False` persists — cannot bypass via reopen |
| Second COLLECT (cooldown) | Same: disabled button + `CollectTips` returns `false`, "not ready" warning |

Backend guards are real — protection is in the service, not UI-only.

## 13. Persistence Behavior

`profile.json` verified after claims:

```
"coins": 1920
lastVisitorTipClaimUtcTicks: 639257598636089374
lastDailyRewardClaimUtcTicks: 639257594959504310
dailyRewardStreakDay: 1
```

Fresh Play Mode session: `coins=1920`, `dailyCanClaim=False`, `nextIn=12h26m`, `streak=1`, `upcoming=2` (correct next-day preview), `visitorCanCollect=False`, `nextTipIn=03:57`. All state survived restart.

## 14. Runtime Test Results

| Test | Result |
|---|---|
| A — Daily open | PASS — raycast; "Day 1 of 7", "50 Coins" preview, "Reward available!" |
| B — Daily claim | PASS — coins 1780→1830, button→CLAIMED, countdown shown |
| C — Double claim | PASS — UI disabled + backend `false`; no duplicate reward |
| D — Persistence | PASS — claimed state + streak + currency survive fresh session |
| E — Visitor open | PASS — "90 Coins" tip (30+acclaim×2), "ready to collect", ad button shown |
| F — Visitor claim | PASS — coins 1830→1920, 4h cooldown starts, ad button hides |
| G — Repeat/cooldown | PASS — `CollectTips`→false, balances unchanged (repeatable-by-design, cooldown enforced) |
| H — Reopen stability | PASS — multiple cycles both panels; single instances; no stale state |
| I — Portrait layout | PASS — all elements on-screen/reachable at 1080×1920 (verified via raycast positions + capture; capture tool squashes output vertically — artifact only) |
| J — Fresh session | PASS — see §13 |

## 15. Console Results

- **Critical errors:** none from game code.
- **Real warnings:** none new.
- **Expected:** `Daily reward already claimed today`, `Tips not ready to collect yet` (guard paths), `[EconomyManager] Added …` grant logs.
- **Noise:** MCP `SocketException`/SignalR connect failures and pipeline `HandleExecRequest` timeouts (my own timed-out evals during domain reloads) — tooling, not game.

## 16. Bugs Discovered & Fixed

| Defect | Fix |
|---|---|
| `currentStreakDay` not persisted → streak reset to 1 every fresh session, breaking the 7-day cycle | Added `dailyRewardStreakDay` to `ProfileData`; saved/loaded in `DailyRewardService` (load defaults to 1 for legacy saves) |

## 17. Known Limitations

- **Date-transition testing limited:** the service reads `DateTime.UtcNow` directly; no time abstraction exists and none was added per spec. Same-day double-claim, cooldown, and persistence were fully verified; cross-day streak progression is code-reviewed (and `GetUpcomingStreakDay` mirrors it) but not time-travel-tested.
- Visitor ad-doubling is mock-only (`MockRewardedAdService`) — production ads out of scope.
- `capture_game_view` output appears vertically squashed (tool renders portrait buffer into landscape image); actual on-screen layout verified via raycast coordinates.

## 18. Deferred Items

- `ResetForTesting()` exists on both services but is not exposed in UI (correctly — debug path only).
- If date-boundary behavior needs automated testing later, an `ITimeProvider` seam would be the clean approach — documented, not implemented.
- Visitor personality/dialogue content — no such data exists; functional UI only.

## 19. Technical Debt (not modified)

- Editor pause during camera transitions left the canvas at a stale scale briefly — a Cinemachine/editor-state artifact, not a game defect (recovered after frames ran).
- `MainMenuUI` now has 8 buttons in absolute layout — functional; a VerticalLayoutGroup would scale better if more entries are added later.

## 20. Final Verdict

**READY — PHASE 23 COMPLETE**

Main Menu → DAILY REWARD → real UI → `ClaimDailyReward` → +50 coins → double-claim blocked in backend → persists across restart → reopen clean. Main Menu → VISITOR → `CollectTips` → +90 coins → 4h cooldown enforced → persists across restart → reopen clean. One real defect found and fixed (streak persistence). Console clean. Scene saved, not dirty. No Phase 24 work started.
