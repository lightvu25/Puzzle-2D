using System;
using UnityEngine;

/// <summary>
/// Mock implementation of IRewardedAdService for Editor workflows, automated testing,
/// and environments without native mobile ad SDK binaries.
/// </summary>
public class MockRewardedAdService : MonoBehaviour, IRewardedAdService
{
    [Header("Mock Settings")]
    [SerializeField] private bool isAvailable = true;
    [SerializeField] private AdResultStatus defaultResult = AdResultStatus.Completed;

    public bool IsAvailable
    {
        get => isAvailable;
        set => isAvailable = value;
    }

    public AdResultStatus DefaultResult
    {
        get => defaultResult;
        set => defaultResult = value;
    }

    public bool IsAdAvailable(RewardedAdPlacement placement)
    {
        return isAvailable;
    }

    public void ShowRewardedAd(RewardedAdPlacement placement, Action<RewardedAdResult> onComplete)
    {
        if (!isAvailable)
        {
            onComplete?.Invoke(RewardedAdResult.Unavailable(placement));
            return;
        }

        RewardedAdResult result;
        switch (defaultResult)
        {
            case AdResultStatus.Completed:
                result = RewardedAdResult.Success(placement);
                break;
            case AdResultStatus.Skipped:
                result = RewardedAdResult.Skipped(placement);
                break;
            case AdResultStatus.Unavailable:
                result = RewardedAdResult.Unavailable(placement);
                break;
            default:
                result = RewardedAdResult.Failed(placement, "Mock simulated failure.");
                break;
        }

        Debug.Log($"[MockRewardedAdService] ShowRewardedAd for {placement} -> {result.Status}");
        onComplete?.Invoke(result);
    }
}

