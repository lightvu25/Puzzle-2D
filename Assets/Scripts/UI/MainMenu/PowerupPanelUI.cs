using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Power-up inventory panel (main menu tab). Displays every catalog entry the
/// player owns — icon, name, count — by spawning a row widget per entry.
/// Create a row prefab with a <see cref="PowerupRowWidget"/> on it and assign
/// the icon/name/count slots; assign each catalog icon Sprite here.
/// </summary>
public class PowerupPanelUI : MonoBehaviour
{
    [Serializable]
    public class PowerupCatalogEntry
    {
        [Tooltip("Id from PowerUpIds, e.g. 'kinetic_shield'.")]
        public string powerUpId = PowerUpIds.KineticShield;
        public string displayName = "Kinetic Shield";
        [Tooltip("Icon art — assign your power-up sprite.")]
        public Sprite icon;
    }

    [Header("Catalog")]
    [SerializeField] private List<PowerupCatalogEntry> catalog = new List<PowerupCatalogEntry>
    {
        new PowerupCatalogEntry()
    };

    [Header("List")]
    [SerializeField] private Transform rowContainer;
    [SerializeField] private PowerupRowWidget rowPrefab;
    [SerializeField] private GameObject emptyStateRoot;
    [SerializeField] private TMP_Text emptyStateText;

    private readonly List<PowerupRowWidget> spawnedRows = new List<PowerupRowWidget>();

    private void OnEnable()
    {
        RefreshList();
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged += RefreshList;
    }

    private void OnDisable()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged -= RefreshList;
    }

    public void RefreshList()
    {
        if (rowContainer == null || rowPrefab == null) return;

        foreach (var row in spawnedRows)
            if (row != null) Destroy(row.gameObject);
        spawnedRows.Clear();

        int owned = 0;
        foreach (var entry in catalog)
        {
            if (string.IsNullOrEmpty(entry.powerUpId)) continue;
            int count = ProgressionManager.Instance != null
                ? ProgressionManager.Instance.GetPowerUpCount(entry.powerUpId)
                : 0;
            if (count <= 0) continue;

            var row = Instantiate(rowPrefab, rowContainer);
            row.gameObject.SetActive(true);
            row.Setup(entry.icon, entry.displayName, count);
            spawnedRows.Add(row);
            owned++;
        }

        if (emptyStateRoot != null) emptyStateRoot.SetActive(owned == 0);
        if (emptyStateText != null && owned == 0)
            emptyStateText.text = "No power-ups yet — win them in mystery boxes!";
    }
}

/// <summary>One row in the power-up list — assign icon + labels on the prefab.</summary>
public class PowerupRowWidget : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;

    public void Setup(Sprite icon, string displayName, int count)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
        if (nameText != null) nameText.text = displayName;
        if (countText != null) countText.text = $"x{count}";
    }
}
