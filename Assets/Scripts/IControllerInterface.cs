public interface IControllerInterface
{
    void IncreaseScore(int amount);
    void IncreaseGameTimer(float inc);
    void LoseGame(LoseReasons reason);
    void LevelUp();
    bool IsPaused();
}
