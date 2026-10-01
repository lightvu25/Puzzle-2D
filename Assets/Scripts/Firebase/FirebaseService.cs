using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

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

    public static string CurrentUserId => Instance != null ? Instance.UserId : null;

    public Status CurrentStatus { get; private set; } = Status.Uninitialized;
    public bool IsReady => CurrentStatus == Status.Ready;

    public IAuthService Auth { get; private set; }

    public ICloudSaveProvider CloudSave { get; private set; }

    public string UserId => Auth != null ? Auth.UserId : null;

    public event Action<Status> OnStatusChanged;

    private FirebaseApp app;

    private void Awake()
    {
        if (Instance != null)
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

        try
        {
            Auth = new FirebaseAuthService(FirebaseAuth.GetAuth(app));
            CloudSave = new FirebaseCloudSaveProvider(app, () => Auth.UserId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FirebaseService] Auth setup failed: {e.Message}. Cloud features disabled.");
            SetStatus(Status.Unavailable);
            return;
        }

        if (Auth.IsSignedIn)
        {
            SetStatus(Status.Ready);
            Debug.Log($"[FirebaseService] Ready — restored session uid={Auth.UserId}");
            return;
        }

        string uid = await Auth.SignInAnonymouslyAsync();
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("[FirebaseService] Anonymous auth failed — email login still available.");
            SetStatus(Status.AuthFailed);
            return;
        }

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
