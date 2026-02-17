using UnityEngine;

public class ExplodeArrow : Arrow
{
    [SerializeField] private AudioClip explosionHitSound;
    [SerializeField] private float explosionVolumeScale = 1.6f;

    protected override void OnArrowHit(RaycastHit hit, BaseTarget target)
    {
        PlayExplosionSound();

        Fracture fracture = hit.transform.GetComponent<Fracture>();
        if (fracture == null)
        {
            fracture = hit.transform.GetComponentInParent<Fracture>();
        }
        
        if (fracture != null)
        {
            if (fracture.TryGetComponent(out FractureExplosionEffect explosionEffect))
            {
                explosionEffect.CauseExplosionFracture(hit.point);
            }
            else
            {
                fracture.CauseFracture();
            }
        }
    }

    private void PlayExplosionSound()
    {
        if (explosionHitSound == null)
        {
            return;
        }

        MetarrowAudioManager audioManager = MetarrowAudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlaySound(explosionHitSound, explosionVolumeScale);
        }
    }
}
