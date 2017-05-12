using UnityEngine;

public static class Constants
{
    public static readonly float AnimationDuration =  0.2f;

    public static readonly float MoveAnimationMinDuration = 0.05f;

    public static readonly float ExplosionDuration = 0.3f;

    public static readonly float WaitBeforePotentialMatchesCheck = 2f;
    public static readonly float OpacityAnimationFrameDelay = 0.05f;

    public static readonly int MinimumMatches = 3;


    public static Color ColorBase = new Color(66 / 255f, 133 / 255f, 244 / 255f);
    public static Color ColorSelected = new Color(100 / 255f, 181 / 255f, 246 / 255f);
    public static Color ColorMatched = new Color(21 / 255f, 101 / 255f, 192 / 255f);
    public static Color ColorDangerRed = new Color(204 / 255f, 0, 0);
    public static Color ColorWarningOrange = new Color(255 / 255f, 136/255f, 0);
    public static Color ColorOKGreen = new Color(0, 126 / 255f, 51 / 255f);



    public static readonly float StartingGameTimer = 60;
    public static float TimerMax = 100;
    public static float TimerLow = 10;
    public static int ScoreReportingInterval = 10;

    public static string DateFormat = "yyyy-MM-dd";
    public static string DateFormatOutput = "dd/MM/yyyy";
    public static string BaseUrl = "http://192.168.1.203:8888/game/";
    public static string GitHubIssueBaseUrl = "https://api.github.com/repos/OranShuster/NumericGameMirror/issues";

    public const string TestCode = "desiree";
    

}
[System.Flags]
public enum GameState
{
    Playing,
    SelectionStarted,
    Animating,
    Lost
}