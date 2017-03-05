public static class Constants
{
    public static readonly float AnimationDuration =  0.2f;

    public static readonly float MoveAnimationMinDuration = 0.05f;

    public static readonly float ExplosionDuration = 0.3f;

    public static readonly float WaitBeforePotentialMatchesCheck = 2f;
    public static readonly float OpacityAnimationFrameDelay = 0.05f;

    public static readonly int MinimumMatches = 3;


    public static UnityEngine.Color ColorBase = new UnityEngine.Color(236 / 255f, 228 / 255f, 217 / 255f);
    public static UnityEngine.Color ColorSelected = new UnityEngine.Color(242 / 255f, 177 / 255f, 121 / 255f);
    public static UnityEngine.Color ColorMatched = new UnityEngine.Color(236 / 255f, 206 / 255f, 110 / 255f);

    public static readonly float StartingGameTimer = 60;
    public static float TimerMax = 100;
    public static float TimerLow = 10;
    public static float IdleTimerCount = 30;

    public static string DateFormat = "dd-MM-yyyy";

    public static string BaseURL = "";

    public static string TutorialPhase1Msg = "Swap tiles by clicking on one and then the other";
    public static string TutorialPhase2Msg = "Create matches by moving 3 identical numbers next to eachother";

}
[System.Flags]
public enum GameState
{
    Playing,
    SelectionStarted,
    Animating,
    Lost
}