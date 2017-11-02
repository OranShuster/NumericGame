using UnityEngine;
public class GameMaster : MonoBehaviour
{
    public static GameMaster GM;
    public static UserInformation UserInformation;
    public static int SeriesDelta = 0;
    public static int Score = 0;
    public static float TotalTimePlayed = 0;
    public static bool ConnectionError { get; set; }
    public static int GameId;
    
    void Awake()
    {
        if(GM != null)
            GameObject.Destroy(GM);
        else
            GM = this;
         
        DontDestroyOnLoad(this);
    }

    public void SendLogs(LogMessage msg)
    {
        StartCoroutine(UserInformation.SendLogs(msg));
    }
}