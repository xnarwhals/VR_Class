using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using System;


public class PullInteraction : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    public static event Action<float> pullActionReleased;
    public Transform start, end;
    public GameObject notch;

    public float pullAmmount { get; private set; } = 0f;
    private LineRenderer _lineRenderer;
    private AudioSource _audioSource;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor pullingInteractor;

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        pullingInteractor = args.interactorObject;
    }

    public void Release() {
        pullActionReleased?.Invoke(pullAmmount);
        pullingInteractor = null;
        pullAmmount = 0f;
        notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, 0f);
        UpdateString();
        PlayReleaseSound();
    }


    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                Vector3 pullPosition = pullingInteractor.transform.position;
                pullAmmount = CalculatePullAmount(pullPosition);
                UpdateString();
                HapticFeedback();
            }
        }
    }

    private float CalculatePullAmount(Vector3 pullPosition)
    {
        Vector3 startToEnd = end.position - start.position;
        Vector3 startToPull = pullPosition - start.position;

        float pullLength = startToEnd.magnitude;
        startToEnd.Normalize();


        float pullValue = Vector3.Dot(startToPull, startToEnd) / pullLength;

        return Mathf.Clamp(pullValue, 0f, 1f);
    }

    private void UpdateString()
    {
        Vector3 linePosition = Vector3.forward * Mathf.Lerp(start.transform.localPosition.z, end.transform.localPosition.z, pullAmmount);
        notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, linePosition.z + 0.2f);
        _lineRenderer.SetPosition(1, linePosition);
    }

    private void HapticFeedback() {
        if (pullingInteractor == null) return;
        var hapticPlayer = pullingInteractor.transform.GetComponentInParent<HapticImpulsePlayer>(true);
        if (hapticPlayer != null)
            hapticPlayer.SendHapticImpulse(pullAmmount, 0.1f);
    }

    private void PlayReleaseSound() {
        if (_audioSource != null) {
            _audioSource.Play();
        }
    }



}
