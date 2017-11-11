﻿using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static UserInformation UserInformation;
    public static int SeriesDelta = 0;
    public static int Score = 0;
    public static float TotalTimePlayed = 0;
    public static bool ConnectionError { get; set; }
    public static int GameId;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SendLogs(LogMessage msg)
    {
        StartCoroutine(UserInformation.SendLogs(msg));
    }
}

