using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum MobileInputAction
{
    Interact, Confirm, Cancel, Menu
}

public partial class GameInput
{
    private readonly Dictionary<int, MobileInputAction> mobileButtons = new Dictionary<int, MobileInputAction>();
    private readonly Dictionary<int, Vector2> mobileSticks = new Dictionary<int, Vector2>();
    private readonly HashSet<MobileInputAction> pendingPresses = new HashSet<MobileInputAction>();
    private readonly HashSet<MobileInputAction> pendingReleases = new HashSet<MobileInputAction>();
    private readonly HashSet<MobileInputAction> mobilePressed = new HashSet<MobileInputAction>();
    private readonly HashSet<MobileInputAction> mobileReleased = new HashSet<MobileInputAction>();
    private readonly HashSet<MobileInputAction> mobileHeld = new HashSet<MobileInputAction>();
    public Vector2 MobileMovement { get; private set; }

    public Vector2 MovementInput
    {
        get
        {
            if (!isActiveAndEnabled || inputActions == null) return Vector2.zero;
            float x = (inputActions.Player.PlayerRight.IsPressed() ? 1f : 0f)
                - (inputActions.Player.PlayerLeft.IsPressed() ? 1f : 0f);
            float y = (inputActions.Player.PlayerUp.IsPressed() ? 1f : 0f)
                - (inputActions.Player.PlayerDown.IsPressed() ? 1f : 0f);
            return new Vector2(Mathf.Clamp(x + MobileMovement.x, -1f, 1f),
                Mathf.Clamp(y + MobileMovement.y, -1f, 1f));
        }
    }

    public void SetMobileMovement(int source, Vector2 value)
    {
        if (!isActiveAndEnabled) return;
        mobileSticks[source] = Vector2.ClampMagnitude(value, 1f);
    }

    public void SetMobileButton(int source, MobileInputAction action, bool held)
    {
        if (held && mobileButtons.TryGetValue(source, out MobileInputAction current) && current == action) return;
        ReleaseMobileButton(source);
        if (!held || !isActiveAndEnabled) return;
        bool alreadyHeld = mobileButtons.ContainsValue(action);
        mobileButtons[source] = action;
        if (!alreadyHeld) pendingPresses.Add(action);
    }

    private void ReleaseMobileButton(int source)
    {
        if (!mobileButtons.TryGetValue(source, out MobileInputAction action)) return;
        mobileButtons.Remove(source);
        if (!mobileButtons.ContainsValue(action)) pendingReleases.Add(action);
    }

    public void ReleaseMobileSource(int source)
    {
        ReleaseMobileButton(source);
        mobileSticks.Remove(source);
    }

    public bool IsMobilePressed(MobileInputAction action) => isActiveAndEnabled && mobilePressed.Contains(action);
    public bool IsMobileHeld(MobileInputAction action) => isActiveAndEnabled && mobileHeld.Contains(action);
    public bool IsMobileReleased(MobileInputAction action) => isActiveAndEnabled && mobileReleased.Contains(action);

    // UI callbacks are collected, then published before gameplay Update. Every consumer
    // sees the same edges, including a quick press and release between two updates.
    private void UpdateMobileInput()
    {
        mobilePressed.Clear();
        mobileReleased.Clear();
        mobileHeld.Clear();
        mobilePressed.UnionWith(pendingPresses);
        mobileReleased.UnionWith(pendingReleases);
        pendingPresses.Clear();
        pendingReleases.Clear();
        foreach (MobileInputAction action in mobileButtons.Values) mobileHeld.Add(action);
        Vector2 movement = Vector2.zero;
        foreach (Vector2 value in mobileSticks.Values) movement += value;
        MobileMovement = Vector2.ClampMagnitude(movement, 1f);

        // Dispatch from a fixed list: an event listener can disable input and clear state.
        if (IsMobilePressed(MobileInputAction.Menu)) OnMenuButtonPressed?.Invoke(this, System.EventArgs.Empty);
        if (IsMobilePressed(MobileInputAction.Interact)) OnInteractPressed?.Invoke();
        if (IsMobilePressed(MobileInputAction.Confirm)) OnConfirmPressed?.Invoke();
        if (IsMobilePressed(MobileInputAction.Cancel)) OnCancelPressed?.Invoke();
    }

    private void ClearMobileInput()
    {
        mobileButtons.Clear();
        mobileSticks.Clear();
        pendingPresses.Clear();
        pendingReleases.Clear();
        mobilePressed.Clear();
        mobileReleased.Clear();
        mobileHeld.Clear();
        MobileMovement = Vector2.zero;
        ClearSwipe();
    }

    private void OnDisable() => ClearMobileInput();
    private void OnApplicationFocus(bool focused) { if (!focused) ClearMobileInput(); }
    private void OnApplicationPause(bool paused) { if (paused) ClearMobileInput(); }

    // ── Swipe detection ────────────────────────────────────────────────────
    // A swipe starts on pointer/touch press and completes once the drag
    // distance exceeds MinSwipeDistance. A new press is required per swipe.
    // Swipes that begin over UI elements are ignored.

    private bool swipeActive;
    private bool swipeFromUI;
    private Vector2 swipeStartPos;

    private void UpdateSwipeInput()
    {
        if (!isActiveAndEnabled)
        {
            ClearSwipe();
            return;
        }

        var pointer = Pointer.current;
        if (pointer == null) return;

        if (pointer.press.wasPressedThisFrame)
        {
            swipeActive = true;
            swipeStartPos = pointer.position.ReadValue();
            swipeFromUI = IsPointerOverUI(swipeStartPos);
        }
        else if (swipeActive && pointer.press.isPressed && !swipeFromUI)
        {
            Vector2 delta = pointer.position.ReadValue() - swipeStartPos;
            if (delta.magnitude >= MinSwipeDistance)
            {
                OnDirectionPressed?.Invoke(DominantDirection(delta));
                swipeActive = false; // require a fresh press for the next swipe
            }
        }
        else if (swipeActive && !pointer.press.isPressed)
        {
            swipeActive = false;
        }
    }

    private void ClearSwipe()
    {
        swipeActive = false;
        swipeFromUI = false;
    }

    private static Vector2 DominantDirection(Vector2 delta)
    {
        return Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
            ? new Vector2(Mathf.Sign(delta.x), 0f)
            : new Vector2(0f, Mathf.Sign(delta.y));
    }

    private static bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;
        var eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
