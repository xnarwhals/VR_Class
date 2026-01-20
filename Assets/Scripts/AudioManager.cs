using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource loopSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip victoryJingle;
    [SerializeField] private AudioClip eatApple;
    [SerializeField] private AudioClip phoneRingLoop;
    [SerializeField] private AudioClip phonePickup;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (loopSource == null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.loop = true;
        }
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    public void PlayVictoryJingle() { PlayOneShot(victoryJingle); }
    public void PlayEatApple() { PlayOneShot(eatApple); }

    // when all puzzles are complete
    public void StartPhoneRing()
    {
        if (phoneRingLoop == null || loopSource == null)
            return;

        if (loopSource.isPlaying && loopSource.clip == phoneRingLoop)
            return;

        loopSource.clip = phoneRingLoop;
        loopSource.Play();
    }

    // called when phone is picked up
    public void StopPhoneRing()
    {
        if (loopSource == null)
            return;

        if (loopSource.isPlaying)
            loopSource.Stop();
    }

    // also called when phone is picked up
    public void PlayPhonePickup()
    {
        if (phonePickup != null)
            PlayOneShot(phonePickup);
    }
}
