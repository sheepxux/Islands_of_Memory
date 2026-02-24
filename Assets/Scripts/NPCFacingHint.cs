using UnityEngine;
using TMPro;

public class NPCFacingHint : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public Transform npcVisualRoot;
    public Canvas hintCanvas;
    public TMP_Text hintText;

    [Header("Behavior")]
    public string inRangeText = "Press E to board";
    public float turnSpeed = 8f;
    public bool facePlayerWhenInRange = true;

    private bool playerInRange;

    void Awake()
    {
        if (hintCanvas != null) hintCanvas.enabled = false;
        if (hintText != null) hintText.text = inRangeText;
    }

    void Update()
    {
        if (!playerInRange) return;
        if (!facePlayerWhenInRange) return;
        if (player == null || npcVisualRoot == null) return;

        Vector3 dir = player.position - npcVisualRoot.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        npcVisualRoot.rotation = Quaternion.Slerp(
            npcVisualRoot.rotation,
            targetRot,
            1f - Mathf.Exp(-turnSpeed * Time.deltaTime)
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (player == null) return;
        if (other.transform != player) return;

        playerInRange = true;
        if (hintText != null) hintText.text = inRangeText;
        if (hintCanvas != null) hintCanvas.enabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (player == null) return;
        if (other.transform != player) return;

        playerInRange = false;
        if (hintCanvas != null) hintCanvas.enabled = false;
    }
}