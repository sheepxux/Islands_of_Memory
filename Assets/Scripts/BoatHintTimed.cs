using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoatHintTimed : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public Text uiText;
    public TMP_Text tmpText;

    [Header("Default Content")]
    [TextArea] public string defaultMessage = "Use A/D to row the oars.";
    public float showSeconds = 3.5f;

    [Header("Style")]
    public Color textColor = Color.white;

    Coroutine co;

    private void Awake()
    {
        HideImmediate();
    }

    public void ShowOnce()
    {
        ShowOnce(defaultMessage, textColor);
    }

    public void ShowOnce(string message)
    {
        ShowOnce(message, textColor);
    }

    public void ShowOnce(string message, Color color)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoShow(message, color));
    }

    IEnumerator CoShow(string message, Color color)
    {
        if (root != null) root.SetActive(true);

        if (uiText != null)
        {
            uiText.text = message;
            uiText.color = color;
        }

        if (tmpText != null)
        {
            tmpText.text = message;
            tmpText.color = color;
        }

        yield return new WaitForSeconds(showSeconds);

        HideImmediate();
        co = null;
    }

    public void HideImmediate()
    {
        if (root != null) root.SetActive(false);
    }
}