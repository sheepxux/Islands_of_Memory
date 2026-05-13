using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject endingPanel;
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
        yield return new WaitForSeconds(showDelay);

        Time.timeScale = 1f;

        if (playerMovement != null)
            playerMovement.enabled = false;

        SetActive(endingPanel, true);

        if (letterText != null)
        {
            Vector2 position = letterText.anchoredPosition;
            position.x = textX;
            position.y = startY;
            letterText.anchoredPosition = position;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, scrollDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (letterText != null)
            {
                Vector2 position = letterText.anchoredPosition;
                position.x = textX;
                position.y = Mathf.Lerp(startY, endY, t);
                letterText.anchoredPosition = position;
            }

            yield return null;
        }
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

        if (letterText == null)
            letterText = FindChildComponent<RectTransform>("LetterText");

        if (quitButton == null)
            quitButton = FindChildComponent<Button>("EndingQuitButton");

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();
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
