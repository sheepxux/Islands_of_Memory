using UnityEngine;
using UnityEngine.UI;

public class PuzzleUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject previewPanel;   // PuzzlePreviewPanel
    public Image previewImage;        // PuzzlePreviewImage
    public Button closeButton;        // ClosePreviewButton

    [Header("Puzzle Images (4 pieces)")]
    public Sprite[] puzzleSprites = new Sprite[4]; // 0 Winter,1 Autumn,2 Summer,3 Last

    [Header("Progress (optional but recommended)")]
    public PuzzleProgress progress;

    private void Start()
    {
        if (previewPanel != null) previewPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePreview);
    }

    public void ShowPuzzle(int index)
    {
        if (index < 0 || index >= puzzleSprites.Length) return;

        if (progress != null && !progress.IsUnlocked(index))
        {
            Debug.Log($"[PuzzleUIManager] Piece {index} not unlocked yet.");
            return;
        }

        if (previewPanel == null || previewImage == null) return;

        previewImage.sprite = puzzleSprites[index];
        previewPanel.SetActive(true);
    }

    public void ClosePreview()
    {
        if (previewPanel != null) previewPanel.SetActive(false);
    }
}