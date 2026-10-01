using System;
using UnityEngine;

/// <summary>
/// A star pickup placed inside a maze layout. Collecting one feeds the
/// level's star rating shown on the completion screen.
///
/// StarCollectible IS a Collectible, so it travels the existing pipeline:
/// OnTriggerEnter2D -> Collect() -> LevelRoot.NotifyCollectibleCollected.
/// The static OnAnyStarCollected event lets feedback/UI react to star
/// pickups specifically without knowing which level root spawned them.
/// </summary>
public class StarCollectible : Collectible
{
    /// <summary>Fired whenever any star pickup is collected. Argument is the collected star.</summary>
    public static event Action<StarCollectible> OnAnyStarCollected;

    protected override void OnCollected()
    {
        OnAnyStarCollected?.Invoke(this);
    }
}
