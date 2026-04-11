using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerRespawn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerArrowHitTracker playerHitTracker;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AudioSource audioSource;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Camera fadeCamera;
    [SerializeField] private bool preferCameraSpaceFade = true;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float blackHoldDuration = 0.2f;
    [SerializeField] private float fadeInDuration = 0.35f;

    [Header("Movement Lock")]
    [SerializeField] private Behaviour[] movementBehavioursToDisable;

    [Header("Audio")]
    [SerializeField] private AudioClip respawnClip;
    [SerializeField] private float respawnClipVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private readonly Dictionary<Behaviour, bool> _movementStates = new Dictionary<Behaviour, bool>();
    private bool _isRespawning;
    private Canvas _fadeCanvas;

    private void Awake()
    {
        if (playerRoot == null)
        {
            playerRoot = transform;
        }

        if (playerHitTracker == null)
        {
            playerHitTracker = GetComponentInParent<PlayerArrowHitTracker>();
        }

        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>(true);
        }

        EnsureFadeCanvas();
        ConfigureFadeCanvasRenderMode();
        SetFadeImmediate(0f);
    }

    private void OnEnable()
    {
        ConfigureFadeCanvasRenderMode();

        if (playerHitTracker != null)
        {
            playerHitTracker.PlayerDied += HandlePlayerDied;
        }
        else if (debugLogs)
        {
            Debug.LogWarning("PlayerRespawn: no PlayerArrowHitTracker assigned or found.", this);
        }
    }

    private void OnDisable()
    {
        if (playerHitTracker != null)
        {
            playerHitTracker.PlayerDied -= HandlePlayerDied;
        }

        if (_isRespawning)
        {
            StopAllCoroutines();
            SetFadeImmediate(0f);
            UnlockMovement();
            _isRespawning = false;
        }
    }

    private void HandlePlayerDied()
    {
        if (_isRespawning)
        {
            return;
        }

        if (respawnClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnClip, Mathf.Max(0f, respawnClipVolume));
        }

        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        _isRespawning = true;

        LockMovement();
        yield return FadeTo(1f, Mathf.Max(0f, fadeOutDuration));
        yield return new WaitForSeconds(Mathf.Max(0f, blackHoldDuration));


        TeleportToRespawn();
        playerHitTracker?.ResetHits();

        yield return FadeTo(0f, Mathf.Max(0f, fadeInDuration));
        UnlockMovement();

        _isRespawning = false;
    }

    private void LockMovement()
    {
        _movementStates.Clear();
        if (movementBehavioursToDisable != null)
        {
            for (int i = 0; i < movementBehavioursToDisable.Length; i++)
            {
                Behaviour behaviour = movementBehavioursToDisable[i];
                if (behaviour == null)
                {
                    continue;
                }

                _movementStates[behaviour] = behaviour.enabled;
                behaviour.enabled = false;
            }
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }

    private void UnlockMovement()
    {
        foreach (KeyValuePair<Behaviour, bool> pair in _movementStates)
        {
            if (pair.Key == null)
            {
                continue;
            }

            pair.Key.enabled = pair.Value;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }

    }

    private void TeleportToRespawn()
    {
        if (respawnPoint == null || playerRoot == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning("PlayerRespawn: respawnPoint or playerRoot is not assigned.", this);
            }

            return;
        }

        playerRoot.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
        Physics.SyncTransforms();
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        float startAlpha = fadeCanvasGroup.alpha;
        if (duration <= 0.001f)
        {
            fadeCanvasGroup.alpha = targetAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    private void SetFadeImmediate(float alpha)
    {
        if (fadeCanvasGroup == null)
        {
            return;
        }

        fadeCanvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    private void EnsureFadeCanvas()
    {
        if (fadeCanvasGroup != null)
        {
            _fadeCanvas = fadeCanvasGroup.GetComponentInParent<Canvas>();
            return;
        }

        GameObject canvasObject = new GameObject("RespawnFadeCanvas");
        canvasObject.transform.SetParent(transform, false);

        _fadeCanvas = canvasObject.AddComponent<Canvas>();
        _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.sortingOrder = short.MaxValue;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageRect = imageObject.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.color = Color.black;

        fadeCanvasGroup = imageObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;

        if (debugLogs)
        {
            Debug.Log("PlayerRespawn: auto-created fade canvas.", this);
        }
    }

    private void ConfigureFadeCanvasRenderMode()
    {
        if (_fadeCanvas == null && fadeCanvasGroup != null)
        {
            _fadeCanvas = fadeCanvasGroup.GetComponentInParent<Canvas>();
        }

        if (_fadeCanvas == null)
        {
            return;
        }

        if (!preferCameraSpaceFade)
        {
            _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _fadeCanvas.worldCamera = null;
            return;
        }

        if (fadeCamera == null)
        {
            fadeCamera = ResolveFadeCamera();
        }

        if (fadeCamera != null)
        {
            _fadeCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            _fadeCanvas.worldCamera = fadeCamera;
            _fadeCanvas.planeDistance = 0.1f;
            return;
        }

        _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.worldCamera = null;

        if (debugLogs)
        {
            Debug.LogWarning("PlayerRespawn: no camera found for fade canvas, using overlay fallback.", this);
        }
    }

    private Camera ResolveFadeCamera()
    {
        if (playerRoot != null)
        {
            Camera cameraInPlayer = playerRoot.GetComponentInChildren<Camera>(true);
            if (cameraInPlayer != null)
            {
                return cameraInPlayer;
            }
        }

        if (Camera.main != null)
        {
            return Camera.main;
        }

        Camera anyCamera = FindObjectOfType<Camera>();
        return anyCamera;
    }
}
