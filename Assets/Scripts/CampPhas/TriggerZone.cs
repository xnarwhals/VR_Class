using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EventTriggerZone : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    [Header("Man / NPC")]
    [SerializeField] private Animator manAnimator;
    [SerializeField] private string headSnap = "HeadSnap";
    [SerializeField] private string headSnapStateName = "HeadSnap";
    [SerializeField] private int headSnapLayer = 0;
    [SerializeField] private float headSnapTimeoutSeconds = 9.0f;
    [SerializeField] private NavMeshAgent manAgent;
    [SerializeField] private Transform manDestination;
    [SerializeField] private string manWalkBoolName = "IsWalking";
    [SerializeField] private MonsterRoam monsterRoamScript;

    [Header("HeadSnap Audio")]
    [SerializeField] private AudioSource headSnapAudioSource;
    [SerializeField] private AudioClip headSnapLoopClip;
    [SerializeField, Range(0f, 1f)] private float headSnapLoopVolume = 1f;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            Debug.Log("Collider does not have the player tag, ignoring.");
            return;
        }

        hasTriggered = true;

        DisableScripts();
        TriggerManAnimation();
        StartManMovement();

    }

    private void DisableScripts()
    {
        if (scriptsToDisable == null)
        {
            return;
        }

        for (int i = 0; i < scriptsToDisable.Length; i++)
        {
            if (scriptsToDisable[i] != null)
            {
                scriptsToDisable[i].enabled = false;
            }
        }
    }

    private void TriggerManAnimation()
    {
        if (manAnimator == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(headSnap))
        {
            Debug.Log("Playing Head Snap");
            manAnimator.SetTrigger(headSnap);
            StartHeadSnapAudio();
        }
    }


    private void StartManMovement()
    {
        StartCoroutine(EnableRoamAfterHeadSnap());
    }

    private IEnumerator EnableRoamAfterHeadSnap()
    {
        if (manAnimator != null && !string.IsNullOrEmpty(headSnapStateName))
        {
            float elapsed = 0f;

            // Wait until the head snap state is actually entered (or timeout).
            while (elapsed < headSnapTimeoutSeconds)
            {
                if (manAnimator.GetCurrentAnimatorStateInfo(headSnapLayer).IsName(headSnapStateName))
                {
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // If we entered the state, wait for it to finish (or timeout).
            while (elapsed < headSnapTimeoutSeconds)
            {
                var state = manAnimator.GetCurrentAnimatorStateInfo(headSnapLayer);
                if (!state.IsName(headSnapStateName) || state.normalizedTime >= 1f)
                {
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        StopHeadSnapAudio();

        EnableDisabledScripts();

        if (monsterRoamScript != null)
        {
            monsterRoamScript.enabled = true;
            monsterRoamScript.ForceStartRoam();
        }
    }

    private void EnableDisabledScripts()
    {
        if (scriptsToDisable == null)
        {
            return;
        }

        for (int i = 0; i < scriptsToDisable.Length; i++)
        {
            if (scriptsToDisable[i] != null)
            {
                scriptsToDisable[i].enabled = true;
            }
        }
    }

    private void StartHeadSnapAudio()
    {
        if (headSnapAudioSource == null || headSnapLoopClip == null)
        {
            return;
        }

        headSnapAudioSource.clip = headSnapLoopClip;
        headSnapAudioSource.loop = true;
        headSnapAudioSource.volume = headSnapLoopVolume;
        headSnapAudioSource.Play();
        Debug.Log("Started Head Snap Audio");
    }

    private void StopHeadSnapAudio()
    {
        if (headSnapAudioSource == null)
        {
            return;
        }

        if (headSnapAudioSource.isPlaying)
        {
            headSnapAudioSource.Stop();
        }
    }
}
