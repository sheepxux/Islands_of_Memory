using UnityEngine;

public class ZoneHint : MonoBehaviour
{
    [Header("Refs")]
    public GameObject player;             // Player GameObject
    public BoatHintTimed hint;            // BoatHintAnchor(BoatHintTimed)
    [TextArea] public string message = "Turn left here to enter Autumn Island.";
    public bool triggerOnce = true;

    [Header("Optional: only show when boating")]
    public BoatBoarding boatBoarding;
    public bool onlyWhenInBoat = true;

    [Header("Trigger Source")]
    public bool useBoatWhenInBoat = true;

    [Header("Hint Color (Inspector)")]
    public Color hintColor = Color.white;

    bool triggered;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered) return;

        if (hint == null) return;

        bool inBoat = (boatBoarding != null && boatBoarding.IsInBoat());

        if (onlyWhenInBoat && !inBoat) return;

        GameObject triggerObj = player;
        if (useBoatWhenInBoat && inBoat && boatBoarding != null && boatBoarding.boatController != null)
        {
            triggerObj = boatBoarding.boatController.gameObject;
        }

        if (triggerObj != null && other.gameObject != triggerObj) return;

        triggered = true;
        hint.ShowOnce(message, hintColor);
    }
}