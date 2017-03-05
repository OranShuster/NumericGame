public interface GameControllerInterface
{
    int Score { get; set; }

    void IncreaseScore(int amount);
    void IncreaseGameTimer(float inc);
    void LoseGame(bool SessionTimeUp=false);
    void MoveMade();
    void LevelUp(int level);
    void QuitGame();
    bool IsPaused();
}
