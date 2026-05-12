using TMPro;
using UnityEngine;

public class FishingMinigameController : MonoBehaviour
{
    private enum FishingState
    {
        Idle,
        Tutorial,
        Playing,
        Complete
    }

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode timingKey = KeyCode.Space;
    public GameObject promptRoot;

    [Header("Player")]
    public GameObject player;
    public MonoBehaviour playerMovement;

    [Header("Camera")]
    public Camera mainCamera;
    public TopDownCamera topDownCamera;
    public Transform fishingViewPoint;

    [Header("Objects")]
    public GameObject fishingRod;

    [Header("Tutorial")]
    public GameObject tutorialRoot;

    [Header("Timing UI")]
    public GameObject gameUiRoot;
    public RectTransform bar;
    public RectTransform marker;
    public RectTransform successZone;
    public TMP_Text catchText;
    public TMP_Text feedbackText;

    [Header("Catch Visuals")]
    public GameObject[] catchVisuals;

    [Header("Audio")]
    public BGMManager bgmManager;
    public AudioClip fishingBgm;
    public AudioSource sfxSource;
    public AudioClip catchSfx;
    public AudioClip missSfx;
    public AudioClip completeSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float fishingBgmVolume = 0.35f;

    [Header("Puzzle Reward")]
    public PuzzleProgress puzzleProgress;
    public int summerPieceIndex = 2;

    [Header("Tuning")]
    public int requiredCatches = 5;
    public float baseMarkerSpeed = 0.45f;
    public float speedIncreasePerCatch = 0.18f;
    public float defaultSuccessMin = 0.42f;
    public float defaultSuccessMax = 0.58f;

    private FishingState state = FishingState.Idle;
    private bool playerInside;
    private int catchCount;
    private int speedLevel;
    private float markerPosition = 0.5f;
    private float markerDirection = 1f;
    private float tutorialStartTime;
    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;
    private AudioClip previousBgm;
    private float previousBgmVolume;
    private bool usingFishingMusic;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (player != null && playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();

        if (bgmManager == null)
            bgmManager = FindFirstObjectByType<BGMManager>();

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        SetActive(promptRoot, false);
        SetActive(tutorialRoot, false);
        SetActive(gameUiRoot, false);
        SetActive(fishingRod, false);
        HideCatchVisuals();
        UpdateCatchText();
    }

