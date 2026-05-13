using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleRewardAnimator : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public Image flashBackground;
    public Image pieceImage;
    public TMP_Text rewardText;

    [Header("Puzzle Sprites")]
    public Sprite[] pieceSprites = new Sprite[4];

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip rewardSfx;
    [Range(0f, 1f)] public float volume = 0.8f;

    [Header("Timing")]
    public float fadeInDuration = 0.2f;
    public float holdDuration = 0.9f;
    public float fadeOutDuration = 0.35f;
    public float startScale = 1.25f;
    public float endScale = 1f;
    public string message = "Memory Piece Restored";

    private Coroutine playRoutine;
    private MonoBehaviour coroutineHost;
    private static PuzzleRewardCoroutineRunner sharedRunner;

    private void Awake()
    {
        EnsureSetup();
        HideImmediate();
    }

    private void EnsureSetup()
    {
        if (root == null)
            root = gameObject;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void Play(int pieceIndex)
    {
        if (!IsValidIndex(pieceIndex)) return;

        EnsureSetup();

        if (playRoutine != null && coroutineHost != null)
            coroutineHost.StopCoroutine(playRoutine);

        SetRootActive(true);
        coroutineHost = gameObject.activeInHierarchy ? this : GetCoroutineRunner();
        playRoutine = coroutineHost.StartCoroutine(PlayRoutine(pieceIndex));
    }

    private IEnumerator PlayRoutine(int pieceIndex)
    {
        if (pieceImage != null)
            pieceImage.sprite = pieceSprites[pieceIndex];

        if (rewardText != null)
            rewardText.text = message;

        SetAlpha(0f);
        SetRootActive(true);
        SetPieceScale(startScale);

        if (audioSource != null && rewardSfx != null)
            audioSource.PlayOneShot(rewardSfx, volume);

        yield return Animate(0f, 1f, fadeInDuration, true);
        yield return new WaitForSeconds(holdDuration);
        yield return Animate(1f, 0f, fadeOutDuration, false);

        SetRootActive(false);
        playRoutine = null;
        coroutineHost = null;
    }

    private IEnumerator Animate(float from, float to, float duration, bool scaleDown)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            SetAlpha(Mathf.Lerp(from, to, eased));

            if (scaleDown)
                SetPieceScale(Mathf.Lerp(startScale, endScale, eased));

            yield return null;
        }

        SetAlpha(to);
    }

    private void HideImmediate()
    {
        SetAlpha(0f);
        SetRootActive(false);
    }

    private void SetAlpha(float alpha)
    {
        SetGraphicAlpha(flashBackground, alpha * 0.35f);
        SetGraphicAlpha(pieceImage, alpha);
        SetGraphicAlpha(rewardText, alpha);
    }

    private void SetGraphicAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;

        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private void SetPieceScale(float scale)
    {
        if (pieceImage == null) return;

        pieceImage.rectTransform.localScale = Vector3.one * scale;
    }

    private void SetRootActive(bool active)
    {
        if (root != null)
            root.SetActive(active);
    }

    private bool IsValidIndex(int index)
    {
        return pieceSprites != null && index >= 0 && index < pieceSprites.Length && pieceSprites[index] != null;
    }

    private static MonoBehaviour GetCoroutineRunner()
    {
        if (sharedRunner != null)
            return sharedRunner;

        GameObject runnerObject = new GameObject("PuzzleRewardCoroutineRunner");
        DontDestroyOnLoad(runnerObject);
        sharedRunner = runnerObject.AddComponent<PuzzleRewardCoroutineRunner>();
        return sharedRunner;
    }

    private class PuzzleRewardCoroutineRunner : MonoBehaviour
    {
    }
}
