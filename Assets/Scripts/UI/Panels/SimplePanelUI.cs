using UnityEngine;

/// <summary>
/// Minimal IUIPanel for panels that need no dedicated script — Show/Hide
/// simply toggle the panel GameObject. Use for static panels (Settings) or
/// as the registered bridge for panels whose content is driven by another
/// component (e.g. PausePanel, where PauseMenuUI owns the buttons and its
/// Show()/Hide() only toggle this same GameObject).
/// </summary>
public class SimplePanelUI : MonoBehaviour, IUIPanel
{
    [Tooltip("Optional override — defaults to this GameObject.")]
    [SerializeField] private GameObject target;

    public void Show() => (target != null ? target : gameObject).SetActive(true);

    public void Hide() => (target != null ? target : gameObject).SetActive(false);
}
