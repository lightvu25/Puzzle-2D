using System;
using UnityEngine;

public class EventChoice
{
    public string prompt;
    public string description;
    public Action onChosen;
}

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    public event Action<EventChoice[]> OnEventTriggered;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerEvent(EventChoice[] choices)
    {
        if (choices == null || choices.Length == 0) return;
        OnEventTriggered?.Invoke(choices);
    }

    // Example pre-built event — HealthSystem/DamageInfo references removed.
    // Restore this once HealthSystem and DamageInfo are ported to this project.
    public void TriggerSacrificeEvent()
    {
        EventChoice[] choices = new EventChoice[]
        {
            new EventChoice
            {
                prompt = "Sacrifice HP for Power",
                description = "Lose 50% of your current HP to gain +25 Max HP (+1 Slot).",
                onChosen = () =>
                {
                    // TODO: implement once HealthSystem is available in this project.
                    Debug.Log("[EventManager] TriggerSacrificeEvent: HealthSystem not yet available.");
                }
            },
            new EventChoice
            {
                prompt = "Leave",
                description = "Walk away unharmed.",
                onChosen = () => { /* Do nothing */ }
            }
        };

        TriggerEvent(choices);
    }
}
