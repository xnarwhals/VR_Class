using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int puzzlesCompleted = 0;
    [SerializeField] private int totalPuzzles = 3;
    private bool phoneRinging = false;
    public PhoneState CurrentPhoneState { get; private set; } = PhoneState.Home;
    public event Action<PhoneState> PhoneStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnPhonePickedUp()
    {
        if (!phoneRinging)
            return;

        phoneRinging = false;
        AudioManager.Instance?.StopPhoneRing();
        AudioManager.Instance?.PlayPhonePickup();
        SetPhoneState(PhoneState.Answered);
    }

    private void TryStartPhoneRing()
    {
        if (phoneRinging)
            return;

        if (AreAllPuzzlesCompleted())
        {
            phoneRinging = true;
            AudioManager.Instance?.StartPhoneRing();
            SetPhoneState(PhoneState.Ringing);
        }
    }

    public void OnPuzzleCompleted()
    {
        puzzlesCompleted++;
        TryStartPhoneRing();
    }

    public bool AreAllPuzzlesCompleted()
    {
        return puzzlesCompleted >= totalPuzzles;
    }

    public void ResetGame()
    {
        puzzlesCompleted = 0;
        SetPhoneState(PhoneState.Home);
    }

    private void SetPhoneState(PhoneState state)
    {
        if (CurrentPhoneState == state)
            return;

        CurrentPhoneState = state;
        PhoneStateChanged?.Invoke(state);
    }

    public enum PhoneState
    {
        Home,
        Ringing,
        Answered
    }
}
