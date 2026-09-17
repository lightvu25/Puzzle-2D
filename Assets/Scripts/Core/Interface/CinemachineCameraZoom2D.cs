using UnityEngine;

// CinemachineCameraZoom2D.cs — STUB
// Controls orthographic zoom for the Cinemachine 2D camera.
// Implement with your actual camera zoom logic when ready.
public class CinemachineCameraZoom2D : MonoBehaviour
{
    public static CinemachineCameraZoom2D Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetNormalOrthographicSize() { }
}
