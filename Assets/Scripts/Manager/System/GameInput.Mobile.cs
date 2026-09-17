using System.Collections.Generic;
using UnityEngine;

public enum MobileInputAction
{
    Jump, Dash, Attack, MidRange, Bow, Magic, Skill, Special,
    Interact, Confirm, Extract, Inventory, Map, Cancel, Heal, Menu,
    Tool1, Tool2, Tool3, Hotbar1, Hotbar2, Hotbar3, Hotbar4, CycleNext, CyclePrevious
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
        if (IsMobilePressed(MobileInputAction.Extract)) OnExtractPressed?.Invoke();
        if (IsMobilePressed(MobileInputAction.Inventory)) OnInventoryPressed?.Invoke();
        if (IsMobilePressed(MobileInputAction.Map)) OnMapTogglePressed?.Invoke();
        if (IsMobilePressed(MobileInputAction.Cancel)) OnCancelPressed?.Invoke();
        if (IsMobilePressed(MobileInputAction.Heal)) OnHealPressed?.Invoke();
        if (IsMobilePressed(MobileInputAction.CycleNext)) OnCycleNextPressed?.Invoke();
        if (IsMobilePressed(MobileInputAction.CyclePrevious)) OnCyclePrevPressed?.Invoke();
        for (int i = 0; i < 3; i++)
            if (IsMobilePressed((MobileInputAction)((int)MobileInputAction.Tool1 + i))) OnToolKeyPressed?.Invoke(i);
        for (int i = 0; i < 4; i++)
            if (IsMobilePressed((MobileInputAction)((int)MobileInputAction.Hotbar1 + i))) OnHotbarKeyPressed?.Invoke(i);
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
    }

    private void OnDisable() => ClearMobileInput();
    private void OnApplicationFocus(bool focused) { if (!focused) ClearMobileInput(); }
    private void OnApplicationPause(bool paused) { if (paused) ClearMobileInput(); }
}
