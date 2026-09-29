using System;
using UnityEngine;

/// <summary>
/// Singleton service managing game currencies:
///   1. Coins (soft currency for standard tools, basic decor)
///   2. Gems (premium currency for collections, bundles, shaders)
///   3. Showcase Acclaim (non-spendable prestige metric unlocking cosmetic tiers)
///
/// Persistence is integrated directly with SaveManager / ProfileData.
/// Strictly guarantees non-negative balances.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    // ─────────────────────────────────────────────────────────────
    //  Events (newBalance, delta)
    // ─────────────────────────────────────────────────────────────
    public event Action<int, int> OnCoinsChanged;
    public event Action<int, int> OnGemsChanged;
    public event Action<int, int> OnShowcaseAcclaimChanged;

    // ─────────────────────────────────────────────────────────────
    //  Runtime Cache
    // ─────────────────────────────────────────────────────────────
    private int coins;
    private int gems;
    private int showcaseAcclaim;

    public int Coins => coins;
    public int Gems => gems;
    public int ShowcaseAcclaim => showcaseAcclaim;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadFromProfile();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ─────────────────────────────────────────────────────────────
    //  Coins API
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds coins to the player's wallet. Amount must be positive.
    /// </summary>
    public void AddCoins(int amount, string source = "")
    {
        if (amount <= 0) return;

        coins += amount;
        SaveToProfile();
        OnCoinsChanged?.Invoke(coins, amount);
        Debug.Log($"[EconomyManager] Added {amount} Coins (Source: {source}). New Balance: {coins}");
    }

    /// <summary>
    /// Checks whether the player can afford the specified coin cost.
    /// </summary>
    public bool CanAffordCoins(int amount)
    {
        if (amount < 0) return false;
        return coins >= amount;
    }

    /// <summary>
    /// Spends coins from the player's wallet. Returns false if insufficient funds or amount <= 0.
    /// Guaranteed never to produce a negative balance.
    /// </summary>
    public bool SpendCoins(int amount, string reason = "")
    {
        if (amount <= 0) return false;
        if (!CanAffordCoins(amount))
        {
            Debug.LogWarning($"[EconomyManager] SpendCoins failed: Insufficient coins ({coins} < {amount}). Reason: {reason}");
            return false;
        }

        coins -= amount;
        SaveToProfile();
        OnCoinsChanged?.Invoke(coins, -amount);
        Debug.Log($"[EconomyManager] Spent {amount} Coins (Reason: {reason}). Remaining: {coins}");
        return true;
    }

    // ─────────────────────────────────────────────────────────────
    //  Gems API
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds gems to the player's wallet. Amount must be positive.
    /// </summary>
    public void AddGems(int amount, string source = "")
    {
        if (amount <= 0) return;

        gems += amount;
        SaveToProfile();
        OnGemsChanged?.Invoke(gems, amount);
        Debug.Log($"[EconomyManager] Added {amount} Gems (Source: {source}). New Balance: {gems}");
    }

    /// <summary>
    /// Checks whether the player can afford the specified gem cost.
    /// </summary>
    public bool CanAffordGems(int amount)
    {
        if (amount < 0) return false;
        return gems >= amount;
    }

    /// <summary>
    /// Spends gems from the player's wallet. Returns false if insufficient funds or amount <= 0.
    /// Guaranteed never to produce a negative balance.
    /// </summary>
    public bool SpendGems(int amount, string reason = "")
    {
        if (amount <= 0) return false;
        if (!CanAffordGems(amount))
        {
            Debug.LogWarning($"[EconomyManager] SpendGems failed: Insufficient gems ({gems} < {amount}). Reason: {reason}");
            return false;
        }

        gems -= amount;
        SaveToProfile();
        OnGemsChanged?.Invoke(gems, -amount);
        Debug.Log($"[EconomyManager] Spent {amount} Gems (Reason: {reason}). Remaining: {gems}");
        return true;
    }

    // ─────────────────────────────────────────────────────────────
    //  Showcase Acclaim API (Non-Spendable Prestige)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds to the player's cumulative Showcase Acclaim prestige score.
    /// Showcase Acclaim is strictly non-spendable and acts as an unlock tier metric.
    /// </summary>
    public void AddShowcaseAcclaim(int amount, string source = "")
    {
        if (amount <= 0) return;

        showcaseAcclaim += amount;
        SaveToProfile();
        OnShowcaseAcclaimChanged?.Invoke(showcaseAcclaim, amount);
        Debug.Log($"[EconomyManager] Added {amount} Showcase Acclaim (Source: {source}). New Total: {showcaseAcclaim}");
    }

    // ─────────────────────────────────────────────────────────────
    //  Persistence Integration
    // ─────────────────────────────────────────────────────────────

    public void LoadFromProfile()
    {
        ProfileData profile = GameSession.Instance?.currentProfile ?? SaveManager.loadProfile();
        if (profile != null)
        {
            coins = Math.Max(0, profile.coins);
            gems = Math.Max(0, profile.gems);
            showcaseAcclaim = Math.Max(0, profile.showcaseAcclaim);
        }
        else
        {
            coins = 0;
            gems = 0;
            showcaseAcclaim = 0;
        }
    }

    private void SaveToProfile()
    {
        // Always load fresh from disk: GameSession.currentProfile can be a stale
        // snapshot that would silently revert fields written by other services.
        ProfileData profile = SaveManager.loadProfile() ?? new ProfileData();
        profile.coins = coins;
        profile.gems = gems;
        profile.showcaseAcclaim = showcaseAcclaim;
        SaveManager.saveProfile(profile);

        if (GameSession.Instance != null)
        {
            GameSession.Instance.currentProfile = profile;
        }
    }

    /// <summary>
    /// Testing helper to reset economy values in memory and save.
    /// </summary>
    public void ResetEconomy()
    {
        int oldCoins = coins;
        int oldGems = gems;
        int oldAcclaim = showcaseAcclaim;

        coins = 0;
        gems = 0;
        showcaseAcclaim = 0;
        SaveToProfile();

        OnCoinsChanged?.Invoke(0, -oldCoins);
        OnGemsChanged?.Invoke(0, -oldGems);
        OnShowcaseAcclaimChanged?.Invoke(0, -oldAcclaim);
        Debug.Log("[EconomyManager] Economy reset to zero.");
    }
}

