using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Scene-driven panel registry and navigation. Panels are assigned in the
/// Inspector as scene GameObjects — the manager never instantiates prefabs
/// and never searches the scene. One panel is "active" at a time; opening a
/// panel closes the current one. Mappings can opt into freezing time while
/// open (Pause, Result).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [System.Serializable]
    public class UIPanelMapping
    {
        public UIPanelType panelType;
        public GameObject panelObject;
        public bool freezeTime;
    }

    [SerializeField] private List<UIPanelMapping> panelMappings;

    private readonly Dictionary<UIPanelType, IUIPanel> panelsDict = new();
    private UIPanelType currentActivePanel = UIPanelType.None;
    public UIPanelType CurrentActivePanel => currentActivePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RegisterPanels();
    }

    private void RegisterPanels()
    {
        panelsDict.Clear();

        foreach (var mapping in panelMappings)
        {
            if (mapping.panelObject == null) continue;

            if (!mapping.panelObject.scene.IsValid())
            {
                Debug.LogWarning($"[UIManager] {mapping.panelObject.name} is not a scene object — panels must be scene GameObjects.");
                continue;
            }

            IUIPanel panel = mapping.panelObject.GetComponent<IUIPanel>();
            if (panel != null)
            {
                panelsDict[mapping.panelType] = panel;
            }
            else
            {
                Debug.LogWarning($"[UIManager] {mapping.panelObject.name} does not implement IUIPanel.");
            }
        }
    }

    public void OpenPanel(UIPanelType type)
    {
        if (type == UIPanelType.None)
        {
            Debug.LogWarning("[UIManager] Cannot open None panel.");
            return;
        }

        if (!panelsDict.TryGetValue(type, out IUIPanel panel))
        {
            Debug.LogError($"[UIManager] Panel '{type}' is not registered.");
            return;
        }

        if (currentActivePanel == type)
        {
            return;
        }

        CloseCurrentPanel();

        // Mark the panel active before Show() so callbacks fired during Show()
        // (button events, close-handlers) see a consistent state.
        currentActivePanel = type;
        panel.Show();

        if (GetFreezeTime(type))
        {
            Time.timeScale = 0f;
        }
    }

    public void CloseCurrentPanel()
    {
        if (currentActivePanel == UIPanelType.None)
        {
            return;
        }

        // Clear the active panel before Hide() so a nested OpenPanel() call
        // from a Hide() callback doesn't recurse back into this close.
        UIPanelType closing = currentActivePanel;
        currentActivePanel = UIPanelType.None;
        Time.timeScale = 1f;

        if (panelsDict.TryGetValue(closing, out IUIPanel panel))
        {
            panel.Hide();
        }
    }

    public void ClosePanelIfOpen(UIPanelType type)
    {
        if (currentActivePanel == type)
        {
            CloseCurrentPanel();
        }
    }

    public T GetPanel<T>(UIPanelType type) where T : class, IUIPanel
    {
        if (panelsDict.TryGetValue(type, out IUIPanel panel))
        {
            return panel as T;
        }

        return null;
    }

    public bool IsPanelOpen(UIPanelType type)
    {
        return currentActivePanel == type;
    }

    public bool IsAnyPanelOpen()
    {
        return currentActivePanel != UIPanelType.None;
    }

    private bool GetFreezeTime(UIPanelType type)
    {
        foreach (UIPanelMapping mapping in panelMappings)
        {
            if (mapping.panelType == type)
            {
                return mapping.freezeTime;
            }
        }

        return false;
    }
}
