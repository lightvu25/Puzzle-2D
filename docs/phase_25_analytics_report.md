# PHASE 25 — ANALYTICS — FINAL REPORT

**Project:** HOUSEFLOW (2D mobile puzzle, Android/Google Play target)
**Unity:** 6000.4.0f1
**Scene:** `Assets/Scenes/GameScene.unity`
**Date:** 2026-09-24
**Status:** COMPLETE — verified in live Play Mode

---

## 1. Scope

First analytics layer for HOUSEFLOW. Local/mock only — no Firebase, GA4,
Unity Analytics, GameAnalytics, AppsFlyer, Adjust, or any network endpoint.
Architecture allows swapping the mock for a production provider later without
touching gameplay systems.

## 2. Architecture

```
Gameplay/UI systems (existing events + minimal new events)
        ↓
AnalyticsEventObserver          (MonoBehaviour on PERSISTENT DATA)
        ↓
IAnalyticsService.TrackEvent    (abstraction; callers never see the mock)
        ↓
AnalyticsService                (MonoBehaviour singleton on PERSISTENT DATA)
        ↓
IAnalyticsProvider
        ↓
MockAnalyticsProvider           (in-memory record list + Debug.Log)
```

### New files — `Assets/Scripts/HouseFlow/Analytics/`

| File | Role |
|---|---|
| `IAnalyticsService.cs` | `TrackEvent(name)` + `TrackEvent(name, Dictionary<string,object>)` |
| `IAnalyticsProvider.cs` | Swappable provider abstraction |
| `AnalyticsEvents.cs` | Centralized event-name consts — no scattered strings |
| `AnalyticsEventRecord.cs` | Small record type for local inspection |
| `MockAnalyticsProvider.cs` | Records events; `SimulateFailures` flag for safety testing |
| `AnalyticsService.cs` | Singleton; wraps provider calls in try/catch — analytics can never break gameplay; fires `session_start` once per play session |
| `AnalyticsEventObserver.cs` | Subscribes to existing service events in `Awake`, maps to `TrackEvent` |

### Minimal edits to existing code

| File | Change |
|---|---|
| `LevelFlowController.cs` | `OnLevelRestarted` event fired from the real `ResetLevel()` path |
| `TutorialUI.cs` | `OnTutorialStarted / StepCompleted / Skipped / Completed` events at its own existing state transitions (no gameplay change) |
| `RewardOverlayUI.cs` | `reward_shown` on display; `reward_claimed` on exit from Completed state (covers NEXT / MAP / RESTART); captures the completed LevelData at grant time so NEXT doesn't report the next level's ID |
| `ShopUI.cs` | `shop_opened` in `OpenShop()`; `shop_purchase_failed{reason:insufficient_currency}` only when `CanPurchase` rejects a genuinely unaffordable item |
| `ToolTrayUI.cs` | `tool_used{tool_type, level_id}` inside the activation-success callback only |
| `CampaignMapUI.cs` | `world_opened{world_index, world_name}` after the unlock check in `SelectWorld` |
| `GameScene.unity` | `AnalyticsService` + `AnalyticsEventObserver` added to `PERSISTENT DATA` |

## 3. Runtime Verification (Play Mode)

| Event | Result |
|---|---|
| `session_start` | Fires exactly once per session; no duplicates on scene/map/level transitions |
| `world_opened` | `{world_index=0, world_name=...}` on real world button click |
| `level_started` | `{level_id=level_001, world_index=1}` |
| `level_completed` | `{level_id, world_index, stars=1}` on real water→drain completion |
| `level_failed` | `{level_id, world_index, reason}` via real `FailLevel` path |
| `level_restarted` | Real HUD RestartBtn click |
| `level_abandoned` | Fixed: only when leaving `Playing` for map — NOT on Completed exit |
| `level_unlocked` | Real `UnlockLevel` path; second call does not re-fire |
| `tutorial_started` | Once per tutorial activation (double-invoke deduped) |
| `tutorial_step_completed` | `{step=0}`, `{step=1}` on real valve observation |
| `tutorial_skipped` | Real SKIP button click |
| `tutorial_completed` | On tutorial completion |
| `reward_shown` | `{level_id, world_index, coins, gems}` on overlay display |
| `reward_claimed` | **Fixed:** reports completed level (`level_001`) even via NEXT; single event per reward |
| `reward_doubled` | Only after real `AdRewardService` mock-ad grant |
| `shop_opened` | Real SHOP button |
| `shop_purchase_success` | `{item_id, cost_type, cost_amount}` on real coin purchase |
| `shop_purchase_failed` | `{item_id, reason=insufficient_currency}` on unaffordable gem item |
| `tool_used` | `{tool_type=FixItTool, level_id}` only on real consumption (count 3→2) |
| `daily_reward_claimed` | `{streak_day, coins}` once; rejected same-day second click = silent |
| `visitor_tip_collected` | `{coins}` once; cooldown second click = silent |
| `house_feature_unlocked` | Real `UnlockFeature`; re-unlock of unlocked feature = silent |
| `cosmetic_equipped` | Real `EquipCosmetic` after `GrantCosmetic`; unowned-equip attempt = silent |
| **Failure safety** | `SimulateFailures=true` → provider throws → `TrackEvent` returns normally, gameplay unaffected, provider recovers |

### Deferred (no reliable source yet)

- Rewind tool analytics — tool not implemented.
- Generic "ad watched" — no production ad SDK; only real `reward_doubled` grants tracked.

## 4. Fixes During Verification

1. **`reward_claimed` wrong level on NEXT** — flow controller swapped
   `CurrentLevelData` to level_002 before the observer ran. Fixed by capturing
   the completed `LevelData` in `RewardOverlayUI` at grant/display time.
2. **`reward_claimed` missed on NEXT/MAP/RESTART** — claim detection moved from
   `OnContinueClicked` to the Completed-state exit path (covers all dismissal
   buttons), with a guard against duplicates.
3. **`tutorial_started` double-fire** — `TryBegin` reached via both
   `OnStateChanged`→Playing and `OnLevelStarted`; gated so only the inactive→
   active transition emits.
4. **`level_abandoned` never fired** — `ReturnToMap` sets state `Idle` *before*
   `OnReturnToMapRequested`, so the observer's last-seen state was already Idle.
   `HandleLevelStateChanged` now ignores Idle, preserving the exited state.

## 5. Safety / Privacy

- All `TrackEvent` calls are exception-safe; verified provider failures cannot
  propagate to gameplay.
- Parameters are small primitives only (ids, indices, counts). No ProfileData,
  LevelData objects, scene data, or personal data is ever sent.
- `MockAnalyticsProvider` performs no network I/O.

## 6. Regression Status

- Phase 20–24 flows exercised during testing (menu → map → level → tutorial →
  completion → reward → next level, shop, daily, visitor, house, cosmetics,
  tools) — all intact.
- Console: **0 errors** after final session; test diagnostics reverted.
- `profile.json` restored from pre-session backup (test cosmetic/house
  entries removed).
- Scene saved with both analytics components on `PERSISTENT DATA`.

**READY — PHASE 25 COMPLETE**
