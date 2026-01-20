using UnityEngine;
using UnityEngine.Events;

public class PlayerEatApple : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private float destroyDelay = 0.05f;

    [Header("Status Effects")]
    [SerializeField] private UnityEvent onRedAppleEaten;
    [SerializeField] private UnityEvent onBlueAppleEaten;
    [SerializeField] private UnityEvent onGreenAppleEaten;
    [SerializeField] private UnityEvent onYellowAppleEaten;
    [SerializeField] private UnityEvent onAnyAppleEaten;

    private void Awake()
    {
        if (audioManager == null)
            audioManager = AudioManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        var apple = other.GetComponentInParent<AppleID>();
        if (apple == null || !apple.TryConsume())
            return;

        ApplyStatusEffect(apple.Color);
        onAnyAppleEaten?.Invoke();

        if (audioManager != null)
            audioManager.PlayEatApple();

        Destroy(apple.gameObject, destroyDelay);
    }

    private void ApplyStatusEffect(AppleColor color)
    {
        switch (color)
        {
            case AppleColor.Red:
                onRedAppleEaten?.Invoke();
                break;
            case AppleColor.Blue:
                onBlueAppleEaten?.Invoke();
                break;
            case AppleColor.Green:
                onGreenAppleEaten?.Invoke();
                break;
            case AppleColor.Yellow:
                onYellowAppleEaten?.Invoke();
                break;
        }
    }
}
