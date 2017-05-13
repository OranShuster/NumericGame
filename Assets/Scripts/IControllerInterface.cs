public interface IControllerInterface
{
    void IncreaseScore(int amount);
    void IncreaseGameTimer(float inc);
    void LoseGame(bool sessionTimeUp=false);
    void MoveMade();
    void LevelUp(int level);
    void QuitGame();
    bool IsPaused();
}
