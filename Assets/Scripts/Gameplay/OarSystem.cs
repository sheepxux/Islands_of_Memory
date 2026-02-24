using System.Collections;
using UnityEngine;

public class OarSystem : MonoBehaviour
{
    [Header("Oar Pivots")]
    public Transform oarPivotL;
    public Transform oarPivotR;

    [Header("Stroke Animation")]
    public float strokeDownAngleX = 28f;
    public float strokeBackAngleZ = 16f;
    public float downDuration = 0.10f;
    public float returnDuration = 0.14f;

    [Header("Splash VFX (Optional)")]
    public ParticleSystem splashPrefab;
    public Transform splashPointL;
    public Transform splashPointR;

    [Header("Audio (Optional)")]
    public AudioClip oarSplashClip;
    [Range(0f, 1f)] public float splashVolume = 0.7f;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    private Quaternion _lStartRot;
    private Quaternion _rStartRot;

    private Coroutine _lCo;
    private Coroutine _rCo;

    private void Awake()
    {
        if (oarPivotL != null) _lStartRot = oarPivotL.localRotation;
        if (oarPivotR != null) _rStartRot = oarPivotR.localRotation;
    }

    public void StrokeLeft()
    {
        if (oarPivotL == null) return;

        SpawnSplash(true);

        if (_lCo != null) StopCoroutine(_lCo);
        _lCo = StartCoroutine(StrokeRoutine(oarPivotL, _lStartRot, -1));
    }

    public void StrokeRight()
    {
        if (oarPivotR == null) return;

        SpawnSplash(false);

        if (_rCo != null) StopCoroutine(_rCo);
        _rCo = StartCoroutine(StrokeRoutine(oarPivotR, _rStartRot, +1));
    }

    private IEnumerator StrokeRoutine(Transform pivot, Quaternion startRot, int side)
    {
        Quaternion downRot = startRot * Quaternion.Euler(strokeDownAngleX, 0f, side * strokeBackAngleZ);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, downDuration);
            pivot.localRotation = Quaternion.Slerp(startRot, downRot, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, returnDuration);
            pivot.localRotation = Quaternion.Slerp(downRot, startRot, t);
            yield return null;
        }

        pivot.localRotation = startRot;
    }

    private void SpawnSplash(bool left)
    {
        // VFX
        if (splashPrefab != null)
        {
            Transform p = left ? splashPointL : splashPointR;
            if (p != null)
                Instantiate(splashPrefab, p.position, p.rotation);
        }

        // Audio
        if (oarSplashClip != null)
        {
            Transform p = left ? splashPointL : splashPointR;
            Vector3 pos = p != null ? p.position : transform.position;

            var go = new GameObject("OarSplashSFX");
            go.transform.position = pos;

            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 1f; // 3D
            src.minDistance = 2f;
            src.maxDistance = 25f;
            src.volume = splashVolume;
            src.pitch = Random.Range(pitchMin, pitchMax);
            src.playOnAwake = false;

            src.clip = oarSplashClip;
            src.Play();

            Destroy(go, oarSplashClip.length + 0.2f);
        }
    }
}