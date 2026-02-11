using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class RespawnBow : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty respawnAction;

    [Header("Spawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform playerHead;
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

        Transform head = GetHeadTransform();
        Vector3 spawnPos;
        Quaternion spawnRot;

        if (respawnPoint != null)
        {
            spawnPos = respawnPoint.position;
            spawnRot = respawnPoint.rotation;
        }
        else
        {
            Vector3 forward = head != null ? head.forward : transform.forward;
            Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude < 0.001f) flatForward = transform.forward;
            flatForward.Normalize();
            spawnPos = (head != null ? head.position : transform.position) + flatForward * spawnDistance;
            spawnRot = Quaternion.LookRotation(flatForward, Vector3.up);
        }

        sceneBow.SetActive(true);
        sceneBow.transform.SetPositionAndRotation(spawnPos, spawnRot);

        if (sceneBow.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (smokePoof != null)
        {
            smokePoof.transform.SetPositionAndRotation(spawnPos, spawnRot);
            smokePoof.Play();
        }
    }

    private Transform GetHeadTransform()
    {
        if (playerHead != null) return playerHead;
        if (Camera.main != null) return Camera.main.transform;
        return null;
    }
}
