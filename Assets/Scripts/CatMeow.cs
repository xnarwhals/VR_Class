using UnityEngine;

public class CatMeow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CatRoam catRoam;
    [SerializeField] private AudioManager audioManager;

    [Header("Pet Rules")]
    [SerializeField] private string handTag = "PlayerHand";
    [SerializeField] private float petCooldown = 2.5f;
    [SerializeField] private float stopRoamDuration = 1.5f;

    private float nextPetTime;

    private void Awake()
    {
        if (catRoam == null)
            catRoam = GetComponent<CatRoam>();

        if (audioManager == null) // cache
            audioManager = AudioManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (!string.IsNullOrEmpty(handTag) && !other.CompareTag(handTag))
            return;

        if (Time.time < nextPetTime)
            return;

        nextPetTime = Time.time + petCooldown;

        if (catRoam != null)
            catRoam.PauseRoam(stopRoamDuration);

        if (audioManager != null)
            audioManager.PlayCatMeow();
    }
}
