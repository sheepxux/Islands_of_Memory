using UnityEngine;
using UnityEngine.AI;

public class FadeWhileMovingToTarget : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public Transform targetPoint;

    [Header("Fade (based on remaining distance)")]
    public float fadeStartRemaining = 3.0f;
    public float fadeEndRemaining = 0.8f;

    [Range(0f, 1f)] public float minAlpha = 0.05f;
    [Range(0f, 1f)] public float maxAlpha = 1.0f;

    public float curvePower = 1.0f;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        if (agent == null || targetPoint == null) return;
        if (renderers == null || renderers.Length == 0) return;

        float remaining = agent.hasPath ? agent.remainingDistance
                                        : Vector3.Distance(agent.transform.position, targetPoint.position);

        float t = Mathf.InverseLerp(fadeEndRemaining, fadeStartRemaining, remaining);
        t = Mathf.Clamp01(t);

        if (curvePower != 1f) t = Mathf.Pow(t, curvePower);

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        ApplyAlpha(alpha);
    }

    private void ApplyAlpha(float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null || r.sharedMaterial == null) continue;

            r.GetPropertyBlock(mpb);

            if (r.sharedMaterial.HasProperty(BaseColorId))
            {
                Color c = r.sharedMaterial.GetColor(BaseColorId);
                c.a = alpha;
                mpb.SetColor(BaseColorId, c);
            }
            else if (r.sharedMaterial.HasProperty(ColorId))
            {
                Color c = r.sharedMaterial.GetColor(ColorId);
                c.a = alpha;
                mpb.SetColor(ColorId, c);
            }

            r.SetPropertyBlock(mpb);
        }
    }
}