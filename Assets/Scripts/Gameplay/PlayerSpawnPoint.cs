using UnityEngine;

/// <summary>
/// Marker component placed inside a maze layout to define where the player
/// spawns when the level starts or resets.
/// </summary>
public class PlayerSpawnPoint : MonoBehaviour
{
    public Vector3 Position => transform.position;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position + Vector3.left * 0.4f, transform.position + Vector3.right * 0.4f);
        Gizmos.DrawLine(transform.position + Vector3.down * 0.4f, transform.position + Vector3.up * 0.4f);
    }
}
