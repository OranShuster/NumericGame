using System.Collections;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour,GameControllerInterface
{
    public int Score { get; set; }
    public Text TutorialText;
    public Text TutorialHeader;
    public Button SkipButton;
    public Image GameField;

    private Game _tutorialGame;

    void Awake()
    {
        _tutorialGame = GameField.GetComponent<Game>();
    }

    // Use this for initialization
    void Start ()
	{
	    SkipButton.image.ZKalphaTo(1,0.5f).start();
	    SkipButton.GetComponentInChildren<Text>().ZKalphaTo(1,0.5f).start();
	    TutorialHeader.ZKalphaTo(1,0.5f).start();
	    TutorialText.ZKalphaTo(1,0.5f).start();
    }

    // Update is called once per frame
    void Update () {}

    IEnumerator ChangeTutorialMessage(string msg)
    {
        TutorialText.ZKalphaTo(0).start();
        yield return new WaitForSeconds(0.5f);
        TutorialText.text = msg;
        TutorialText.ZKalphaTo(1).start();
        yield return new WaitForSeconds(0.5f);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }
    public bool IsPaused() { return false; }

    //Empty methods to comply with interface
    public void IncreaseScore(int amount){}
    public void IncreaseGameTimer(float inc){}
    public void LoseGame(bool _){}
    public void MoveMade(){}
    public void LevelUp(int level){}
    public void QuitGame(){}
}
