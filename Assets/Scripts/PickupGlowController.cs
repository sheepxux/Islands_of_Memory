using UnityEngine;

public class PickupGlowController : MonoBehaviour
{
    public GameObject glowObject;
    public float fadeSpeed = 6f;

    float target = 0f;
    float current = 0f;

    Renderer[] renderers;
    Color[] baseColors;

    void Awake()
    {
        if (glowObject == null) return;

        renderers = glowObject.GetComponentsInChildren<Renderer>(true);
        baseColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            baseColors[i] = renderers[i].material.GetColor("_BaseColor");
        }

        glowObject.SetActive(false);
    }

    void Update()
    {
        if (glowObject == null || renderers == null) return;

        float next = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);
        if (Mathf.Approximately(next, current)) return;
        current = next;

        if (current > 0f && !glowObject.activeSelf) glowObject.SetActive(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            Color c = baseColors[i];
            c.a = current;

            if (r.material.HasProperty("_BaseColor"))
                r.material.SetColor("_BaseColor", c);
        }

        if (current <= 0f && glowObject.activeSelf) glowObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        target = 1f;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        target = 0f;
    }
}