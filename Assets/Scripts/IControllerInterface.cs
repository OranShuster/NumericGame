public interface IControllerInterface
{
    void IncreaseScore(int amount);
    void IncreaseGameTimer(float inc);
    void LoseGame(LoseReasons reason);
    void MoveMade();
    void LevelUp(int level);
    void QuitGame();
    bool IsPaused();
}
