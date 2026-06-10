using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnStart = true;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        if (musicClip == null)
        {
            Debug.LogWarning("SceneMusicPlayer has no music clip assigned.");
            return;
        }

        audioSource.clip = musicClip;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}