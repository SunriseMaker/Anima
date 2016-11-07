using UnityEngine.Events;
using System.Collections.Generic;

public class GameEventSystem
{
    public enum EventID { FightStart, RoundPreStart, RoundStart, RoundEnd, RoundResults, FightEnd }

    #region Variables
    private static Dictionary<EventID, UnityEvent> events = new Dictionary<EventID, UnityEvent>();
    #endregion Variables

    #region Red
    public static void Nullify()
    {
        events.Clear();
    }

    public static void StartListening(EventID event_id, UnityAction listener)
    {
        UnityEvent unity_event = null;

        if (events.TryGetValue(event_id, out unity_event))
        {
            unity_event.AddListener(listener);
        }
        else
        {
            unity_event = new UnityEvent();
            unity_event.AddListener(listener);
            events.Add(event_id, unity_event);
        }
    }

    public static void StopListening(EventID event_id, UnityAction listener)
    {
        UnityEvent unity_event = null;

        if (events.TryGetValue(event_id, out unity_event))
        {
            unity_event.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(EventID event_id)
    {
        UnityEvent unity_event = null;

        if (events.TryGetValue(event_id, out unity_event))
        {
            unity_event.Invoke();
        }
    }
    #endregion Red
}
