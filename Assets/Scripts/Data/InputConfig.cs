using UnityEngine;

[CreateAssetMenu(menuName = "Data/Input Config")]
public class InputConfig : ScriptableObject
{
    public enum ControlScheme
    {
        WASD_JUK,    // Set 1: WASD movement, J/U/K combat, L dash
        Arrow_ZXC    // Set 2: Arrow movement, Z/X/C combat, L-Shift dash
    }

    [Header("Active Scheme")]
    [SerializeField] private ControlScheme activeScheme = ControlScheme.WASD_JUK;

    public ControlScheme ActiveScheme => activeScheme;

    // Combat Keys (Kept for Inspector configuration/UI display, but logic uses New Input System Bindings)  
    [Header("Scheme 1: WASD + JUK (Combat)")]
    [SerializeField] private KeyCode wasd_Attack = KeyCode.J;
    [SerializeField] private KeyCode wasd_Skill = KeyCode.U;
    [SerializeField] private KeyCode wasd_Special = KeyCode.K;
    [SerializeField] private KeyCode wasd_Dash = KeyCode.LeftShift;
    [SerializeField] private KeyCode wasd_Jump = KeyCode.W;

    [Header("Scheme 2: Arrow + ZXC (Combat)")]
    [SerializeField] private KeyCode arrow_Attack = KeyCode.Z;
    [SerializeField] private KeyCode arrow_Skill = KeyCode.X;
    [SerializeField] private KeyCode arrow_Special = KeyCode.C;
    [SerializeField] private KeyCode arrow_Dash = KeyCode.LeftShift;
    [SerializeField] private KeyCode arrow_Jump = KeyCode.UpArrow;

    // Combat Input Properties (Wrappers for UI/Display)  
    public KeyCode AttackKey => activeScheme == ControlScheme.WASD_JUK ? wasd_Attack : arrow_Attack;
    public KeyCode SkillKey => activeScheme == ControlScheme.WASD_JUK ? wasd_Skill : arrow_Skill;
    public KeyCode SpecialKey => activeScheme == ControlScheme.WASD_JUK ? wasd_Special : arrow_Special;
    public KeyCode DashKey => activeScheme == ControlScheme.WASD_JUK ? wasd_Dash : arrow_Dash;
    public KeyCode JumpKey => activeScheme == ControlScheme.WASD_JUK ? wasd_Jump : arrow_Jump;

    [Header("General Input")]
    [Tooltip("UI confirmation key used by Mind World nodes and other confirmation prompts.")]
    [SerializeField] private KeyCode confirmKey = KeyCode.Space;

    public KeyCode ConfirmKey => confirmKey;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    public KeyCode InteractKey => interactKey;
    [SerializeField] private KeyCode extractKey = KeyCode.R;

    public KeyCode ExtractKey => extractKey;
    [SerializeField] private KeyCode inventoryKey = KeyCode.E;

    public KeyCode InventoryKey => inventoryKey;
    [SerializeField] private KeyCode mapKey = KeyCode.M;

    public KeyCode MapKey => mapKey;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    public KeyCode CancelKey => cancelKey;
    [SerializeField] private KeyCode healKey = KeyCode.H;

    public KeyCode HealKey => healKey;
    
    [Header("Equipment/Tool Keys")]
    [SerializeField] private KeyCode tool1Key = KeyCode.T;

    public KeyCode Tool1Key => tool1Key;
    [SerializeField] private KeyCode tool2Key = KeyCode.Y;

    public KeyCode Tool2Key => tool2Key;
    [SerializeField] private KeyCode tool3Key = KeyCode.G;

    public KeyCode Tool3Key => tool3Key;

    [Header("Playstyle Keys")]
    [SerializeField] private KeyCode meleeKey = KeyCode.J;
    [SerializeField] private KeyCode midRangeKey = KeyCode.K;
    [SerializeField] private KeyCode longRangeKey = KeyCode.L;
    [SerializeField] private KeyCode magicKey = KeyCode.Semicolon;

