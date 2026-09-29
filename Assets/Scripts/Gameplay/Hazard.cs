using UnityEngine;

/// <summary>
/// Trigger volume that kills the player on contact (spikes, enemies, traps).
/// Reports to the parent <see cref="LevelRoot"/> which fails the level.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    [Tooltip("Reason shown on the fail screen.")]
    [SerializeField] private string failReason = "You hit a hazard!";

    public string FailReason => failReason;

    private LevelRoot levelRoot;

    private void Awake()
    {
        levelRoot = GetComponentInParent<LevelRoot>();

        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[Hazard] '{name}': Collider2D must be a trigger. Setting isTrigger = true.", this);
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        levelRoot?.NotifyHazardTriggered(this);
    }
}
