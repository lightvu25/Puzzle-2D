using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

/// <summary>
/// Singleton entry point for the Firebase infrastructure layer.
///
/// Startup chain: dependency check → FirebaseApp → anonymous auth → cloud
/// save provider. Every step is async and failure-safe: if any step fails
/// the service settles into a non-ready status and the game keeps working
/// offline. Gameplay code must only use Auth/CloudSave/UserId — never the
/// Firebase.* namespaces directly.
/// </summary>
public class FirebaseService : MonoBehaviour
{
    public enum Status
    {
        Uninitialized,
        Initializing,
        Ready,
        AuthFailed,
        Unavailable
    }

    public static FirebaseService Instance { get; private set; }

    /// <summary>Firebase UID of the signed-in user, or null when signed out.</summary>
    public static string CurrentUserId => Instance != null ? Instance.UserId : null;

    public Status CurrentStatus { get; private set; } = Status.Uninitialized;
    public bool IsReady => CurrentStatus == Status.Ready;

    /// <summary>Auth abstraction — anonymous sign-in only for now.</summary>
    public IAuthService Auth { get; private set; }

    /// <summary>Optional cloud save sync. Null until Status.Ready.</summary>
    public ICloudSaveProvider CloudSave { get; private set; }

    /// <summary>Firebase UID of the signed-in user, or null.</summary>
    public string UserId => Auth != null ? Auth.UserId : null;

    /// <summary>Fired whenever CurrentStatus changes.</summary>
    public event Action<Status> OnStatusChanged;

    private FirebaseApp app;

    private void Awake()
    {
        if (Instance != null && !ReferenceEquals(Instance, this))
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (ReferenceEquals(Instance, this))
            Instance = null;
    }

    private void Start()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        SetStatus(Status.Initializing);

        try
        {
            DependencyStatus deps = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (deps != DependencyStatus.Available)
            {
                Debug.LogWarning($"[FirebaseService] Dependencies unavailable ({deps}). Cloud features disabled.");
                SetStatus(Status.Unavailable);
                return;
            }

            app = FirebaseApp.DefaultInstance;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FirebaseService] Initialization failed: {e.Message}. Cloud features disabled.");
            SetStatus(Status.Unavailable);
            return;
        }

        Auth = new FirebaseAuthService(FirebaseAuth.GetAuth(app));
        string uid = await Auth.SignInAnonymouslyAsync();
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("[FirebaseService] Anonymous auth failed — running without cloud sync.");
            SetStatus(Status.AuthFailed);
            return;
        }

        CloudSave = new FirebaseCloudSaveProvider(app, () => Auth.UserId);
        SetStatus(Status.Ready);
        Debug.Log($"[FirebaseService] Ready — uid={uid}");
    }

    private void SetStatus(Status status)
    {
        if (CurrentStatus == status) return;
        CurrentStatus = status;
        OnStatusChanged?.Invoke(status);
    }
}
