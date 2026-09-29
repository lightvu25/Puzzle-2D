using System;
using System.Threading.Tasks;

/// <summary>
/// Authentication abstraction for the game. Anonymous-only for now;
/// additional providers (Google etc.) plug in behind this interface later.
/// </summary>
public interface IAuthService
{
    /// <summary>True when a user is currently signed in.</summary>
    bool IsSignedIn { get; }

    /// <summary>Stable user id (Firebase UID) or null when signed out.</summary>
    string UserId { get; }

    /// <summary>Fired after a successful sign-in. Argument is the user id.</summary>
    event Action<string> OnUserSignedIn;

    /// <summary>Signs in anonymously. Returns the UID, or null on failure — never throws.</summary>
    Task<string> SignInAnonymouslyAsync();

    /// <summary>Signs out the current user.</summary>
    void SignOut();
}
