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
    private Transform playerTf;

    void Start()
    {
        if (glow != null) glow.Hide();

        if (distanceTarget == null)
            distanceTarget = transform;

        if (puzzleVisual == null && transform.childCount > 0)
            puzzleVisual = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (playerInside && glow != null && playerTf != null)
        {
            float d = Vector3.Distance(playerTf.position, distanceTarget.position);
            float factor = Mathf.InverseLerp(farDistance, nearDistance, d);
            glow.SetDistanceFactor(factor);
        }

        if (!pickupOnTrigger) return;
        if (!playerInside) return;

        if (Input.GetKeyDown(pickupKey))
            TryPickup();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        playerTf = other.transform;

        if (glow != null)
        {
            glow.Show();
            glow.SetDistanceFactor(0f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        playerTf = null;

        if (glow != null) glow.Hide();
    }

    private void TryPickup()
    {
        if (progress != null) progress.UnlockPiece(pieceIndex);
        if (pickupSfx != null) pickupSfx.Play();

        if (glow != null) glow.Hide();
        if (puzzleVisual != null) puzzleVisual.SetActive(false);

        if (destroyOnPickup) Destroy(gameObject);
    }
}