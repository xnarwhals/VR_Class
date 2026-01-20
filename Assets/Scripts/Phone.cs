using UnityEngine;
using UnityEngine.UI;

public class Phone : MonoBehaviour
{
    [SerializeField] private Image screen;
    [SerializeField] private Sprite homeScreen;
    [SerializeField] private Sprite ringingScreen;
    [SerializeField] private Sprite answeredScreen;
    private GameManager gameManager;
    private void Awake()
    {
        gameManager = GameManager.Instance;
    }

    private void OnEnable()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        if (gameManager != null)
            gameManager.PhoneStateChanged += UpdateScreen;

        UpdateScreen(gameManager != null ? gameManager.CurrentPhoneState : GameManager.PhoneState.Home);
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.PhoneStateChanged -= UpdateScreen;
    }

    private void UpdateScreen(GameManager.PhoneState state)
    {
        if (screen == null)
            return;

        switch (state)
        {
            case GameManager.PhoneState.Home:
                screen.sprite = homeScreen;
                break;
            case GameManager.PhoneState.Ringing:
                screen.sprite = ringingScreen;
                break;
            case GameManager.PhoneState.Answered:
                screen.sprite = answeredScreen;
                break;
        }
    }
}
