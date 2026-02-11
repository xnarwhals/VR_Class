using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class RespawnBow : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty respawnAction;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnDistance = 0.6f;
    [SerializeField] private GameObject sceneBow;
    [SerializeField] private float cooldownSeconds = 5f;
    [SerializeField] private ParticleSystem smokePoof;

    private float _nextAllowedTime = 0f;

    private void OnEnable()
    {
        if (respawnAction.action != null)
        {
            respawnAction.action.performed += OnRespawnAction;
            respawnAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (respawnAction.action != null)
        {
            respawnAction.action.performed -= OnRespawnAction;
            respawnAction.action.Disable();
        }
    }

    private void OnRespawnAction(InputAction.CallbackContext ctx)
    {
        TryRespawn();
    }

    private void TryRespawn()
    {
        if (Time.time < _nextAllowedTime) return;
        _nextAllowedTime = Time.time + cooldownSeconds;

        RespawnNow();
    }

    private void RespawnNow()
    {
        if (sceneBow == null) return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
            spawnRot = spawnPoint.rotation;
        }
        else
        {
            spawnPos = transform.position + transform.forward * spawnDistance;
            spawnRot = transform.rotation;
            Debug.LogWarning("No spawn point set for RespawnBow, defaulting to in front of the player.");
        }

        sceneBow.transform.SetPositionAndRotation(spawnPos, spawnRot);

        if (smokePoof != null)
        {
            Debug.Log("Playing smoke poof effect at respawn location.");
            smokePoof.transform.SetPositionAndRotation(spawnPos, spawnRot);
            smokePoof.Play();
        }
    }
}
