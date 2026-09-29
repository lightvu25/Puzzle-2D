using UnityEngine;

/// <summary>
/// Trigger volume marking the exit of a maze level. When the player slides
/// into it, the parent <see cref="LevelRoot"/> is notified and the
/// ReachExit objective completes.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [Tooltip("Optional id if a layout ever needs multiple exits.")]
    [SerializeField] private string exitId;

    public string ExitId => exitId;

    private LevelRoot levelRoot;
    private bool triggered;

    private void Awake()
    {
        levelRoot = GetComponentInParent<LevelRoot>();

        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[LevelExit] '{name}': Collider2D must be a trigger. Setting isTrigger = true.", this);
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.GetComponentInParent<PlayerMovement>() == null) return;

        triggered = true;
        levelRoot?.NotifyExitReached(this);
    }

    /// <summary>Re-arms the exit for a level reset.</summary>
    public void ResetExit()
    {
        triggered = false;
    }
}
