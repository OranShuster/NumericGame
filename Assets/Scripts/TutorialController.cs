using System;
using System.Collections;
using System.Collections.Generic;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TutorialController : MonoBehaviour
{
    public Text TutorialText;
    public Text TutorialHeader;
    public Button SkipButton;
    public Image GameArea;
    public GameObject GameArea1Prefab;
    public GameObject GameArea2Prefab;


    private int _tutorialPhase = 1;
    private Vector2 _candySize;
    private readonly int _maxNumber=3;
    private GameState State = GameState.Playing;
    private GameObject _hitGo = null;


    // Use this for initialization
    void Start ()
	{
	    SkipButton.image.ZKalphaTo(1,0.5f).start();
	    SkipButton.GetComponentInChildren<Text>().ZKalphaTo(1,0.5f).start();
	    TutorialHeader.ZKalphaTo(1,0.5f).start();
    }

    // Update is called once per frame
    void Update () {
        switch (_tutorialPhase)
        {
            //show swap board
            case 1:
                StartCoroutine(ShowBoard(1));
                _tutorialPhase = 2;
                break;
            //wait for a swap
            case 2:
                if (HandleMouseClicks())
                    _tutorialPhase = 3;
                break;
            //show match board
            case 3:
                HideBoard(1);
                //ShowBoard(2);
                _tutorialPhase = 4;
                break;
            //wait for match
            case 4:
                //_tutorialPhase = 5;
                break;
            //hide board and start game
            case 5:
                StartGame();
                break;
        }
	}

    private IEnumerator ShowBoard(int index)
    {
        State = GameState.Animating;
        var boardPrefab = Resources.Load(string.Format("TutorialBoard_{0}", index)) as GameObject;
        var boardGo = Instantiate(boardPrefab,new Vector3(200,-200),Quaternion.identity);
        boardGo.layer = 5;
        boardGo.transform.SetParent(GameArea.transform,false);
        var boardRectTransform = boardGo.GetComponent<RectTransform>() as RectTransform;
        boardRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
        boardRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 400);
        var boardGoImage = boardGo.GetComponent<Image>();
        boardGoImage.color = new Color(boardGoImage.color.r, boardGoImage.color.g, boardGoImage.color.b,0);
        boardGoImage.ZKalphaTo(1).start();
        yield return new WaitForSeconds(0.3f);
        State = GameState.Playing;
    }
    void HideBoard(int index)
    {
        var board_go = GameObject.Find(String.Format("TutorialBoard_{0}(Clone)",index));
        board_go.GetComponent<Image>().ZKalphaTo(0).start();
        Destroy(board_go);
       
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }
    private bool HandleMouseClicks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var cursor = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
            var objectsHit = new List<RaycastResult>();
            EventSystem.current.RaycastAll(cursor, objectsHit);
            var hit = objectsHit.Find(x => x.gameObject.name == "NumberTile").gameObject;
            if (hit!=null)
            {
                switch (State)
                {
                    case GameState.Playing:
                        _hitGo = hit;
                        _hitGo.GetComponent<Image>().color = Constants.ColorSelected;
                        State = GameState.SelectionStarted;
                        return false;
                    case GameState.SelectionStarted:
                        if (hit != _hitGo)
                        {
                            //if the two shapes are diagonally aligned (different row and column), just return
                            if (!Utilities.AreNeighbors(_hitGo.GetComponent<Shape>(), hit.GetComponent<Shape>()))
                            {
                                _hitGo.GetComponent<Image>().color = Constants.ColorBase;
                                this.State = GameState.Playing;
                                return false;
                            }
                            else
                            {
                                State = GameState.Animating;
                                ShapesManager.FixSortingLayer(_hitGo, hit);
                                StartCoroutine(SwapTiles(_hitGo, hit.gameObject)); 
                                return true;
                            }
                        }
                        return false;
                }
            }
        }
        return false;    
    }

    private IEnumerator SwapTiles(GameObject go1,GameObject go2)
    {
        go2.GetComponent<Image>().color = Constants.ColorSelected;

        //move the swapped ones
        go1.transform.ZKlocalPositionTo(go2.transform.localPosition, Constants.AnimationDuration).start();
        go2.transform.ZKlocalPositionTo(go1.transform.localPosition, Constants.AnimationDuration).start();
        yield return new WaitForSeconds(Constants.AnimationDuration);

        //remove selection color from squares
        go2.GetComponent<Image>().color = Constants.ColorBase;
        go1.GetComponent<Image>().color = Constants.ColorBase;

        State = GameState.Playing;
    }
}