    // Combat Input Methods
    public bool GetAttackDown() => GameInput.Instance != null && GameInput.Instance.IsAttackActionPressed();
    public bool GetAttackHeld() => GameInput.Instance != null && GameInput.Instance.IsAttackActionHeld();
    public bool GetAttackUp() => GameInput.Instance != null && GameInput.Instance.IsAttackActionReleased();
    
    public bool GetSkillDown() => GameInput.Instance != null && GameInput.Instance.IsSkillActionPressed();
    public bool GetSpecialDown() => GameInput.Instance != null && GameInput.Instance.IsSpecialActionPressed();

    // Playstyle Inputs
    public bool GetAttackMeleeDown() => Input.GetKeyDown(meleeKey) || GetAttackDown(); // Fallback to primary attack
    public bool GetAttackMeleeHeld() => Input.GetKey(meleeKey) || GetAttackHeld();
    
    public bool GetAttackMidDown() => Input.GetKeyDown(midRangeKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.MidRange));
    public bool GetAttackMidHeld() => Input.GetKey(midRangeKey) || (GameInput.Instance != null && GameInput.Instance.IsMobileHeld(MobileInputAction.MidRange));
    
    public bool GetAttackLongDown() => Input.GetKeyDown(longRangeKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Bow));
    public bool GetAttackLongHeld() => Input.GetKey(longRangeKey) || (GameInput.Instance != null && GameInput.Instance.IsMobileHeld(MobileInputAction.Bow));
    
    public bool GetAttackMagicDown() => Input.GetKeyDown(magicKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Magic));
    public bool GetAttackMagicHeld() => Input.GetKey(magicKey) || (GameInput.Instance != null && GameInput.Instance.IsMobileHeld(MobileInputAction.Magic));

    // Movement
    
    public float GetHorizontalInput() => GameInput.Instance != null ? GameInput.Instance.MovementInput.x : 0f;
    public float GetVerticalInput() => GameInput.Instance != null ? GameInput.Instance.MovementInput.y : 0f;

    public Vector2 GetMovementInput()
    {
        return new Vector2(GetHorizontalInput(), GetVerticalInput());
    }

    // Jump
    
    public bool GetJumpDown() => GameInput.Instance != null && GameInput.Instance.IsJumpActionPressed();
    public bool GetJumpHeld() => GameInput.Instance != null && GameInput.Instance.IsJumpActionHeld();
    public bool GetJumpUp() => GameInput.Instance != null && GameInput.Instance.IsJumpActionReleased();

    // Dash
    public bool GetDashDown() => GameInput.Instance != null && GameInput.Instance.IsDashActionPressed();

    // Pause/Menu  
    
    public bool GetPauseDown() => GameInput.Instance != null && GameInput.Instance.IsPauseActionPressed();

    // Utility  
    
    public void ToggleScheme()
    {
        activeScheme = activeScheme == ControlScheme.WASD_JUK 
            ? ControlScheme.Arrow_ZXC 
            : ControlScheme.WASD_JUK;
    }

    public bool GetInteractDown() => Input.GetKeyDown(interactKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Interact));
    public bool GetConfirmDown() => Input.GetKeyDown(confirmKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Confirm));

    public bool GetExtractDown() => Input.GetKeyDown(extractKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Extract));
    public bool GetInventoryDown() => Input.GetKeyDown(inventoryKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Inventory));
    public bool GetMapDown() => Input.GetKeyDown(mapKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Map));
    public bool GetCancelDown() => Input.GetKeyDown(cancelKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Cancel));
    public bool GetHealDown() => Input.GetKeyDown(healKey) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Heal));
    
    public bool GetToolDown(int index)
    {
        return index switch
        {
            0 => Input.GetKeyDown(tool1Key) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Tool1)),
            1 => Input.GetKeyDown(tool2Key) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Tool2)),
            2 => Input.GetKeyDown(tool3Key) || (GameInput.Instance != null && GameInput.Instance.IsMobilePressed(MobileInputAction.Tool3)),
            _ => false
        };
    }
}
