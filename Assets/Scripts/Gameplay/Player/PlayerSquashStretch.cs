using DG.Tweening;
using UnityEngine;

/// <summary>
/// Subtle squash &amp; stretch juice for the player sprite.
///
/// The deformation is applied ONLY to the visual child Transform (the
/// SpriteRenderer object) — never to the Player root — so the Rigidbody2D,
/// Collider2D and all physics behaviour stay untouched.
///
/// Event-driven: it listens to PlayerMovement.OnMoveStarted / OnMoveStopped,
/// so no Update/LateUpdate work happens when nothing is moving.
/// DOTween's DOScale is used instead of writing localScale per frame so the
/// easing, duration and cleanup are handled by the tween engine.
/// </summary>
public class PlayerSquashStretch : MonoBehaviour
{
    [Header("Visual Target")]
    [Tooltip("The visual child that gets scaled — NOT the Player root, so physics stay unaffected.")]
    [SerializeField] private Transform visualTransform;

    [Header("GDD Forms (pixels @ 32 PPU)")]
    [Tooltip("Standard moving form — GDD: 20 px wide.")]
    [SerializeField] private float movingWidthPx = 20f;
    [Tooltip("Standard moving form — GDD: 28 px tall.")]
    [SerializeField] private float movingHeightPx = 28f;
    [Tooltip("Wall impact form — GDD: 28 px wide.")]
    [SerializeField] private float impactWidthPx = 28f;
    [Tooltip("Wall impact form — GDD: 16 px tall.")]
    [SerializeField] private float impactHeightPx = 16f;

    [Header("Amounts (fraction of base scale — keep small for pixel-art)")]
    [Tooltip("Stretch along the slide direction when a slide starts.")]
    [SerializeField, Range(0f, 0.3f)] private float stretchAmount = 0.12f;

    [Header("Timing")]
    [SerializeField, Min(0.05f)] private float squashDuration = 0.22f;
    [SerializeField, Min(0.05f)] private float stretchDuration = 0.18f;

    [Tooltip("Ease used when returning to the base scale.")]
    [SerializeField] private Ease returnEase = Ease.OutQuad;

    [Header("Safety")]
    [Tooltip("Hard cap on total deformation so the sprite never distorts badly. Must cover the GDD impact ratio (~0.43).")]
    [SerializeField, Range(0f, 0.6f)] private float maxDeformation = 0.45f;

    private PlayerMovement movement;
    private Vector3 baseScale;
    private Vector2 lastDirection = Vector2.right;
    private Sequence activeSequence;

    // ---------------------------------------------------------------

    private void Awake()
    {
        // Auto-resolve the visual child (a SpriteRenderer below the root)
        // when the field isn't wired in the Inspector. Never accept the root
        // itself or any transform carrying a collider — scaling physics
        // geometry wedges the player into wall seams.
        if (visualTransform == null
            || visualTransform == transform
            || visualTransform.GetComponent<Collider2D>() != null)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            visualTransform = sr != null && sr.transform != transform ? sr.transform : null;
        }

        if (visualTransform == null)
        {
            Debug.LogWarning("[PlayerSquashStretch] No safe visual child found (needs a SpriteRenderer that isn't the physics root) — component disabled.", this);
            enabled = false;
            return;
        }

        baseScale = visualTransform.localScale;
    }

    private void OnEnable()
    {
        movement = GetComponentInParent<PlayerMovement>();
        if (movement == null) return;
        movement.OnMoveStarted += HandleMoveStarted;
        movement.OnMoveStopped += HandleMoveStopped;
    }

    private void OnDisable()
    {
        if (movement != null)
        {
            movement.OnMoveStarted -= HandleMoveStarted;
            movement.OnMoveStopped -= HandleMoveStopped;
        }
        KillActive();
        if (visualTransform != null) visualTransform.localScale = baseScale;
    }

    // ---------------------------------------------------------------
    //  Event handlers — the only triggers, no per-frame work.
    // ---------------------------------------------------------------

    private void HandleMoveStarted(Vector2 direction)
    {
        lastDirection = direction;
        Stretch(direction);   // take-off / direction change: elongate along motion
    }

    private void HandleMoveStopped()
    {
        Squash(lastDirection); // landing: compress along motion, widen laterally
    }

    // ---------------------------------------------------------------
    //  Public API
    // ---------------------------------------------------------------

    /// <summary>Wall-impact squash using the last known slide direction.</summary>
    public void Squash() => Squash(lastDirection);

    /// <summary>
    /// Wall-impact pose: GDD spec is 28 px wide × 16 px tall versus the
    /// 20 × 28 moving form — the sprite goes wide and flat for a beat before
    /// snapping back. Applied direction-independent so vertical and
    /// horizontal impacts read identically.
    /// </summary>
    public void Squash(Vector2 direction)
    {
        float sx = Mathf.Clamp(impactWidthPx / Mathf.Max(1f, movingWidthPx) - 1f, -maxDeformation, maxDeformation);
        float sy = Mathf.Clamp(impactHeightPx / Mathf.Max(1f, movingHeightPx) - 1f, -maxDeformation, maxDeformation);
        Vector3 target = new Vector3(baseScale.x * (1f + sx), baseScale.y * (1f + sy), baseScale.z);
        Play(target, squashDuration);
    }

    /// <summary>Slide-start stretch using the last known direction.</summary>
    public void Stretch() => Stretch(lastDirection);

    /// <summary>Elongate along <paramref name="direction"/>, shrink slightly across — the "take-off" look.</summary>
    public void Stretch(Vector2 direction)
    {
        Play(Deform(direction, +stretchAmount, -stretchAmount * 0.5f), stretchDuration);
    }

    /// <summary>Tween the visual back to its untouched base scale.</summary>
    public void ResetScale()
    {
        if (visualTransform == null) return;
        KillActive();
        activeSequence = DOTween.Sequence()
            .Append(visualTransform.DOScale(baseScale, squashDuration * 0.5f).SetEase(returnEase));
    }

    // ---------------------------------------------------------------
    //  Internals
    // ---------------------------------------------------------------

    /// <summary>Scale target: <paramref name="along"/> along the motion axis, <paramref name="across"/> perpendicular.</summary>
    private Vector3 Deform(Vector2 direction, float along, float across)
    {
        float a = Mathf.Clamp(along, -maxDeformation, maxDeformation);
        float c = Mathf.Clamp(across, -maxDeformation, maxDeformation);
        bool horizontal = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);

        float sx = horizontal ? a : c;
        float sy = horizontal ? c : a;
        return new Vector3(baseScale.x * (1f + sx), baseScale.y * (1f + sy), baseScale.z);
    }

    /// <summary>Two-step tween: deform fast, then ease back to the base scale.</summary>
    private void Play(Vector3 deformed, float duration)
    {
        if (visualTransform == null) return;

        // Killing the previous sequence prevents tweens from stacking up and
        // fighting each other during rapid direction changes.
        KillActive();
        activeSequence = DOTween.Sequence()
            .Append(visualTransform.DOScale(deformed, duration * 0.4f).SetEase(Ease.OutQuad))
            .Append(visualTransform.DOScale(baseScale, duration * 0.6f).SetEase(returnEase));
    }

    private void KillActive()
    {
        if (activeSequence != null && activeSequence.IsActive())
            activeSequence.Kill();
        activeSequence = null;
    }
}
