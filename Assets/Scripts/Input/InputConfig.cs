using UnityEngine;

/// <summary>
/// ScriptableObject holding input configuration: which keyboard scheme is
/// active and the key bindings for general UI actions.
///
/// Create via: Assets → Create → Data → Input Config
/// </summary>
[CreateAssetMenu(menuName = "Data/Input Config")]
public class InputConfig : ScriptableObject
{
    public enum ControlScheme
    {
        WASD,    // WASD movement keys
        Arrows   // Arrow-key movement
    }

    [Header("Active Scheme")]
    [SerializeField] private ControlScheme activeScheme = ControlScheme.WASD;

    public ControlScheme ActiveScheme => activeScheme;

    [Header("General Input")]
    [Tooltip("UI confirmation key used by confirmation prompts.")]
    [SerializeField] private KeyCode confirmKey = KeyCode.Space;
    public KeyCode ConfirmKey => confirmKey;

    [SerializeField] private KeyCode interactKey = KeyCode.F;
    public KeyCode InteractKey => interactKey;

    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
    public KeyCode CancelKey => cancelKey;

    [Header("Swipe")]
    [Tooltip("Minimum pointer drag distance in pixels to count as a swipe.")]
    [SerializeField, Min(10f)] private float minSwipePixels = 60f;
    public float MinSwipePixels => minSwipePixels;

    // ── Movement ──────────────────────────────────────────────────────────

    public float GetHorizontalInput() => GameInput.Instance != null ? GameInput.Instance.MovementInput.x : 0f;
    public float GetVerticalInput()   => GameInput.Instance != null ? GameInput.Instance.MovementInput.y : 0f;

    public Vector2 GetMovementInput()
    {
        return new Vector2(GetHorizontalInput(), GetVerticalInput());
    }

    // ── General ───────────────────────────────────────────────────────────

    public bool GetInteractDown() => GameInput.WasKeyPressed(interactKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Interact));
    public bool GetConfirmDown()  => GameInput.WasKeyPressed(confirmKey)  || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Confirm));
    public bool GetCancelDown()   => GameInput.WasKeyPressed(cancelKey)   || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Cancel));
    public bool GetPauseDown()    => GameInput.Instance != null && GameInput.Instance.IsPauseActionPressed();

    public void ToggleScheme()
    {
        activeScheme = activeScheme == ControlScheme.WASD
            ? ControlScheme.Arrows
            : ControlScheme.WASD;
    }
}
