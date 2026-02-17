using UnityEngine;

public class MetarrowAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource mainChannelSource;

    // singleton
    public static MetarrowAudioManager Instance { get; private set; }
    [SerializeField] private AudioClip levelCompleteClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        MetarrowGameManager.Instance.levelCompleted += PlayLevelCompleteSound;
    }

    private void OnDisable()
    {
        if (MetarrowGameManager.Instance != null)
        {
            MetarrowGameManager.Instance.levelCompleted -= PlayLevelCompleteSound;
        }
    }

    private void PlayLevelCompleteSound()
    {
        mainChannelSource?.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        PlaySound(clip, 1f);
    }

    public void PlaySound(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;

        if (mainChannelSource == null)
        {
            Debug.LogWarning("MetarrowAudioManager.PlaySound skipped: mainChannelSource is not assigned.");
            return;
        }

        mainChannelSource.PlayOneShot(clip, volumeScale);
    }
}
