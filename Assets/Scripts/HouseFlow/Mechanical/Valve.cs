using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using HouseFlow.Fluid;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// A tap-to-toggle valve that opens or closes connected WaterSources.
    ///
    /// When tapped, the valve rotates using DOTween and toggles emission on its
    /// controlled WaterSources. All gameplay values are configurable from the Inspector.
    ///
    /// Designer Workflow:
    ///   Place a Valve in the Layout Prefab.
    ///   Assign one or more WaterSources in the controlledSources list.
    ///   Configure open/closed rotation, animation duration, and starting state.
    /// </summary>
    public class Valve : MonoBehaviour, IInteractable
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Controlled Sources")]
        [Tooltip("The WaterSources that this valve opens and closes.")]
        [SerializeField] private WaterSource[] controlledSources;

        [Tooltip("Optional: Fans or AirflowSources this valve toggles.")]
        [SerializeField] private AirflowSource[] controlledFans;

        [Header("State")]
        [Tooltip("Whether the valve starts in the open (emitting) state.")]
        [SerializeField] private bool startsOpen = false;

        [Header("Animation")]
        [Tooltip("How many degrees the valve rotates when toggled (applied to the z-axis locally).")]
        [SerializeField] private float rotationAmount = 90f;

        [Tooltip("Duration of the rotation animation in seconds.")]
        [SerializeField, Min(0f)] private float animationDuration = 0.3f;

        [Tooltip("DOTween ease type for the toggle animation.")]
        [SerializeField] private Ease animationEase = Ease.OutQuad;

        [Header("Interaction")]
        [Tooltip("Whether the player can currently interact with this valve. Set to false to lock it.")]
        [SerializeField] private bool interactable = true;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private bool    isOpen;
        private bool    isAnimating;
        private float   closedZRotation;
        private float   openZRotation;
        private Tweener activeTween;
        private float   initialRotX;
        private float   initialRotY;

        // ─────────────────────────────────────────────────────────────
        //  IInteractable
        // ─────────────────────────────────────────────────────────────

        public bool IsInteractable => interactable && !isAnimating;

        public void Interact()
        {
            if (!IsInteractable) return;
            Toggle();
        }

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            initialRotX     = transform.localEulerAngles.x;
            initialRotY     = transform.localEulerAngles.y;
            closedZRotation = transform.localEulerAngles.z;
            openZRotation   = closedZRotation + rotationAmount;

            isOpen = startsOpen;
            ApplyStateImmediate(isOpen);
        }

        private void OnDestroy()
        {
            activeTween?.Kill();
        }

        // ─────────────────────────────────────────────────────────────
        //  Valve Logic
        // ─────────────────────────────────────────────────────────────

        private void Toggle()
        {
            isOpen = !isOpen;
            AnimateToState(isOpen);
            ApplySourceState(isOpen);
        }

        private void ApplyStateImmediate(bool open)
        {
            float targetZ = open ? openZRotation : closedZRotation;
            transform.localEulerAngles = new Vector3(initialRotX, initialRotY, targetZ);

            ApplySourceState(open);
        }

        private void AnimateToState(bool open)
        {
            activeTween?.Kill();

            float targetZ = open ? openZRotation : closedZRotation;

            isAnimating = true;
            activeTween = transform
                .DOLocalRotate(new Vector3(initialRotX, initialRotY, targetZ), animationDuration)
                .SetEase(animationEase)
                .OnComplete(() => isAnimating = false);
        }

        private void ApplySourceState(bool open)
        {
            if (controlledSources != null)
            {
                foreach (var source in controlledSources)
                {
                    if (source != null)
                        source.SetEmissionEnabled(open);
                }
            }

            if (controlledFans != null)
            {
                foreach (var fan in controlledFans)
                {
                    if (fan != null)
                        fan.SetActive(open);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        /// <summary>Restores the valve to its initial state. Called by LevelRoot.ResetAll().</summary>
        public void ResetValve()
        {
            activeTween?.Kill();
            isAnimating = false;
            isOpen      = startsOpen;
            ApplyStateImmediate(isOpen);
        }

        // ─────────────────────────────────────────────────────────────
        //  Public State Query
        // ─────────────────────────────────────────────────────────────

        public bool IsOpen => isOpen;

        // ─────────────────────────────────────────────────────────────
        //  Editor Gizmos
        // ─────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (controlledSources != null)
            {
                foreach (var src in controlledSources)
                {
                    if (src != null)
                        Gizmos.DrawLine(transform.position, src.transform.position);
                }
            }

            Gizmos.color = Color.cyan;
            if (controlledFans != null)
            {
                foreach (var fan in controlledFans)
                {
                    if (fan != null)
                        Gizmos.DrawLine(transform.position, fan.transform.position);
                }
            }
        }
    }
}
