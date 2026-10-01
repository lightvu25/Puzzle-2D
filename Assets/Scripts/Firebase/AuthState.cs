/// <summary>
/// The lifecycle state of the authentication system.
///
/// The UI reads this (or listens to OnStateChanged) instead of guessing
/// from button clicks — a persisted session means the player can already
/// be SignedIn when the game starts, without anyone pressing Login.
/// </summary>
public enum AuthState
{
    /// <summary>Firebase is still initializing / restoring a saved session.</summary>
    Loading,

    /// <summary>Nobody is signed in — show the login form.</summary>
    SignedOut,

    /// <summary>A user is signed in (email or anonymous).</summary>
    SignedIn,

    /// <summary>Firebase itself could not initialize — auth is unavailable.</summary>
    Error
}
