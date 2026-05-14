using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathRespawnUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;

    [Header("Groups")]
    public CanvasGroup blackFadeGroup;
    public CanvasGroup contentGroup;

    [Header("Content")]
    public Image memoryImage;
    public TMP_Text messageText;
    public Button returnButton;
    public TMP_Text returnButtonText;

    [Header("Text")]
    public string message = "You will awaken again in her memory.";
    public string returnPrompt = "Press SPACE to return to memory";

    [Header("Input")]
    public KeyCode returnKey = KeyCode.Space;

    [Header("Timing")]
    public float fadeToBlackDuration = 1f;
    public float contentFadeDuration = 1f;
    public float fadeOutDuration = 0.45f;

    private bool returnRequested;

    public bool IsReady => root != null && blackFadeGroup != null && contentGroup != null;

    private void Awake()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(RequestReturn);

        HideImmediate();
    }

    public IEnumerator PlayIntroAndWait()
    {
        if (!IsReady)
            yield break;

        returnRequested = false;

        ShowRoot();

        ApplyText();
        SetButtonInteractable(false);
        SetAlpha(blackFadeGroup, 0f);
        SetAlpha(contentGroup, 0f);

        yield return null;

        yield return FadeGroup(blackFadeGroup, 0f, 1f, fadeToBlackDuration);
        yield return FadeGroup(contentGroup, 0f, 1f, contentFadeDuration);

        SetButtonInteractable(true);

        while (!returnRequested)
        {
            if (Input.GetKeyDown(returnKey))
                returnRequested = true;

            yield return null;
        }
    }

    public IEnumerator PlayOutro()
    {
        SetButtonInteractable(false);

        yield return FadeGroup(contentGroup, GetAlpha(contentGroup), 0f, fadeOutDuration * 0.6f);
        yield return FadeGroup(blackFadeGroup, GetAlpha(blackFadeGroup), 0f, fadeOutDuration);

        HideImmediate();
    }

    public void HideImmediate()
    {
        SetButtonInteractable(false);
        SetAlpha(blackFadeGroup, 0f);
        SetAlpha(contentGroup, 0f);

        if (root != null)
            root.SetActive(false);
    }

    private void ApplyText()
    {
        if (messageText != null)
            messageText.text = message;

        if (returnButtonText != null)
            returnButtonText.text = returnPrompt;
    }

    private void RequestReturn()
    {
        returnRequested = true;
    }

    private void ShowRoot()
    {
        if (root == null)
            return;

        root.SetActive(true);
        root.transform.SetAsLastSibling();

        Canvas rootCanvas = root.GetComponent<Canvas>();
        if (rootCanvas == null)
            rootCanvas = root.AddComponent<Canvas>();

        rootCanvas.overrideSorting = true;
        rootCanvas.sortingOrder = 500;

        GraphicRaycaster raycaster = root.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            root.AddComponent<GraphicRaycaster>();

        if (blackFadeGroup != null)
        {
            blackFadeGroup.gameObject.SetActive(true);
            blackFadeGroup.blocksRaycasts = true;
            blackFadeGroup.interactable = false;
        }

        if (contentGroup != null)
        {
            contentGroup.gameObject.SetActive(true);
            contentGroup.blocksRaycasts = true;
            contentGroup.interactable = true;
        }

        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
            yield break;

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        group.alpha = from;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        group.alpha = to;
    }

    private void SetButtonInteractable(bool interactable)
    {
        if (returnButton != null)
            returnButton.interactable = interactable;
    }

    private void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null)
            group.alpha = alpha;
    }

    private float GetAlpha(CanvasGroup group)
    {
        return group != null ? group.alpha : 0f;
    }
}