    private void Update()
    {
        if (state == FishingState.Idle)
        {
            if (playerInside && Input.GetKeyDown(interactKey))
            {
                BeginTutorial();
                return;
            }
        }

        if (state == FishingState.Tutorial)
        {
            if (Time.unscaledTime - tutorialStartTime > 0.2f && Input.anyKeyDown)
            {
                BeginGame();
                return;
            }
        }

        if (state == FishingState.Playing)
        {
            TickMarker();

            if (Input.GetKeyDown(timingKey))
                TryCatch();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (state != FishingState.Idle) return;

        playerInside = true;
        SetActive(promptRoot, true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (state != FishingState.Idle) return;

        playerInside = true;
        SetActive(promptRoot, true);

        if (Input.GetKeyDown(interactKey))
            BeginTutorial();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (state == FishingState.Idle)
            SetActive(promptRoot, false);
    }

    private void BeginTutorial()
    {
        state = FishingState.Tutorial;
        tutorialStartTime = Time.unscaledTime;
        SetActive(promptRoot, false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (mainCamera != null)
        {
            savedCameraPosition = mainCamera.transform.position;
            savedCameraRotation = mainCamera.transform.rotation;
        }

        if (topDownCamera != null)
            topDownCamera.enabled = false;

        if (mainCamera != null && fishingViewPoint != null)
        {
            mainCamera.transform.position = fishingViewPoint.position;
            mainCamera.transform.rotation = fishingViewPoint.rotation;
        }

        StartFishingMusic();
        SetActive(tutorialRoot, true);
    }

    private void BeginGame()
    {
        state = FishingState.Playing;
        catchCount = 0;
        speedLevel = 0;
        markerPosition = 0.5f;
        markerDirection = 1f;

        SetActive(tutorialRoot, false);
        SetActive(fishingRod, true);
        SetActive(gameUiRoot, true);

        SetFeedback("Press SPACE in the bright zone");
        UpdateCatchText();
        RandomizeSuccessZone();
        UpdateMarkerVisual();
    }

    private void TickMarker()
    {
        float speed = baseMarkerSpeed + speedIncreasePerCatch * speedLevel;
        markerPosition += markerDirection * speed * Time.deltaTime;

        if (markerPosition >= 1f)
        {
            markerPosition = 1f;
            markerDirection = -1f;
        }
        else if (markerPosition <= 0f)
        {
            markerPosition = 0f;
            markerDirection = 1f;
        }

        UpdateMarkerVisual();
    }

    private void TryCatch()
    {
        GetSuccessRange(out float min, out float max);
        bool success = markerPosition >= min && markerPosition <= max;

        if (success)
        {
            catchCount++;
            speedLevel = Mathf.Min(speedLevel + 1, requiredCatches - 1);
            SetFeedback("Catch!");
            PlaySfx(catchSfx);
            ShowCatchVisual(catchCount - 1);
        }
        else
        {
            speedLevel = Mathf.Max(0, speedLevel - 1);
            SetFeedback("Miss. The rhythm slows down.");
            PlaySfx(missSfx);
        }

        UpdateCatchText();

        if (catchCount >= requiredCatches)
        {
            CompleteFishing();
        }
        else
        {
            RandomizeSuccessZone();
        }
    }

    private void CompleteFishing()
    {
        state = FishingState.Complete;

        if (puzzleProgress != null)
            puzzleProgress.UnlockPiece(summerPieceIndex);

        SetFeedback("Summer memory restored");
        PlaySfx(completeSfx);
        Invoke(nameof(ExitFishing), 1.2f);
    }

    private void ExitFishing()
    {
        SetActive(tutorialRoot, false);
        SetActive(gameUiRoot, false);
        SetActive(fishingRod, false);
        HideCatchVisuals();

        if (mainCamera != null)
        {
            mainCamera.transform.position = savedCameraPosition;
            mainCamera.transform.rotation = savedCameraRotation;
        }

        if (topDownCamera != null)
            topDownCamera.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = true;

        RestorePreviousMusic();
        state = FishingState.Complete;
    }

    private void UpdateMarkerVisual()
    {
        if (marker == null || bar == null) return;

        float width = bar.rect.width;
        Vector2 anchored = marker.anchoredPosition;
        anchored.x = (markerPosition - 0.5f) * width;
        marker.anchoredPosition = anchored;
    }

    private void GetSuccessRange(out float min, out float max)
    {
        min = defaultSuccessMin;
        max = defaultSuccessMax;

        if (successZone == null || bar == null) return;

        float barWidth = Mathf.Max(1f, bar.rect.width);
        float zoneWidth = successZone.rect.width;
        float zoneCenter = successZone.anchoredPosition.x + barWidth * 0.5f;

        min = Mathf.Clamp01((zoneCenter - zoneWidth * 0.5f) / barWidth);
        max = Mathf.Clamp01((zoneCenter + zoneWidth * 0.5f) / barWidth);
    }

    private void RandomizeSuccessZone()
    {
        if (successZone == null || bar == null) return;

        float barWidth = Mathf.Max(1f, bar.rect.width);
        float zoneWidth = Mathf.Min(successZone.rect.width, barWidth);
        float halfRange = (barWidth - zoneWidth) * 0.5f;

        Vector2 anchored = successZone.anchoredPosition;
        anchored.x = Random.Range(-halfRange, halfRange);
        successZone.anchoredPosition = anchored;
    }

    private void ShowCatchVisual(int index)
    {
        if (catchVisuals == null) return;
        if (index < 0 || index >= catchVisuals.Length) return;

        SetActive(catchVisuals[index], true);
    }

    private void HideCatchVisuals()
    {
        if (catchVisuals == null) return;

        foreach (GameObject visual in catchVisuals)
            SetActive(visual, false);
    }

    private void StartFishingMusic()
    {
        if (bgmManager == null || fishingBgm == null) return;

        previousBgm = bgmManager.CurrentClip;
        previousBgmVolume = bgmManager.volume;
        usingFishingMusic = true;
        bgmManager.SetVolume(fishingBgmVolume);
        bgmManager.Play(fishingBgm);
    }

    private void RestorePreviousMusic()
    {
        if (!usingFishingMusic) return;
        if (bgmManager == null) return;

        bgmManager.SetVolume(previousBgmVolume);

        if (previousBgm != null)
            bgmManager.Play(previousBgm);

        usingFishingMusic = false;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private void UpdateCatchText()
    {
        if (catchText != null)
            catchText.text = "Catch " + catchCount + " / " + requiredCatches;
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }

    private void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}
