using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject endingPanel;
    public RectTransform letterMask;
    public RectTransform letterText;
    public Button quitButton;

    [Header("Player")]
    public MonoBehaviour playerMovement;

    [Header("Timing")]
    public float showDelay = 1.6f;
    public float textX = 0f;
    public float startY = -260f;
    public float endY = 260f;
    public float scrollDuration = 12f;
    public bool useMaskScrollBounds = true;

    private Coroutine endingRoutine;

    private void Reset()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            ResolveReferences();
    }

    private void Awake()
    {
        ResolveReferences();

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        SetActive(endingPanel, false);
    }

    public void ShowEnding()
    {
        if (endingRoutine != null)
            StopCoroutine(endingRoutine);

        endingRoutine = StartCoroutine(ShowEndingRoutine());
    }

    private IEnumerator ShowEndingRoutine()
    {
        yield return new WaitForSecondsRealtime(showDelay);

        Time.timeScale = 1f;

        if (playerMovement != null)
            playerMovement.enabled = false;

        SetActive(endingPanel, true);
        PrepareLetterMask();

        float actualStartY = startY;
        float actualEndY = endY;
        GetScrollBounds(out actualStartY, out actualEndY);
        SetLetterPosition(actualStartY);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, scrollDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetLetterPosition(Mathf.Lerp(actualStartY, actualEndY, t));

            yield return null;
        }

        SetLetterPosition(actualEndY);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ResolveReferences()
    {
        if (endingPanel == null)
            endingPanel = FindChildObject("EndingPanel");

        if (letterMask == null)
            letterMask = FindChildComponent<RectTransform>("LetterMask");

        if (letterText == null)
            letterText = FindChildComponent<RectTransform>("LetterText");

        if (quitButton == null)
            quitButton = FindChildComponent<Button>("EndingQuitButton");

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    private void PrepareLetterMask()
    {
        if (endingPanel != null)
            endingPanel.transform.SetAsLastSibling();

        if (letterMask != null)
        {
            letterMask.gameObject.SetActive(true);
            letterMask.SetAsLastSibling();

            Mask mask = letterMask.GetComponent<Mask>();
            if (mask != null)
                mask.showMaskGraphic = false;

            Image image = letterMask.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = Mathf.Max(color.a, 0.01f);
                image.color = color;
                image.canvasRenderer.cullTransparentMesh = false;
            }
        }

        if (letterText != null)
        {
            letterText.gameObject.SetActive(true);
            letterText.SetAsLastSibling();

            TMP_Text text = letterText.GetComponent<TMP_Text>();
            if (text != null)
            {
                Color color = text.color;
                color.a = 1f;
                text.color = color;
                text.ForceMeshUpdate();
            }
        }
    }

    private void GetScrollBounds(out float actualStartY, out float actualEndY)
    {
        actualStartY = startY;
        actualEndY = endY;

        if (!useMaskScrollBounds || letterMask == null || letterText == null)
            return;

        float maskHeight = letterMask.rect.height;
        float textHeight = letterText.rect.height;

        TMP_Text text = letterText.GetComponent<TMP_Text>();
        if (text != null)
            textHeight = Mathf.Max(textHeight, text.preferredHeight);

        float travel = (maskHeight + textHeight) * 0.5f;
        actualStartY = -travel;
        actualEndY = travel;
    }

    private void SetLetterPosition(float y)
    {
        if (letterText == null)
            return;

        Vector2 position = letterText.anchoredPosition;
        position.x = textX;
        position.y = y;
        letterText.anchoredPosition = position;
    }

    private GameObject FindChildObject(string childName)
    {
        Transform found = FindChildTransform(childName);
        return found != null ? found.gameObject : null;
    }

    private T FindChildComponent<T>(string childName) where T : Component
    {
        Transform found = FindChildTransform(childName);
        return found != null ? found.GetComponent<T>() : null;
    }

    private Transform FindChildTransform(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}
