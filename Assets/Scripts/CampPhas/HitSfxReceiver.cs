using UnityEngine;

public class HitSfxReceiver : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private float minInterval = 0.05f;

    private float nextAllowedTime;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayHit()
    {
        if (audioSource == null || hitClip == null)
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now < nextAllowedTime)
        {
            return;
        }

        nextAllowedTime = now + Mathf.Max(0f, minInterval);
        audioSource.PlayOneShot(hitClip);
    }
}
