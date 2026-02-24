using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzlePickup : MonoBehaviour
{
    [Header("Which piece is this? 0=Winter,1=Autumn,2=Summer,3=Last")]
    [Range(0, 3)]
    public int pieceIndex = 0;

    [Header("Pickup Settings")]
    public bool pickupOnTrigger = true;
    public KeyCode pickupKey = KeyCode.E;

    [Header("References")]
    public PuzzleProgress progress;

    private bool playerInside = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (!pickupOnTrigger) return;
        if (!playerInside) return;

        if (Input.GetKeyDown(pickupKey))
            DoPickup();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pickupOnTrigger) return;
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!pickupOnTrigger) return;
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    private void DoPickup()
    {
        if (progress == null)
        {
            Debug.LogWarning("[PuzzlePickup] progress is NULL.");
            return;
        }

        progress.UnlockPiece(pieceIndex);
        Debug.Log($"Picked puzzle piece: {pieceIndex}");

        gameObject.SetActive(false);
    }
}