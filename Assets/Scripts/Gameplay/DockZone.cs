using UnityEngine;

public class DockZone : MonoBehaviour
{
    [Header("Optional: where player appears when exiting the boat at this dock")]
    public Transform exitPoint;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }
}