using System.Collections;
using UnityEngine;

public class NPCIntroDialogue : MonoBehaviour
{
    [Header("References")]
    public NPCFacingHint hint;
    public BoatBoarding boatBoarding;
    public GuideOrbAI guide;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Prompts")]
    public string talkPrompt = "Press E to talk";
    public string boardPrompt = "Press E to board";

    [Header("Dialogue")]
    [TextArea(2, 4)]
    public string[] dialogueLines =
    {
        "The boat is ready, but my memories feel far away today.",
        "Will you stay with me?",
        "And help me find them again?"
    };

    [Header("Timing")]
    public bool autoCompleteAfterFinalLine = true;
    public float finalLineHoldTime = 2f;

    [Header("Guide")]
    public bool pauseGuideUntilComplete = true;
    public bool facePlayerDuringDialogue = true;
    public bool facePlayerAfterDialogue = true;

    private int currentLineIndex = -1;
    private bool dialogueComplete;
    private Coroutine completeRoutine;
    private GuideOrbAI.State savedGuideState;
    private Transform savedGuideTarget;
    private bool savedFacePlayerWhenInRange;

    private void Reset()
    {
        hint = GetComponent<NPCFacingHint>();
        boatBoarding = FindFirstObjectByType<BoatBoarding>();
        guide = GetComponent<GuideOrbAI>();
    }

    private void Awake()
    {
        if (hint == null)
            hint = GetComponent<NPCFacingHint>();

        if (boatBoarding == null)
            boatBoarding = FindFirstObjectByType<BoatBoarding>();

        if (guide == null)
            guide = GetComponent<GuideOrbAI>();
    }

    private void Start()
    {
        dialogueComplete = false;
        currentLineIndex = -1;

        SaveGuideState();
        SetGuidePaused(true);

        if (hint != null)
        {
            savedFacePlayerWhenInRange = hint.facePlayerWhenInRange;
            hint.facePlayerWhenInRange = facePlayerDuringDialogue;
        }

        if (boatBoarding != null)
            boatBoarding.SetBoardingAllowed(false);

        if (hint != null)
            hint.SetInRangeText(talkPrompt);
    }

    private void Update()
    {
        if (dialogueComplete) return;
        if (hint == null || !hint.PlayerInRange) return;
        if (!Input.GetKeyDown(interactKey)) return;

        AdvanceDialogue();
    }

    private void AdvanceDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            CompleteDialogue();
            return;
        }

        bool isFinalLineShowing = currentLineIndex >= dialogueLines.Length - 1;
        if (isFinalLineShowing)
        {
            CompleteDialogue();
            return;
        }

        currentLineIndex++;
        ShowLine(currentLineIndex);

        if (autoCompleteAfterFinalLine && currentLineIndex >= dialogueLines.Length - 1)
        {
            if (completeRoutine != null)
                StopCoroutine(completeRoutine);

            completeRoutine = StartCoroutine(CompleteAfterDelay());
        }
    }

    private void ShowLine(int lineIndex)
    {
        if (hint == null) return;
        if (lineIndex < 0 || lineIndex >= dialogueLines.Length) return;

        hint.SetInRangeText(dialogueLines[lineIndex]);
    }

    private IEnumerator CompleteAfterDelay()
    {
        yield return new WaitForSeconds(finalLineHoldTime);
        CompleteDialogue();
    }

    private void CompleteDialogue()
    {
        if (dialogueComplete) return;

        dialogueComplete = true;

        if (completeRoutine != null)
        {
            StopCoroutine(completeRoutine);
            completeRoutine = null;
        }

        if (hint != null)
        {
            hint.SetInRangeText(boardPrompt);
            hint.facePlayerWhenInRange = facePlayerAfterDialogue && savedFacePlayerWhenInRange;
            hint.faceOnlyWhenStopped = true;
        }

        if (boatBoarding != null)
            boatBoarding.SetBoardingAllowed(true);

        SetGuidePaused(false);
    }

    private void SaveGuideState()
    {
        if (guide == null) return;

        savedGuideState = guide.state;
        savedGuideTarget = guide.targetPoint;
    }

    private void SetGuidePaused(bool paused)
    {
        if (!pauseGuideUntilComplete || guide == null) return;

        if (paused)
        {
            guide.state = GuideOrbAI.State.PromptInteract;

            if (guide.agent != null && guide.agent.enabled && guide.agent.isOnNavMesh)
            {
                guide.agent.isStopped = true;
                guide.agent.ResetPath();
            }

            return;
        }

        guide.targetPoint = savedGuideTarget;
        guide.state = savedGuideState;

        if (guide.agent != null && guide.agent.enabled && guide.agent.isOnNavMesh)
        {
            bool shouldMove = savedGuideState == GuideOrbAI.State.FollowPlayer ||
                              savedGuideState == GuideOrbAI.State.LeadToTarget;

            guide.agent.isStopped = !shouldMove;
        }
    }
}
