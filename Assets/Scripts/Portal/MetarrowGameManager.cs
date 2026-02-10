using UnityEngine;
using System;

public class MetarrowGameManager : MonoBehaviour
{
    public static MetarrowGameManager Instance { get; private set; }
    public event Action levelCompleted;

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

}
