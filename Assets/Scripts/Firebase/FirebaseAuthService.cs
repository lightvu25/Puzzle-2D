using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

/// <summary>
/// IAuthService backed by Firebase Authentication. Anonymous sign-in only —
/// additional providers plug in later without touching callers.
/// </summary>
public class FirebaseAuthService : IAuthService
{
    private readonly FirebaseAuth auth;

    public bool IsSignedIn => auth != null && auth.CurrentUser != null;
    public string UserId => IsSignedIn ? auth.CurrentUser.UserId : null;

    public event Action<string> OnUserSignedIn;

    public FirebaseAuthService(FirebaseAuth auth)
    {
        this.auth = auth;
    }

    public async Task<string> SignInAnonymouslyAsync()
    {
        if (auth == null) return null;
        if (IsSignedIn) return UserId;

        try
        {
            AuthResult result = await auth.SignInAnonymouslyAsync();
            string uid = result != null && result.User != null ? result.User.UserId : null;
            if (!string.IsNullOrEmpty(uid))
                OnUserSignedIn?.Invoke(uid);
            return uid;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FirebaseAuthService] Anonymous sign-in failed: {e.Message}");
            return null;
        }
    }

    public void SignOut()
    {
        try
        {
            auth?.SignOut();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FirebaseAuthService] Sign-out failed: {e.Message}");
        }
    }
}
