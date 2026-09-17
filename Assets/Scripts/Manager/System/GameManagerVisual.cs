using Unity.Cinemachine;
using UnityEngine;

public class GameManagerVisual : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource pickupCinemachineImpulseSource;
    [SerializeField] private Transform pickupCollectVfxPrefab;
    // [SerializeField] private ScorePopup scorePopupPrefab;
    [SerializeField] private Transform confettiVfxPrefab;

    private void Start()
    {
        if (PlayerInteract.Instance != null)
            PlayerInteract.Instance.OnCoinPickup += PlayerInteract_OnCoinPickup;
    }

    private void OnDestroy()
    {
        if (PlayerInteract.Instance != null)
            PlayerInteract.Instance.OnCoinPickup -= PlayerInteract_OnCoinPickup;
    }

    private void PlayerInteract_OnCoinPickup(object sender, System.EventArgs e)
    {
        if (pickupCollectVfxPrefab != null)
        {
            Vector3 spawnPos = sender is Component c ? c.transform.position : transform.position;
            Transform vfx = Instantiate(pickupCollectVfxPrefab, spawnPos, Quaternion.identity);
            Destroy(vfx.gameObject, 1f);
        }
        pickupCinemachineImpulseSource.GenerateImpulse(4f);
        // Instantiate(scorePopupPrefab, spawnPos, Quaternion.identity);
    }

}