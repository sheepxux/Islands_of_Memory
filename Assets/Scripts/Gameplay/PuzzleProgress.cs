using UnityEngine;
using UnityEngine.UI;

public class PuzzleProgress : MonoBehaviour
{
    [Header("Slot Images (top-right 4 slots)")]
    public Image[] slotImages = new Image[4];

    [Header("Locked Visual")]
    public Sprite lockedSprite;
    [Range(0f, 1f)] public float lockedAlpha = 0.35f;

    [Header("Unlocked Puzzle Sprites (4 pieces)")]
    public Sprite[] unlockedSprites = new Sprite[4];

    private bool[] unlocked = new bool[4];

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            SetLocked(i);
        }
    }

    private void SetLocked(int index)
    {
        if (!IsValidIndex(index)) return;
        if (slotImages[index] == null) return;

        slotImages[index].sprite = lockedSprite;
        var c = slotImages[index].color;
        c.a = lockedAlpha;
        slotImages[index].color = c;

        var btn = slotImages[index].GetComponent<Button>();
        if (btn != null) btn.interactable = false;

        unlocked[index] = false;
    }

    private void SetUnlocked(int index)
    {
        if (!IsValidIndex(index)) return;
        if (slotImages[index] == null) return;

        slotImages[index].sprite = unlockedSprites[index];
        var c = slotImages[index].color;
        c.a = 1f;
        slotImages[index].color = c;

        var btn = slotImages[index].GetComponent<Button>();
        if (btn != null) btn.interactable = true;

        unlocked[index] = true;

        Debug.Log($"[PuzzleProgress] Unlocked piece: {index}");
    }

    public void UnlockPiece(int index)
    {
        if (!IsValidIndex(index)) return;
        if (unlocked[index]) return;

        if (unlockedSprites[index] == null)
        {
            Debug.LogWarning($"[PuzzleProgress] unlockedSprites[{index}] is NULL.");
            return;
        }

        SetUnlocked(index);
    }

    public bool IsUnlocked(int index)
    {
        if (!IsValidIndex(index)) return false;
        return unlocked[index];
    }

    private bool IsValidIndex(int index) => index >= 0 && index < 4;
}