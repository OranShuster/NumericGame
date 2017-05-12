using JetBrains.Annotations;
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
    public static Color32 ColorOKGreen = new Color32(0, 126, 51,255);
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
        new Color32(239,83,80,255),
        new Color32(255,167,38,255),
        new Color32(102,187,106,255),
        new Color32(51,181,229,255),
        new Color32(120,144,156,255),
        new Color32(212,225,87,255),
        new Color32(171,71,188,255)
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
    public static float TimerMax = 100;
    public static float TimerLow = 10;
    public static int ScoreReportingInterval = 10;

    public static string DateFormat = "yyyy-MM-dd";
    public static string DateFormatOutput = "dd/MM/yyyy";
    public static string BaseUrl = "https://cnl.bgu.ac.il/numeric_game/playrpc/";
    public static string GitHubIssueBaseUrl = "https://api.github.com/repos/OranShuster/NumericGameMirror/issues";

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