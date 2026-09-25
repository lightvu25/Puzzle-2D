using UnityEngine;
using HouseFlow.Fluid;

namespace HouseFlow.Level
{
    /// <summary>
    /// Trigger volume placed at the bounds of a level (typically below the
    /// floor) that removes any gameplay object which escapes the play area.
    ///
    /// Objects that belong to an ObjectPoolManager pool are returned to the
    /// pool; anything else is destroyed. FluidParticle gets the fast path via
    /// its own ReturnToPool(), which also resets physics/thermal state.
    ///
    /// Usage: add to a child GameObject of a Layout Prefab with a trigger
    /// Collider2D sized to cover the level's lower boundary.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeadZone : MonoBehaviour
    {
        [Tooltip("Only objects on these layers are removed. Defaults to everything.")]
        [SerializeField] private LayerMask affectedLayers = ~0;

        [Tooltip("If true, the Rigidbody2D's whole hierarchy is removed (catches trigger colliders on child objects).")]
        [SerializeField] private bool removeAttachedRigidbodyRoot = true;

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (!col.isTrigger)
            {
                Debug.LogWarning($"[DeadZone] '{name}': Collider2D is not a trigger. Setting isTrigger = true.", this);
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            GameObject target = other.gameObject;

            // If the collider lives on a child of a moving body, remove the body root.
            if (removeAttachedRigidbodyRoot && other.attachedRigidbody != null)
                target = other.attachedRigidbody.gameObject;

            if ((affectedLayers.value & (1 << target.layer)) == 0) return;

            var particle = target.GetComponent<FluidParticle>();
            if (particle != null)
            {
                particle.ReturnToPool();
                return;
            }

            // Pooled objects are deactivated for reuse; non-pooled strays are destroyed.
            ObjectPoolManager.ReturnObjectToPool(target);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.35f);
            if (GetComponent<Collider2D>() is BoxCollider2D box)
            {
                Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
            }
        }
    }
}
