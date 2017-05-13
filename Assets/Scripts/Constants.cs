﻿using JetBrains.Annotations;
using UnityEngine;

public static class Constants
{
    public static readonly float AnimationDuration =  0.2f;

    public static readonly float MoveAnimationMinDuration = 0.05f;

    public static readonly float ExplosionDuration = 0.3f;

    public static readonly float WaitBeforePotentialMatchesCheck = 2f;
    public static readonly float OpacityAnimationFrameDelay = 0.05f;

    public static readonly int MinimumMatches = 3;


    public static Color32 ColorBase = new Color32(66, 133, 244, 255);
    public static Color32 ColorSelected = new Color32(100, 181, 246, 255);
    public static Color32 ColorMatched = new Color32(21, 101, 192, 255);
    public static Color32 ColorDangerRed = new Color32(204, 0, 0, 255);
    public static Color32 ColorWarningOrange = new Color32(255, 136, 0, 255);
    public static Color32 ColorOkGreen = new Color32(0, 126, 51,255);
    public static Color32[] ControlBaseColors = new[]
    {
        new Color32(244,67,54,255),
        new Color32(255,152,0,255),
        new Color32(76,175,80,255),
        new Color32(0,153,204,255),
        new Color32(96,125,139,255),
        new Color32(205,220,57,255), 
        new Color32(156,39,176,255)

    };
    public static Color32[] ControlSelectedColors = new[]
    {
        new Color32(229,115,115,255),
        new Color32(255,183,77,255),
        new Color32(129,199,132,255),
        new Color32(56,190,235,255),
        new Color32(144,164,174,255),
        new Color32(212,225,87,255),
        new Color32(186,104,200,255)
    };
    public static Color32[] ControlMatchedColors = new[]
    {
        new Color32(229,57,53,255),
        new Color32(251,140,0,255),
        new Color32(67,160,71,255),
        new Color32(0,151,167,255),
        new Color32(84,110,122,255),
        new Color32(192,202,51,255),
        new Color32(142,36,170,255)
    };


    public static readonly float StartingGameTimer = 60;
    public static float TimerMax = 90;
    public static float TimerLow = 10;
    public static int ScoreReportingInterval = 10;
    public static float IdleTimerSeconds = 30;
    public static float IdleTimerLow = 10;

    public static string DateFormat = "yyyy-MM-dd";
    public static string DateFormatOutput = "dd/MM/yyyy";
    public static string BaseUrl = "https://cnl.bgu.ac.il/numeric_game/playrpc/";

    public const string TestCode = "desiree";
    public const string TestCode2 = "desiree2";


}
[System.Flags]
public enum GameState
{
    Playing,
    SelectionStarted,
    Animating,
    Lost
}

public enum LoseReasons
{
    SessionTime,
    Idle,
    GameTime,
    Points
}