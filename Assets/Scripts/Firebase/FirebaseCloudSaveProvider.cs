using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// ICloudSaveProvider backed by Cloud Firestore.
///
/// Documents live at cloud_saves/{uid}/slots/{slot} and store an opaque
/// JSON payload plus an update timestamp — deliberately schema-agnostic so
/// the final save format can evolve with the Tomb of the Mask GDD.
/// </summary>
public class FirebaseCloudSaveProvider : ICloudSaveProvider
{
    private const string RootCollection = "cloud_saves";
    private const string SlotsCollection = "slots";
    private const string PayloadField = "payload";
    private const string UpdatedField = "updatedUtcTicks";

    private readonly FirebaseApp app;
    private readonly Func<string> userIdProvider;
    private FirebaseFirestore firestore;
    private bool firestoreFailed;

    public bool IsAvailable
    {
        get
        {
            DocumentReference unused;
            return TryGetDocument("probe", out unused, log: false);
        }
    }

    public FirebaseCloudSaveProvider(FirebaseApp app, Func<string> userIdProvider)
    {
        this.app = app;
        this.userIdProvider = userIdProvider;
    }

    public async Task<bool> UploadAsync(string slot, string json)
    {
        DocumentReference doc;
        if (string.IsNullOrEmpty(json) || !TryGetDocument(slot, out doc)) return false;

        var data = new Dictionary<string, object>
        {
            { PayloadField, json },
            { UpdatedField, DateTime.UtcNow.Ticks }
        };

        try
        {
            await doc.SetAsync(data);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FirebaseCloudSave] Upload '{slot}' failed: {e.Message}");
            return false;
        }
    }

    public async Task<string> DownloadAsync(string slot)
    {
        DocumentReference doc;
        if (!TryGetDocument(slot, out doc)) return null;

        try
        {
            DocumentSnapshot snapshot = await doc.GetSnapshotAsync();
            if (!snapshot.Exists) return null;

            string json;
            return snapshot.TryGetValue(PayloadField, out json) ? json : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FirebaseCloudSave] Download '{slot}' failed: {e.Message}");
            return null;
        }
    }

    private bool TryGetDocument(string slot, out DocumentReference doc, bool log = true)
    {
        doc = null;
        if (firestoreFailed || app == null || userIdProvider == null) return false;

        string uid = userIdProvider();
        if (string.IsNullOrEmpty(uid)) return false;

        if (firestore == null)
        {
            try
            {
                firestore = FirebaseFirestore.GetInstance(app);
            }
            catch (Exception e)
            {
                firestoreFailed = true;
                if (log)
                    Debug.LogWarning($"[FirebaseCloudSave] Firestore unavailable: {e.Message}");
                return false;
            }
        }

        doc = firestore
            .Collection(RootCollection).Document(uid)
            .Collection(SlotsCollection).Document(slot);
        return true;
    }
}
