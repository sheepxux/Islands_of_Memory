using UnityEngine;

public class BoatBoarding : MonoBehaviour
{
    [Header("Trigger (IMPORTANT)")]
    public Collider interactTrigger;

    [Header("References")]
    public Transform seat;
    public Vector3 seatEulerOffset = Vector3.zero;
    public Transform fallbackExitPoint;

    public BoatController boatController;

    public TopDownCamera topDownCamera;
    public Transform boatForwardRef;
    public GameObject player;

    [Header("Boat Hint (optional)")]
    public BoatHintTimed boatHint;

    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Rules")]
    public bool onlyExitAtDock = true;

    [Header("Debug")]
    public bool logDebug = false;

    private CharacterController playerCC;
    private MonoBehaviour playerMovement;

    private bool playerInRange;
    private bool inBoat;

    private DockZone currentDock;

    public bool IsInBoat() => inBoat;

    private void Awake()
    {
        if (interactTrigger == null)
            Debug.LogError("[BoatBoarding] interactTrigger is not assigned.");
        else
            interactTrigger.isTrigger = true;

        if (player != null)
        {
            playerCC = player.GetComponent<CharacterController>();

            foreach (var mb in player.GetComponents<MonoBehaviour>())
            {
                if (mb != null && mb.GetType().Name == "PlayerMovement")
                {
                    playerMovement = mb;
                    break;
                }
            }
        }

        if (boatController != null)
            boatController.enabled = false;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(interactKey)) return;

        if (!inBoat)
        {
            if (!playerInRange)
            {
                if (logDebug) Debug.Log("[BoatBoarding] Not in BoardZone range.");
                return;
            }

            Board();
        }
        else
        {
            if (onlyExitAtDock && currentDock == null)
            {
                if (logDebug) Debug.Log("[BoatBoarding] Can't exit: not at a dock.");
                return;
            }

            TryExit();
        }
    }

    private void Board()
    {
        if (player == null || seat == null || boatController == null)
        {
            Debug.LogWarning("[BoatBoarding] Board failed: missing references.");
            return;
        }

        inBoat = true;

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCC != null) playerCC.enabled = false;

        player.transform.SetParent(seat, true);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.Euler(seatEulerOffset);

        boatController.enabled = true;

        if (topDownCamera != null)
        {
            topDownCamera.SetTarget(boatController.transform);
            if (boatForwardRef != null) topDownCamera.SetBoatForwardRef(boatForwardRef);
            topDownCamera.SetMode(TopDownCamera.Mode.Boat);
            topDownCamera.Snap();
        }

        if (boatHint != null)
            boatHint.ShowOnce();

        if (logDebug) Debug.Log("[BoatBoarding] Player boarded the boat!");
    }

    private void TryExit()
    {
        Transform exitT =
            (currentDock != null && currentDock.exitPoint != null)
            ? currentDock.exitPoint
            : fallbackExitPoint;

        if (exitT == null)
        {
            Debug.LogWarning("[BoatBoarding] No exit point set (dock exitPoint / fallbackExitPoint).");
            return;
        }

        inBoat = false;
        playerInRange = false;

        player.transform.SetParent(null, true);
        player.transform.position = exitT.position;
        player.transform.rotation = exitT.rotation;

        if (playerCC != null) playerCC.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;

        boatController.enabled = false;

        if (topDownCamera != null)
        {
            topDownCamera.SetTarget(player.transform);
            topDownCamera.SetMode(TopDownCamera.Mode.Walk);
            topDownCamera.Snap();
        }

        if (logDebug) Debug.Log("[BoatBoarding] Player exited the boat!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player != null && IsPlayerCollider(other))
            playerInRange = true;

        var dock = other.GetComponent<DockZone>();
        if (dock != null)
            currentDock = dock;
    }

    private void OnTriggerExit(Collider other)
    {
        if (player != null && IsPlayerCollider(other))
            playerInRange = false;

        var dock = other.GetComponent<DockZone>();
        if (dock != null && currentDock == dock)
            currentDock = null;
    }

    private bool IsPlayerCollider(Collider other)
    {
        return other.transform == player.transform || other.transform.root == player.transform;
    }
}