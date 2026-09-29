using System.Threading.Tasks;

/// <summary>
/// Optional cloud synchronization for save data, keyed by save slot.
/// SaveManager remains the local/domain save authority — this layer only
/// mirrors payloads. All methods are failure-safe and never throw.
/// </summary>
public interface ICloudSaveProvider
{
    /// <summary>False when the provider has no usable backend/user.</summary>
    bool IsAvailable { get; }

    /// <summary>Uploads a serialized payload for the given slot. Returns success.</summary>
    Task<bool> UploadAsync(string slot, string json);

    /// <summary>Returns the stored payload for the slot, or null if missing/failed.</summary>
    Task<string> DownloadAsync(string slot);
}
