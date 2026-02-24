using UnityEngine;

[DisallowMultipleComponent]
public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    private AudioSource audioSource;

    [Header("Optional")]
    public AudioClip bgmClip;
    [Range(0f, 1f)] public float volume = 0.35f;
    public bool playOnStart = true;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;

        if (bgmClip != null && audioSource.clip == null)
            audioSource.clip = bgmClip;

        if (playOnStart && audioSource.clip != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (audioSource != null) audioSource.volume = volume;
    }

    public void Play( AudioClip clip = null )
    {
        if (audioSource == null) return;

        if (clip != null && audioSource.clip != clip)
            audioSource.clip = clip;

        if (audioSource.clip != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    public void Stop()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}