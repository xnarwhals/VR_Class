using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VRGlock : MonoBehaviour
{
    [Header("Gun Interactable")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable gunInteractable;

    [Header("Magazine")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor magazineSocket;
    [SerializeField] private string acceptedMagazineId = "Glock";

    private Magazine currentMagazine;

    [Header("Shoot Settings")]
    [SerializeField] private float cooldownBetweenShots = 0.2f;
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private float maxRange = 50f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private Tracer tracerPrefab;
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private float impactLifetime = 1.0f;

    [Header("Magazine Eject")]
    [SerializeField] private InputActionProperty ejectAction;
    [SerializeField] private float ejectCooldown = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource gunAudioSource;
    [SerializeField] private AudioClip magInsertClip;
    [SerializeField] private AudioClip emptyMagClip;
    [SerializeField] private AudioClip gunShotClip;

    private float nextAllowedShotTime;
    private float nextAllowedEjectTime;

    private void Awake()
    {
        if (gunInteractable == null)
        {
            gunInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        if (gunInteractable != null)
        {
            gunInteractable.activated.AddListener(OnGunActivated);
        }

        if (magazineSocket != null)
        {
            magazineSocket.selectEntered.AddListener(OnMagazineInserted);
            magazineSocket.selectExited.AddListener(OnMagazineRemoved);
        }
    }

    private void OnEnable()
    {
        if (ejectAction.action != null)
        {
            ejectAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (ejectAction.action != null)
        {
            ejectAction.action.Disable();
        }
    }

    private void OnDestroy()
    {
        if (gunInteractable != null)
        {
            gunInteractable.activated.RemoveListener(OnGunActivated);
        }

        if (magazineSocket != null)
        {
            magazineSocket.selectEntered.RemoveListener(OnMagazineInserted);
            magazineSocket.selectExited.RemoveListener(OnMagazineRemoved);
        }
    }

    private void Update()
    {
        if (ejectAction.action == null)
        {
            return;
        }

        if (!ejectAction.action.WasPerformedThisFrame())
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now < nextAllowedEjectTime)
        {
            return;
        }

        nextAllowedEjectTime = now + Mathf.Max(0f, ejectCooldown);
        TryEjectMagazine();
    }

    private void OnMagazineInserted(SelectEnterEventArgs args)
    {
        var magazine = args.interactableObject.transform.GetComponentInParent<Magazine>();
        if (magazine == null)
        {
            return;
        }

        if (!IsCompatible(magazine))
        {
            ForceEject(args.interactableObject);
            return;
        }

        currentMagazine = magazine;
        magazine.SetLoaded(true);
        PlayClip(magInsertClip);
    }

    private void OnMagazineRemoved(SelectExitEventArgs args)
    {
        var magazine = args.interactableObject.transform.GetComponentInParent<Magazine>();
        if (magazine == null)
        {
            return;
        }

        if (currentMagazine == magazine)
        {
            currentMagazine = null;
        }

        magazine.SetLoaded(false);
    }

    private void OnGunActivated(ActivateEventArgs args)
    {
        float now = Time.unscaledTime;
        if (now < nextAllowedShotTime)
        {
            return;
        }

        nextAllowedShotTime = now + Mathf.Max(0f, cooldownBetweenShots);

        if (currentMagazine != null && currentMagazine.TryConsumeRound())
        {
            PlayClip(gunShotClip);
            Shoot();
            return;
        }

        PlayClip(emptyMagClip);
    }

    public void AttachMagazine(Magazine magazine)
    {
        if (magazineSocket == null || magazine == null)
        {
            return;
        }

        var interactable = magazine.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable == null)
        {
            return;
        }

        if (magazineSocket.interactionManager == null)
        {
            return;
        }

        magazineSocket.interactionManager.SelectEnter(
            (UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)magazineSocket,
            (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)interactable
        );
    }

    private bool IsCompatible(Magazine magazine)
    {
        if (string.IsNullOrWhiteSpace(acceptedMagazineId))
        {
            return true;
        }

        return magazine.MagazineId == acceptedMagazineId;
    }

    private void ForceEject(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable)
    {
        if (magazineSocket == null || magazineSocket.interactionManager == null)
        {
            return;
        }

        magazineSocket.interactionManager.SelectExit(magazineSocket, interactable);
    }

    private void PlayClip(AudioClip clip)
    {
        if (gunAudioSource == null || clip == null)
        {
            return;
        }

        gunAudioSource.PlayOneShot(clip);
    }

    private void Shoot()
    {
        if (muzzleTransform == null)
        {
            return;
        }

        Vector3 origin = muzzleTransform.position;
        Vector3 direction = muzzleTransform.forward;

        Vector3 endPoint = origin + direction * maxRange;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;
            TryPlayHitSfx(hit);
            SpawnImpact(hit);
        }

        SpawnTracer(origin, endPoint);
    }

    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (tracerPrefab == null)
        {
            return;
        }

        Tracer tracer = Instantiate(tracerPrefab, start, Quaternion.identity);
        tracer.SetPositions(start, end);
    }

    private void SpawnImpact(RaycastHit hit)
    {
        if (impactPrefab == null)
        {
            return;
        }

        Quaternion rotation = Quaternion.LookRotation(hit.normal);
        GameObject impact = Instantiate(impactPrefab, hit.point, rotation);
        if (impactLifetime > 0f)
        {
            Destroy(impact, impactLifetime);
        }
    }

    private void TryEjectMagazine()
    {
        if (magazineSocket == null || magazineSocket.interactionManager == null)
        {
            return;
        }

        if (!magazineSocket.hasSelection)
        {
            return;
        }

        var selected = magazineSocket.firstInteractableSelected;
        if (selected == null)
        {
            return;
        }

        ForceEject(selected);
    }

    private void TryPlayHitSfx(RaycastHit hit)
    {
        var receiver = hit.collider.GetComponentInParent<HitSfxReceiver>();
        if (receiver == null)
        {
            return;
        }

        receiver.PlayHit();
    }
}
