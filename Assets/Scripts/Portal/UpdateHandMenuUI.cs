using UnityEngine;
using TMPro;

public class UpdateHandMenuUI : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text targetsLeftText;
    private MetarrowGameManager _manager;

    private void OnEnable()
    {
        TryBindManager();
    }

    private void Start()
    {
        TryBindManager();
    }

    private void OnDisable()
    {
        UnbindManager();
    }

    public void UpdateLevelText()
    {
        if (levelText == null)
        {
            Debug.LogError("Level Text UI element is not assigned.", this);
            return;
        }

        levelText.text = $"Level: {_manager.ActiveLevelName}";
    }

    public void UpdateTargetsLeftText(int targetsLeft)
    {
        if (targetsLeftText == null)
        {
            Debug.LogError("Targets Left Text UI element is not assigned.", this);
            return;
        }

        targetsLeftText.text = $"Targets Left: {targetsLeft}";
    }

    private void TryBindManager()
    {
        if (_manager != null)
        {
            return;
        }

        _manager = MetarrowGameManager.Instance;
        if (_manager == null)
        {
            Debug.LogError("MetarrowGameManager instance not found in the scene.", this);
            return;
        }

        _manager.targetsLeftChanged += UpdateTargetsLeftText;
        UpdateLevelText();
        UpdateTargetsLeftText(_manager.TargetsLeft);
    }

    private void UnbindManager()
    {
        if (_manager == null)
        {
            return;
        }

        _manager.targetsLeftChanged -= UpdateTargetsLeftText;
        _manager = null;
    }
}
