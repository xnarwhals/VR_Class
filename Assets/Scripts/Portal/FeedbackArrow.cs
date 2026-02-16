using UnityEngine;

public class FeedbackArrow : MonoBehaviour
{
    private Renderer[] _renderers;
    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        if (_renderers == null || _renderers.Length == 0)
        {
            Debug.LogError("FeedbackArrow requires a Renderer component.");
        }
    }

    private void Start()
    {
        SetArrowActive(false);
    }

    public void SetArrowActive(bool active)
    {
        if (_renderers == null)
        {
            return;
        }

        foreach (var renderer in _renderers)
        {
            renderer.enabled = active;
        }
    }
}
