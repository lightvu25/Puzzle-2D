using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HouseFlow.Tools;
using HouseFlow.Level;
using HouseFlow.Analytics;

namespace HouseFlow.UI
{
    [System.Serializable]
    public class ToolSlot
    {
        public string toolId;
        public Button button;
        public Text quantityText;
        public Image iconImage;
    }

    /// <summary>
    /// UI that displays available tools and allows player to use them.
    /// Strictly presentation logic. Defers consumption to ToolInventory.
    /// </summary>
    public class ToolTrayUI : MonoBehaviour
    {
        [SerializeField] private LevelFlowController flowController;
        [SerializeField] private List<ToolSlot> slots = new List<ToolSlot>();

        private void Awake()
        {
            if (flowController == null)
                flowController = FindAnyObjectByType<LevelFlowController>();

            foreach (var slot in slots)
            {
                if (slot.button != null)
                {
                    var boundToolId = slot.toolId; // local copy for closure
                    slot.button.onClick.AddListener(() => OnToolClicked(boundToolId));
                }
            }
        }

        private void Start()
        {
            if (ToolInventory.Instance != null)
            {
                ToolInventory.Instance.OnToolQuantityChanged += HandleQuantityChanged;
            }
            RefreshAllSlots();
        }

        private void OnDestroy()
        {
            if (ToolInventory.Instance != null)
            {
                ToolInventory.Instance.OnToolQuantityChanged -= HandleQuantityChanged;
            }
        }

        private void HandleQuantityChanged(string toolId, int newQuantity)
        {
            RefreshSlot(toolId);
        }

        private void RefreshAllSlots()
        {
            foreach (var slot in slots)
            {
                RefreshSlot(slot.toolId);
            }
        }

        private void RefreshSlot(string toolId)
        {
            if (string.IsNullOrEmpty(toolId) || ToolInventory.Instance == null) return;

            var slot = slots.Find(s => s.toolId == toolId);
            if (slot == null) return;

            int qty = ToolInventory.Instance.GetToolCount(toolId);
            
            if (slot.quantityText != null)
            {
                slot.quantityText.text = qty.ToString();
            }

            if (slot.button != null)
            {
                slot.button.interactable = (qty > 0);
            }
        }

        private void OnToolClicked(string toolId)
        {
            if (ToolInventory.Instance == null || flowController == null) return;

            if (ToolInventory.Instance.GetToolCount(toolId) <= 0) return;

            ITroubleshooterTool tool = null;
            var allTools = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTools)
            {
                if (t is ITroubleshooterTool tsTool && tsTool.ToolType.ToString() == toolId)
                {
                    tool = tsTool;
                    break;
                }
            }

            if (tool == null)
            {
                Debug.LogWarning($"[ToolTrayUI] Tool {toolId} implementation not found in scene.");
                return;
            }

            var root = flowController.CurrentRoot;
            if (root == null) return;

            if (tool.CanActivate(root))
            {
                tool.Activate(root, () =>
                {
                    // Only consume the tool IF activation succeeds and completes
                    ToolInventory.Instance.ConsumeTool(toolId, 1);

                    var p = new Dictionary<string, object> { ["tool_type"] = toolId };
                    var level = flowController.CurrentLevelData;
                    if (level != null) p["level_id"] = level.LevelId;
                    AnalyticsService.Track(AnalyticsEvents.ToolUsed, p);
                });
            }
            else
            {
                Debug.Log($"[ToolTrayUI] Tool {toolId} cannot activate right now.");
            }
        }
    }
}
