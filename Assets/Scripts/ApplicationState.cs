namespace NumericalGame
{
    public class ApplicationState
    {
        public static UserStatistics UserStatistics;
        public static int SeriesDelta = 0;
        public static int Score = 0;
        public static float TotalTimePlayed = 0;
        public static bool ConnectionError { get; set; }
        public static int GameId;
    }
}