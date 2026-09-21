using UnityEngine;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// Represents an airflow source (like a fan or vent) that pushes fluids in a specific direction.
    /// Acts as a clean wrapper around Unity's highly optimized AreaEffector2D.
    /// </summary>
    [RequireComponent(typeof(AreaEffector2D))]
    [RequireComponent(typeof(Collider2D))]
    public class AirflowSource : MonoBehaviour
    {
        [Header("Airflow Settings")]
        [Tooltip("Should this airflow start active?")]
        [SerializeField] private bool startsActive = true;

        private AreaEffector2D effector;
        private Collider2D col;

        public bool IsActive => effector != null && effector.enabled;

        private void Awake()
        {
            effector = GetComponent<AreaEffector2D>();
            col = GetComponent<Collider2D>();

            // Ensure the collider is set up properly for the effector
            col.isTrigger = true;
            col.usedByEffector = true;

            SetActive(startsActive);
        }

        public void SetActive(bool active)
        {
            if (effector != null) effector.enabled = active;
        }

        public void ResetSource()
        {
            SetActive(startsActive);
        }
    }
}
