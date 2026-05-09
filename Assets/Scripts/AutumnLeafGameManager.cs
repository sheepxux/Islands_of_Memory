using UnityEngine;
using TMPro;
using System.Collections;

public class AutumnLeafGameManager : MonoBehaviour
{
    [Header("Leaf Settings")]
    public int totalLeaves = 6;

    [Header("UI")]
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI completeText;

    [Header("Game Objects")]
    public GameObject instructionUI;
    public GameObject leavesRoot;
    public GameObject rewardPuzzle;

    private int collectedLeaves;
    private bool showingInstruction;
    private bool gameStarted;

    private void Start()
    {
        collectedLeaves = 0;
        showingInstruction = false;
        gameStarted = false;

        if (instructionUI != null)
            instructionUI.SetActive(false);

        if (leavesRoot != null)
            leavesRoot.SetActive(false);

        if (rewardPuzzle != null)
            rewardPuzzle.SetActive(false);

        if (progressText != null)
            progressText.gameObject.SetActive(false);

        if (completeText != null)
            completeText.gameObject.SetActive(false);

        UpdateUI();
    }

    private void Update()
    {
        if (showingInstruction && Input.anyKeyDown)
        {
            StartLeafGame();
        }
    }

    public void ShowInstruction()
    {
        if (gameStarted) return;

        if (instructionUI != null)
            instructionUI.SetActive(true);

        showingInstruction = true;
    }

    public void StartLeafGame()
    {
        showingInstruction = false;
        gameStarted = true;

        if (instructionUI != null)
            instructionUI.SetActive(false);

        if (leavesRoot != null)
            leavesRoot.SetActive(true);

        if (progressText != null)
            progressText.gameObject.SetActive(true);

        UpdateUI();
    }

    public void CollectLeaf()
    {
        if (!gameStarted) return;

        collectedLeaves++;
        collectedLeaves = Mathf.Clamp(collectedLeaves, 0, totalLeaves);

        UpdateUI();

        if (collectedLeaves >= totalLeaves)
        {
            UnlockReward();
        }
    }

    private void UpdateUI()
    {
        if (progressText != null)
            progressText.text = collectedLeaves + "/" + totalLeaves;
    }

    private void UnlockReward()
    {
        if (progressText != null)
            progressText.gameObject.SetActive(false);

        if (completeText != null)
        {
            completeText.text = "All leaves collected. A new puzzle piece appeared.";
            completeText.gameObject.SetActive(true);

            StartCoroutine(HideCompleteTextAfterDelay());
        }

        if (rewardPuzzle != null)
            rewardPuzzle.SetActive(true);
    }

    private IEnumerator HideCompleteTextAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (completeText != null)
            completeText.gameObject.SetActive(false);
    }
}