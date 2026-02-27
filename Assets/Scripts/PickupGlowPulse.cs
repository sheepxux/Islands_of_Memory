using UnityEngine;

public class PickupGlowPulse : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color emissionColor = Color.white;

    [Header("Pulse")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 2.0f;
    [SerializeField] private float pulseSpeed = 2.0f;

    [Header("Visibility")]
    [SerializeField] private bool startHidden = true;

    private Material mat;
    private bool isVisible;
    private float distanceFactor = 1f;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null) mat = targetRenderer.material;

        isVisible = !startHidden;
        ApplyEmission(isVisible ? minIntensity : 0f);
    }

    void Update()
    {
        if (!isVisible || mat == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float pulse = Mathf.Lerp(minIntensity, maxIntensity, t);

        float intensity = pulse * Mathf.Clamp01(distanceFactor);
        ApplyEmission(intensity);
    }

    public void Show()
    {
        isVisible = true;
        ApplyEmission(minIntensity);
    }

    public void Hide()
    {
        isVisible = false;
        ApplyEmission(0f);
    }

    public void SetDistanceFactor(float factor01)
    {
        distanceFactor = Mathf.Clamp01(factor01);
    }

    private void ApplyEmission(float intensity)
    {
        if (mat == null) return;

        Color c = emissionColor * Mathf.LinearToGammaSpace(intensity);
        mat.SetColor(EmissionColorId, c);
    }
}