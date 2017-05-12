using System.Collections;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControllerTutorial : MonoBehaviour,IControllerInterface
{
    public int Score { get; set; }
    public Text TutorialText;
    public Button SkipButton;
    public Image GameField;

    private UserStatistics _userStatistics;

    void Awake()
    {
        _userStatistics = new UserStatistics();
    }

    void Start ()
	{
	    SkipButton.image.ZKalphaTo(1,0.5f).start();
	    SkipButton.GetComponentInChildren<Text>().ZKalphaTo(1,0.5f).start();
	    TutorialText.ZKalphaTo(1,0.5f).start();
	    TutorialText.text = Utilities.LoadStringFromFile("TutorialHeader", 25);
    }

    public void StartGame()
    {
        ZestKit.instance.stopAllTweens();
        SceneManager.LoadScene("MainGame");
    }
    public bool IsPaused() { return false; }

    public bool IsControl()
    {
        return _userStatistics.GetToday().Control == 1;
    }
    //Empty methods to comply with interface
    public void IncreaseScore(int amount){}
    public void IncreaseGameTimer(float inc){}
    public void LoseGame(bool _){}
    public void MoveMade(){}
    public void LevelUp(int level){}
    public void QuitGame(){}
    void Update() { }
}
