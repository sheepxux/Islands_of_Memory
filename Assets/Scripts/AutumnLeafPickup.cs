using UnityEngine;

public class AutumnLeafPickup : MonoBehaviour
{
    public KeyCode pickupKey = KeyCode.E;
    public AutumnLeafGameManager manager;
    public PickupGlowPulse glow;
    public GameObject visualRoot;
    public GameObject glowRoot;

    private bool playerInside;
    private bool collected;

    private void Start()
    {
        if (visualRoot == null)
            visualRoot = gameObject;

        if (glowRoot == null && glow != null)
            glowRoot = glow.gameObject;

        if (glow != null)
            glow.Hide();

        if (glowRoot != null)
            glowRoot.SetActive(false);
    }

    private void Update()
    {
        if (playerInside && !collected && Input.GetKeyDown(pickupKey))
        {
            Collect();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (collected) return;

        playerInside = true;

        if (glowRoot != null)
            glowRoot.SetActive(true);

        if (glow != null)
            glow.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (glow != null)
            glow.Hide();

        if (glowRoot != null)
            glowRoot.SetActive(false);
    }

    private void Collect()
    {
        if (collected) return;

        collected = true;

        if (manager != null)
            manager.CollectLeaf();

        if (glow != null)
            glow.Hide();

        if (glowRoot != null)
            glowRoot.SetActive(false);

        if (visualRoot != null)
            visualRoot.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }
}