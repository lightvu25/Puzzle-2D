using UnityEngine;

/// <summary>
/// A dot/coin pickup placed inside a maze layout. Collected automatically
/// when the player slides over it. Reports the pickup to the parent
/// <see cref="LevelRoot"/> so objectives and the HUD can track progress.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [Tooltip("Optional id used by objectives that target a specific pickup.")]
    [SerializeField] private string pickupId;

    [Tooltip("Score/coin value granted when collected.")]
    [SerializeField, Min(1)] private int value = 1;

    public string PickupId => pickupId;
    public int Value => value;
    public bool IsCollected { get; private set; }

    private LevelRoot levelRoot;

    private void Awake()
    {
        levelRoot = GetComponentInParent<LevelRoot>();

        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[Collectible] '{name}': Collider2D must be a trigger. Setting isTrigger = true.", this);
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected) return;
        if (other.GetComponentInParent<PlayerMovement>() == null) return;

        Collect();
    }

    public void Collect()
    {
        if (IsCollected) return;
        IsCollected = true;
        gameObject.SetActive(false);
        levelRoot?.NotifyCollectibleCollected(this);
    }

    /// <summary>Restores the pickup for a level reset.</summary>
    public void ResetCollectible()
    {
        IsCollected = false;
        gameObject.SetActive(true);
    }
}
