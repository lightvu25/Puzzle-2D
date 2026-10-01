using System;
using System.Threading.Tasks;

/// <summary>
/// Authentication abstraction for the game — the only surface gameplay and
/// UI code should talk to. Supports email/password plus anonymous sign-in;
/// additional providers (Google etc.) plug in behind this interface later.
/// </summary>
public interface IAuthService
{
    /// <summary>Current lifecycle state — drives the UI without polling button results.</summary>
    AuthState State { get; }

    /// <summary>True when a user is currently signed in.</summary>
    bool IsSignedIn { get; }

    /// <summary>Stable user id (Firebase UID) or null when signed out.</summary>
    string UserId { get; }

    /// <summary>Email of the signed-in user — null for anonymous/guest users.</summary>
    string Email { get; }

    /// <summary>Fired after a successful sign-in. Argument is the user id.</summary>
    event Action<string> OnUserSignedIn;

    /// <summary>Fired whenever <see cref="State"/> changes — subscribe to refresh UI.</summary>
    event Action OnStateChanged;

    /// <summary>Signs in anonymously ("play as guest"). Returns the UID, or null on failure — never throws.</summary>
    Task<string> SignInAnonymouslyAsync();

    /// <summary>Signs in with email and password. Never throws — check AuthResult.Success.</summary>
    Task<AuthResult> SignInAsync(string email, string password);

    /// <summary>Creates a new email/password account and signs in. Never throws — check AuthResult.Success.</summary>
    Task<AuthResult> RegisterAsync(string email, string password);

    /// <summary>Signs out the current user.</summary>
    void SignOut();
}
