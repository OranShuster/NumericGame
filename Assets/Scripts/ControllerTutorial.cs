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

    private UserStatistics _userStatistics=ApplicationState.UserStatistics;

    void Start ()
	{
	    SkipButton.image.ZKalphaTo(1,0.5f).start();
	    SkipButton.GetComponentInChildren<Text>().ZKalphaTo(1,0.5f).start();
	    TutorialText.ZKalphaTo(1,0.5f).start();
	    var tutorialHeaderStringName = "TutorialHeaderControl";
	    if (!_userStatistics.IsControl())
	    {
	        tutorialHeaderStringName = string.Format("TutorialHeader{0}", ApplicationState.SeriesDelta);
	    }
        TutorialText.text = Utilities.LoadStringFromFile(tutorialHeaderStringName, 35);
    }

    public void StartGame()
    {
        ZestKit.instance.stopAllTweens();
        SceneManager.LoadScene("MainGame");
    }
    public bool IsPaused() { return false; }

    //Empty methods to comply with interface
    public void IncreaseScore(int amount){}
    public void IncreaseGameTimer(float inc){}
    public void LoseGame(LoseReasons _){}
    public void MoveMade(){}
    public void LevelUp(int level){}
    public void QuitGame(){}
    void Update() { }
}
