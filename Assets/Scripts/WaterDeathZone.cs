using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterDeathZone : MonoBehaviour
{
    [Header("References")]
    public RespawnManager respawnManager;
    public BoatBoarding boatBoarding;

    [Header("Rules")]
    public bool ignoreWhileInBoat = true;

    private Collider triggerCollider;

    private void Reset()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        if (respawnManager == null)
            respawnManager = FindFirstObjectByType<RespawnManager>();

        if (boatBoarding == null)
            boatBoarding = FindFirstObjectByType<BoatBoarding>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
            return;

        if (ignoreWhileInBoat && boatBoarding != null && boatBoarding.IsInBoat())
            return;

        if (respawnManager == null)
            respawnManager = FindFirstObjectByType<RespawnManager>();

        if (respawnManager != null)
            respawnManager.Respawn(playerMovement.gameObject);
    }
}
