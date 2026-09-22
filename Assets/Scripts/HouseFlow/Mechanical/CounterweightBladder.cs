using UnityEngine;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// A constrained inflatable bladder that applies force along a specific axis when its
    /// linked airflow source is active. Used to actuate mechanical arms and counterweights.
    ///
    /// GDD Reference (Section 6.3): "Inflatable counterweight bladders"
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CounterweightBladder : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Airflow Link")]
        [Tooltip("The airflow provider (Fan or Duct) that inflates this bladder.")]
        [SerializeField] private MonoBehaviour linkedAirflowSourceRef;
        private IAirflowProvider linkedAirflowSource => linkedAirflowSourceRef as IAirflowProvider;

        [Header("Inflation Force")]
        [Tooltip("The maximum force applied when fully inflating.")]
        [SerializeField] private float maxInflationForce = 50f;

        [Tooltip("The local axis along which the bladder expands/applies force.")]
        [SerializeField] private Vector2 inflationAxis = Vector2.up;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private Rigidbody2D rb;
        private bool        isInflating;
        private Vector2     startPosition;
        private float       startRotation;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            rb            = GetComponent<Rigidbody2D>();
            startPosition = rb.position;
            startRotation = rb.rotation;

            if (linkedAirflowSource != null)
            {
                linkedAirflowSource.OnFlowChanged += HandleAirflowChanged;
                isInflating = linkedAirflowSource.IsAirflowActive;
            }
            
            // Only run FixedUpdate if inflating
            enabled = isInflating;
        }

        private void OnDestroy()
        {
            if (linkedAirflowSource != null)
            {
                linkedAirflowSource.OnFlowChanged -= HandleAirflowChanged;
            }
        }

        private void FixedUpdate()
        {
            if (isInflating)
            {
                // Apply force along the configured local axis
                Vector2 worldAxis = transform.TransformDirection(inflationAxis);
                rb.AddForce(worldAxis * maxInflationForce);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Event Handlers
        // ─────────────────────────────────────────────────────────────

        private void HandleAirflowChanged()
        {
            isInflating = linkedAirflowSource != null && linkedAirflowSource.IsAirflowActive;
            enabled     = isInflating;
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        /// <summary>Restores the bladder to its initial position and state. Called by LevelRoot.</summary>
        public void ResetBladder()
        {
            rb.position        = startPosition;
            rb.rotation        = startRotation;
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;

            if (linkedAirflowSource != null)
            {
                isInflating = linkedAirflowSource.IsAirflowActive;
                enabled     = isInflating;
            }
        }
    }
}
