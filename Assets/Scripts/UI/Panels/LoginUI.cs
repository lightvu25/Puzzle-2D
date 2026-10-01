using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple email/password login panel — a test-friendly implementation of
/// the auth flow end to end:
///
///   Button click → client-side validation → IAuthService async call
///   → AuthResult → status text + view refresh.
///
/// Uses legacy uGUI (InputField/Text/Button) like the rest of the project's
/// UI. Not registered with UIManager — auth is infrastructure, not a
/// gameplay panel, so it manages itself.
/// </summary>
public class LoginUI : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField passwordInput;

    [Header("Buttons")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button logoutButton;

    [Header("Labels")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text userText;

    [Header("Shown only while signed out")]
    [SerializeField] private GameObject signInGroup;

    private bool busy;

    /// <summary>Shortcut to the auth service — null until FirebaseService finishes initializing.</summary>
    private IAuthService Auth => FirebaseService.Instance != null ? FirebaseService.Instance.Auth : null;

    private void Awake()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        logoutButton.onClick.AddListener(OnLogoutClicked);
    }

    private void Start()
    {
        // Firebase initializes asynchronously — wait for it before wiring up,
        // so a fast click can never reach a half-built auth service.
        StartCoroutine(WaitForAuth());
    }

    private void OnDestroy()
    {
        var auth = Auth;
        if (auth != null) auth.OnStateChanged -= HandleStateChanged;
    }

    private IEnumerator WaitForAuth()
    {
        ShowStatus("Connecting to Firebase…");
        SetInteractable(false);

        // Poll until the service exists and auth has left the Loading state —
        // or give up with a clear message.
        float waited = 0f;
        const float timeout = 30f;
        while (waited < timeout)
        {
            var auth = Auth;
            if (auth != null && auth.State != AuthState.Loading)
            {
                auth.OnStateChanged += HandleStateChanged;
                RefreshView();
                yield break;
            }
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        ShowStatus("Firebase is unavailable — playing offline.");
    }

    // ---------------------------------------------------------------
    //  Button handlers — async void is correct ONLY here: Unity event
    //  handlers are "fire and forget" entry points, and we catch every
    //  outcome through AuthResult instead of throwing.
    // ---------------------------------------------------------------

    private async void OnLoginClicked()
    {
        if (!ValidateInputs(out string email, out string password)) return;

        SetBusy(true, "Signing in…");
        AuthResult result = await Auth.SignInAsync(email, password);
        SetBusy(false, null);

        if (!result.Success) ShowStatus(result.ErrorMessage);
        // On success nothing else is needed — OnStateChanged refreshes the view.
    }

    private async void OnRegisterClicked()
    {
        if (!ValidateInputs(out string email, out string password)) return;

        SetBusy(true, "Creating account…");
        AuthResult result = await Auth.RegisterAsync(email, password);
        SetBusy(false, null);

        if (!result.Success) ShowStatus(result.ErrorMessage);
    }

    private void OnLogoutClicked()
    {
        Auth?.SignOut();
        ShowStatus("Signed out.");
    }

    // ---------------------------------------------------------------
    //  Input validation — cheap checks that never reach the network.
    // ---------------------------------------------------------------

    private bool ValidateInputs(out string email, out string password)
    {
        email = emailInput.text.Trim();
        password = passwordInput.text;

        if (string.IsNullOrEmpty(email))
        {
            ShowStatus("Enter your email.");
            return false;
        }
        if (!email.Contains("@"))
        {
            ShowStatus("That doesn't look like an email address.");
            return false;
        }
        if (string.IsNullOrEmpty(password))
        {
            ShowStatus("Enter your password.");
            return false;
        }
        if (password.Length < 6)
        {
            ShowStatus("Password must be at least 6 characters.");
            return false;
        }
        return true;
    }

    // ---------------------------------------------------------------
    //  View state — driven by AuthState, not by button clicks.
    // ---------------------------------------------------------------

    private void HandleStateChanged()
    {
        RefreshView();
    }

    private void RefreshView()
    {
        var auth = Auth;
        if (auth == null)
        {
            ShowStatus("Firebase is unavailable — playing offline.");
            SetInteractable(false);
            return;
        }

        switch (auth.State)
        {
            case AuthState.Loading:
                ShowStatus("Connecting to Firebase…");
                SetInteractable(false);
                break;

            case AuthState.Error:
                ShowStatus("Authentication is unavailable.");
                SetInteractable(false);
                break;

            case AuthState.SignedIn:
                ShowStatus("");
                if (signInGroup != null) signInGroup.SetActive(false);
                if (userText != null)
                    userText.text = string.IsNullOrEmpty(auth.Email)
                        ? $"Signed in as guest ({auth.UserId})"
                        : $"Signed in as {auth.Email}";
                if (logoutButton != null) logoutButton.gameObject.SetActive(true);
                SetInteractable(true);
                break;

            default: // SignedOut
                if (signInGroup != null) signInGroup.SetActive(true);
                if (userText != null) userText.text = "";
                if (logoutButton != null) logoutButton.gameObject.SetActive(false);
                SetInteractable(true);
                break;
        }
    }

    private void SetBusy(bool value, string message)
    {
        busy = value;
        SetInteractable(!value);
        if (message != null) ShowStatus(message);
    }

    private void SetInteractable(bool value)
    {
        if (busy && value) return;
        loginButton.interactable = value;
        registerButton.interactable = value;
        emailInput.interactable = value;
        passwordInput.interactable = value;
    }

    private void ShowStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
