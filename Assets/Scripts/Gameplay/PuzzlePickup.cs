using UnityEngine;

public class PuzzlePickup : MonoBehaviour
{
    [Header("Which piece is this? 0=Winter,1=Autumn,2=Summer,3=Last")]
    public int pieceIndex = 0;

    [Header("Pickup Settings")]
    public bool pickupOnTrigger = true;
    public KeyCode pickupKey = KeyCode.E;

    [Header("References")]
    public PuzzleProgress progress;
    public PickupGlowPulse glow;

    [Header("Distance Brightness")]
    public Transform distanceTarget;
    public float nearDistance = 0.8f;
    public float farDistance = 2.5f;

    [Header("Behaviour")]
    public bool destroyOnPickup = true;
    public GameObject puzzleVisual;
    public AudioSource pickupSfx;

    private bool playerInside;
    private bool collected;
    private Transform playerTf;

    private void Start()
    {
        if (glow != null)
            glow.Hide();

        if (distanceTarget == null)
            distanceTarget = transform;

        if (puzzleVisual == null && transform.childCount > 0)
            puzzleVisual = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if (playerInside && glow != null && playerTf != null)
        {
            float d = Vector3.Distance(playerTf.position, distanceTarget.position);
            float factor = Mathf.InverseLerp(farDistance, nearDistance, d);
            glow.SetDistanceFactor(factor);
        }

        if (!pickupOnTrigger) return;
        if (!playerInside) return;
        if (collected) return;

        if (Input.GetKeyDown(pickupKey))
            TryPickup();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (collected) return;

        playerInside = true;
        playerTf = other.transform;

        if (glow != null)
        {
            glow.Show();
            glow.SetDistanceFactor(0f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        playerTf = null;

        if (glow != null)
            glow.Hide();
    }

    private void TryPickup()
    {
        if (collected) return;

        collected = true;

        if (progress != null)
            progress.UnlockPiece(pieceIndex);

        AutumnPuzzleStart autumnStart = GetComponent<AutumnPuzzleStart>();
        if (autumnStart != null)
            autumnStart.TriggerLeafInstruction();

        if (pickupSfx != null)
            pickupSfx.Play();

        if (glow != null)
            glow.Hide();

        if (puzzleVisual != null)
            puzzleVisual.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}