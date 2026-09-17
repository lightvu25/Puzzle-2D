using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public partial class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnMenuButtonPressed;

    public event Action<int> OnHotbarKeyPressed;
    public event Action OnCycleNextPressed;
    public event Action OnCyclePrevPressed;

    public event Action OnMapTogglePressed;
    public event Action OnInventoryPressed;
    public event Action OnInteractPressed;
    public event Action OnConfirmPressed;
    public event Action OnExtractPressed;
    public event Action OnCancelPressed;
    public event Action OnHealPressed;
    public event Action<int> OnToolKeyPressed;

    [Header("Input Configuration")]
    [SerializeField] private InputConfig inputConfig;

    private InputActions inputActions;
    private InputConfig.ControlScheme lastScheme;

    public KeyCode InteractKey => inputConfig != null ? inputConfig.InteractKey : KeyCode.F;
    public KeyCode ConfirmKey => inputConfig != null ? inputConfig.ConfirmKey : KeyCode.Space;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new InputActions();
        inputActions.Enable();

        inputActions.Player.Menu.performed += Menu_performed;
    }

    private void Start()
    {
        if (inputConfig != null)
        {
            lastScheme = inputConfig.ActiveScheme;
            ApplyBindings();
        }
    }

    private void Update()
    {
        UpdateMobileInput();
        if (!isActiveAndEnabled) return;
        if (inputConfig != null && inputConfig.ActiveScheme != lastScheme)
        {
            lastScheme = inputConfig.ActiveScheme;
            ApplyBindings();
        }

        // Hotbar & Cycle Inputs
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnHotbarKeyPressed?.Invoke(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnHotbarKeyPressed?.Invoke(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnHotbarKeyPressed?.Invoke(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) OnHotbarKeyPressed?.Invoke(3);
        
        if (Input.GetKeyDown(KeyCode.Q)) OnCyclePrevPressed?.Invoke();
        
        KeyCode mapKey = inputConfig != null ? inputConfig.MapKey : KeyCode.M;
        KeyCode invKey = inputConfig != null ? inputConfig.InventoryKey : KeyCode.E;
        KeyCode interactKey = InteractKey;
        KeyCode confirmKey = ConfirmKey;
        KeyCode extractKey = inputConfig != null ? inputConfig.ExtractKey : KeyCode.R;
        KeyCode cancelKey = inputConfig != null ? inputConfig.CancelKey : KeyCode.Escape;
        KeyCode healKey = inputConfig != null ? inputConfig.HealKey : KeyCode.H;
        KeyCode tool1Key = inputConfig != null ? inputConfig.Tool1Key : KeyCode.T;
        KeyCode tool2Key = inputConfig != null ? inputConfig.Tool2Key : KeyCode.Y;
        KeyCode tool3Key = inputConfig != null ? inputConfig.Tool3Key : KeyCode.G;

        if (Input.GetKeyDown(mapKey)) OnMapTogglePressed?.Invoke();
        if (Input.GetKeyDown(invKey)) OnInventoryPressed?.Invoke();

        if (Input.GetKeyDown(interactKey)) OnInteractPressed?.Invoke();
        if (Input.GetKeyDown(confirmKey)) OnConfirmPressed?.Invoke();
        if (Input.GetKeyDown(extractKey)) OnExtractPressed?.Invoke();
        if (Input.GetKeyDown(cancelKey)) OnCancelPressed?.Invoke();
        if (Input.GetKeyDown(healKey)) OnHealPressed?.Invoke();
        if (Input.GetKeyDown(tool1Key)) OnToolKeyPressed?.Invoke(0);
        if (Input.GetKeyDown(tool2Key)) OnToolKeyPressed?.Invoke(1);
        if (Input.GetKeyDown(tool3Key)) OnToolKeyPressed?.Invoke(2);
    }

    private void ApplyBindings()
    {
        if (inputConfig == null) return;

        bool isWASD = inputConfig.ActiveScheme == InputConfig.ControlScheme.WASD_JUK;

        ApplyBinding(inputActions.Player.PlayerLeft, isWASD ? "<Keyboard>/a" : "<Keyboard>/leftArrow");
        ApplyBinding(inputActions.Player.PlayerRight, isWASD ? "<Keyboard>/d" : "<Keyboard>/rightArrow");
        ApplyBinding(inputActions.Player.PlayerUp, isWASD ? "<Keyboard>/w" : "<Keyboard>/upArrow");
        ApplyBinding(inputActions.Player.PlayerDown, isWASD ? "<Keyboard>/s" : "<Keyboard>/downArrow");

        ApplyBinding(inputActions.Player.Attack, GetInputPath(inputConfig.AttackKey));
        ApplyBinding(inputActions.Player.Skill, GetInputPath(inputConfig.SkillKey));
        ApplyBinding(inputActions.Player.Special, GetInputPath(inputConfig.SpecialKey));
        ApplyBinding(inputActions.Player.PlayerDash, GetInputPath(inputConfig.DashKey));
        ApplyBinding(inputActions.Player.PlayerJump, GetInputPath(inputConfig.JumpKey));
        
    }

    private void ApplyBinding(InputAction action, string path)
    {
        action.ApplyBindingOverride(new InputBinding { overridePath = path });
    }

    private string GetInputPath(KeyCode key)
    {
        return key switch
        {
            KeyCode.A => "<Keyboard>/a", KeyCode.B => "<Keyboard>/b", KeyCode.C => "<Keyboard>/c",
            KeyCode.D => "<Keyboard>/d", KeyCode.E => "<Keyboard>/e", KeyCode.F => "<Keyboard>/f",
            KeyCode.G => "<Keyboard>/g", KeyCode.H => "<Keyboard>/h", KeyCode.I => "<Keyboard>/i",
            KeyCode.J => "<Keyboard>/j", KeyCode.K => "<Keyboard>/k", KeyCode.L => "<Keyboard>/l",
            KeyCode.M => "<Keyboard>/m", KeyCode.N => "<Keyboard>/n", KeyCode.O => "<Keyboard>/o",
            KeyCode.P => "<Keyboard>/p", KeyCode.Q => "<Keyboard>/q", KeyCode.R => "<Keyboard>/r",
            KeyCode.S => "<Keyboard>/s", KeyCode.T => "<Keyboard>/t", KeyCode.U => "<Keyboard>/u",
            KeyCode.V => "<Keyboard>/v", KeyCode.W => "<Keyboard>/w", KeyCode.X => "<Keyboard>/x",
            KeyCode.Y => "<Keyboard>/y", KeyCode.Z => "<Keyboard>/z",
            
            KeyCode.UpArrow => "<Keyboard>/upArrow", KeyCode.DownArrow => "<Keyboard>/downArrow",
            KeyCode.LeftArrow => "<Keyboard>/leftArrow", KeyCode.RightArrow => "<Keyboard>/rightArrow",
            
            KeyCode.LeftShift => "<Keyboard>/leftShift", KeyCode.RightShift => "<Keyboard>/rightShift",
            KeyCode.LeftControl => "<Keyboard>/leftCtrl", KeyCode.RightControl => "<Keyboard>/rightCtrl",
            KeyCode.LeftAlt => "<Keyboard>/leftAlt", KeyCode.RightAlt => "<Keyboard>/rightAlt",
            
            KeyCode.Space => "<Keyboard>/space", KeyCode.Return => "<Keyboard>/enter",
            KeyCode.Escape => "<Keyboard>/escape", KeyCode.Tab => "<Keyboard>/tab",
            KeyCode.Backspace => "<Keyboard>/backspace",
            _ => "<Keyboard>/space" 
        };
    }

    private void Menu_performed(InputAction.CallbackContext obj) => OnMenuButtonPressed?.Invoke(this, EventArgs.Empty);
    private void OnDestroy() 
    {
        if (inputActions != null)
        {
            inputActions.Player.Menu.performed -= Menu_performed;
            inputActions.Disable();
        }

        if (Instance == this)
            Instance = null;
    }

    public void SetInputsEnabled(bool isEnabled)
    {
        if (isEnabled)
        {
            inputActions?.Enable();
            this.enabled = true;
        }
        else
        {
            inputActions?.Disable();
            this.enabled = false;
        }
    }

    public bool IsUpActionPressed() => MovementInput.y > 0.5f;
    public bool IsDownActionPressed() => MovementInput.y < -0.5f;
    public bool IsLeftActionPressed() => MovementInput.x < -0.5f;
    public bool IsRightActionPressed() => MovementInput.x > 0.5f;
    
    public bool IsJumpActionPressed() => isActiveAndEnabled && (inputActions.Player.PlayerJump.WasPressedThisFrame() || IsMobilePressed(MobileInputAction.Jump));
    public bool IsDashActionPressed() => isActiveAndEnabled && (inputActions.Player.PlayerDash.WasPressedThisFrame() || IsMobilePressed(MobileInputAction.Dash));
    public bool IsAttackActionPressed() => isActiveAndEnabled && (inputActions.Player.Attack.WasPressedThisFrame() || IsMobilePressed(MobileInputAction.Attack));
    public bool IsSkillActionPressed() => isActiveAndEnabled && (inputActions.Player.Skill.WasPressedThisFrame() || IsMobilePressed(MobileInputAction.Skill));
    public bool IsSpecialActionPressed() => isActiveAndEnabled && (inputActions.Player.Special.WasPressedThisFrame() || IsMobilePressed(MobileInputAction.Special));
    public bool IsInteractActionPressed() => isActiveAndEnabled && (Input.GetKeyDown(InteractKey) || IsMobilePressed(MobileInputAction.Interact));
    public bool IsConfirmActionPressed() => isActiveAndEnabled && (Input.GetKeyDown(ConfirmKey) || IsMobilePressed(MobileInputAction.Confirm));
    
    public bool IsJumpActionHeld() => isActiveAndEnabled && (inputActions.Player.PlayerJump.IsPressed() || IsMobileHeld(MobileInputAction.Jump));
    public bool IsJumpActionReleased() => isActiveAndEnabled && (inputActions.Player.PlayerJump.WasReleasedThisFrame() || IsMobileReleased(MobileInputAction.Jump));
    
    public bool IsAttackActionHeld() => isActiveAndEnabled && (inputActions.Player.Attack.IsPressed() || IsMobileHeld(MobileInputAction.Attack));
    public bool IsAttackActionReleased() => isActiveAndEnabled && (inputActions.Player.Attack.WasReleasedThisFrame() || IsMobileReleased(MobileInputAction.Attack));
    
    public bool IsPauseActionPressed() => isActiveAndEnabled && (inputActions.Player.Menu.IsPressed() || IsMobilePressed(MobileInputAction.Menu));
}
