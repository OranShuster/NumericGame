using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Prime31.ZestKit;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    private Vector2[] _spawnPositions;

    private int Level
    {
        get { return GameManager.SeriesDelta; }
        set { GameManager.SeriesDelta=value; }
    }

    private Vector2 _cellSize;
    private GameObject _hitGo;
    private Sprite[] _numberSquareSprites;
    private GameState _state;
    private int _maxNumber;
    private int _rows;
    private int _columns;
    private IControllerInterface _controllerScript;
    private ShapesMatrix _shapes;

    public GameObject NumberSquarePrefab;
    public GameObject Manager;
    public Image GameField;

    private int[] NextLevelScore
    {
        get {return Constants.LevelUpScores;}
    } 

    private SoundManager _soundManager;
    private string _tileImagesFolder;

    void Awake()
    {
        _state = GameState.PreGame;
        _controllerScript = Manager.GetComponent<IControllerInterface>();
        _soundManager = GetComponent<SoundManager>();
    }

    void Start()
    {
        _tileImagesFolder = GameManager.UserInformation.IsControlSession() ? "Images/Control" : "Images/Numbers";
        _numberSquareSprites = Resources.LoadAll<Sprite>(_tileImagesFolder).OrderBy(t => Convert.ToInt32(t.name)).ToArray();
        _maxNumber = _numberSquareSprites.Length;
        _rows = _maxNumber;
        _columns = _maxNumber;
        var spacingSize = 5 * (_maxNumber + 1);
        var playWidth = (int)GameField.rectTransform.rect.width - spacingSize;
        var playHeight = (int)GameField.rectTransform.rect.height - spacingSize;
        _cellSize = new Vector2(playWidth / (float)_maxNumber, playHeight / (float)_maxNumber);
        StartCoroutine(InitializeCellAndSpawnPositions());
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButtonDown(0) || _controllerScript.IsPaused()) return;
        var cursor = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var objectsHit = new List<RaycastResult>();
        EventSystem.current.RaycastAll(cursor, objectsHit);
        var hit = objectsHit.Find(x => x.gameObject.name == "NumberTile(Clone)").gameObject;
        if (hit == null) return;
        switch (_state)
        {
            case GameState.Playing:
                _hitGo = hit;
                SetTileColorSelected(_hitGo);
                _state = GameState.SelectionStarted;
                break;
            case GameState.SelectionStarted:
                if (hit != _hitGo)
                {
                    if (!Utilities.AreNeighbors(_hitGo.GetComponent<NumberCell>(), hit.GetComponent<NumberCell>()))
                    {
                        SetTileColorBase(_hitGo);
                        SetTileColorSelected(hit);
                        _hitGo = hit;
                    }
                    else
                    {
                        _state = GameState.Animating;
                        FixSortingLayer(_hitGo, hit);
                        StartCoroutine(FindMatchesAndCollapse(hit));
                    }
                }
                else
                {
                    _state = GameState.Playing;
                    SetTileColorBase(hit);
                }
                break;
        }
    }

    public GameState GetState()
    {
        return _state;
    }
    private static void SetTileColorBase(GameObject go)
    {
        var numberImage = go.transform.Find("NumberImage").gameObject;
        numberImage.GetComponent<Image>().color = Color.white;
        if (!GameManager.UserInformation.IsControlSession()) return;
        numberImage.GetComponent<Outline>().effectDistance = new Vector2(0, 0);
        numberImage.GetComponent<Outline>().effectColor =
            new Color(1,1,1,0);
        go.GetComponent<Outline>().effectDistance = new Vector2(0, 0);
        go.GetComponent<Outline>().effectColor =
            new Color(1, 1, 1, 0);
    }
    private static void SetTileColorSelected(GameObject go)
    {
        var numberImage = go.transform.Find("NumberImage").gameObject;
        numberImage.GetComponent<Image>().color = GameManager.UserInformation.IsControlSession()
            ? Constants.ControlSelectedColors[go.GetComponent<NumberCell>().Value - 1]
            : Constants.ColorSelected;
        if (!GameManager.UserInformation.IsControlSession()) return;
        numberImage.GetComponent<Outline>().effectDistance = new Vector2(5f, 5f);
        numberImage.GetComponent<Outline>().effectColor =
            Constants.ControlSelectedColors[go.GetComponent<NumberCell>().Value - 1];
        go.GetComponent<Outline>().effectDistance = new Vector2(5f, 5f);
        go.GetComponent<Outline>().effectColor =
            Constants.ControlSelectedColors[go.GetComponent<NumberCell>().Value - 1];
    }
    private static void SetTileColorMatched(GameObject go)
    {
        var numberImage = go.transform.Find("NumberImage").gameObject;
        numberImage.GetComponent<Image>().color = GameManager.UserInformation.IsControlSession()
            ? Constants.ControlMatchedColors[go.GetComponent<NumberCell>().Value - 1]
            : Constants.ColorMatched;
        if (!GameManager.UserInformation.IsControlSession()) return;
        numberImage.GetComponent<Outline>().effectDistance = new Vector2(5f, 5f);
        numberImage.GetComponent<Outline>().effectColor =
            Constants.ControlMatchedColors[go.GetComponent<NumberCell>().Value - 1];
        go.GetComponent<Outline>().effectDistance = new Vector2(5f, 5f);
        go.GetComponent<Outline>().effectColor =
            Constants.ControlMatchedColors[go.GetComponent<NumberCell>().Value - 1];
    }

    public IEnumerator InitializeCellAndSpawnPositions()
    {
        _state = GameState.PreGame;
        _shapes = new ShapesMatrix(_rows, _columns, _maxNumber);
        _spawnPositions = new Vector2[_columns];
        for (var row = 0; row < _rows; row++)
        {
            for (var column = 0; column < _columns; column++)
            {
                InstantiateAndPlaceNewCell(row, column, NumberSquarePrefab);
            }
        }
        SetupSpawnPositions();
        yield return StartCoroutine(ClearBoardMatches());
        _state = GameState.Playing;
        yield return null;
    }


    public IEnumerator ClearBoardMatches()
    {
        var totalMatches = _shapes.GetMatches(_maxNumber, Level, GameManager.UserInformation.IsControlSession(), false);
        var sameMatches = _shapes.GetMatches(_maxNumber, 0, GameManager.UserInformation.IsControlSession(), false);
        totalMatches.CombineMatchesInfo(sameMatches,false);
        yield return StartCoroutine(HandleMatches(totalMatches, false, false, true));
        GameField.gameObject.GetComponent<CanvasGroup>().interactable = true;
        GameField.gameObject.GetComponent<CanvasGroup>().alpha = 1;
    }

    private void SetupSpawnPositions()
    {
        //create the spawn positions for the new shapes (will pop from the 'ceiling')
        for (var column = 0; column < _columns; column++)
        {
            var location = Calculate_cell_location(0, column);
            _spawnPositions[column] = new Vector2(location[0], +_cellSize.y / 2);
        }
    }

    public IEnumerator HandleMatches(MatchesInfo totalMatches, bool withScore = true, bool withEffects = true,
        bool quickMode = false)
    {
        while (totalMatches.MatchedCells.Count() >= Constants.MinimumMatches)
        {
            if (withScore)
            {
                _controllerScript.IncreaseScore(totalMatches.AddedScore);
                _controllerScript.IncreaseGameTimer(5 * totalMatches.NumberOfMatches);
            }
            foreach (var item in totalMatches.MatchedCells.Distinct())
            {
                SetTileColorMatched(item);
            }
            if (!quickMode)
            {
                //Debug.Log(string.Format("DEBUG|201706021724|{0}", totalMatches.PrintMatches()));
                _soundManager.PlayCrincle();
                yield return new WaitForSeconds(0.5f);
            }

            foreach (var item in totalMatches.MatchedCells.Distinct())
            {
                _shapes.Remove(item);
                RemoveFromScene(item);
            }

            //get the columns that we had a collapse
            var columns = totalMatches.MatchedCells.Select(go => go.GetComponent<NumberCell>().Column).Distinct();

            //the order the 2 methods below get called is important!!!
            //collapse the ones gone
            var collapsedCellsInfo = _shapes.Collapse(columns);
            //create new ones
            var newCellsInfo = CreateNewCellsInSpecificColumns(columns);

            var maxDistance = Mathf.Max(collapsedCellsInfo.MaxDistance, newCellsInfo.MaxDistance);

            MoveAndAnimate(newCellsInfo.AlteredCell, maxDistance, quickMode);
            MoveAndAnimate(collapsedCellsInfo.AlteredCell, maxDistance, quickMode);

            //will wait for both of the above animations
            yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

            if (GameManager.Score >= NextLevelScore[Level])
                break;

            //Check for new matches with new tiles
            totalMatches = _shapes.GetMatches(_maxNumber, Level, GameManager.UserInformation.IsControlSession(), withScore);

            //Search identical matches 
            if (Level == 0) continue;
            var sameMatches = _shapes.GetMatches(_maxNumber, 0, GameManager.UserInformation.IsControlSession(), false);
            totalMatches.CombineMatchesInfo(sameMatches, GameManager.UserInformation.IsControlSession());
        }
        if (GameManager.Score >= NextLevelScore[Level])
        {
            LevelUp();
            _controllerScript.LevelUp();
        }
        _state = GameState.Playing;
    }

    public IEnumerator FindMatchesAndCollapse(GameObject hit2)
    {
        _state = GameState.Animating;
        //get the second item that was part of the swipe
        var hitGo2 = hit2;
        SetTileColorSelected(hitGo2);
        _shapes.Swap(_hitGo, hitGo2);

        //move the swapped ones
        _hitGo.transform.ZKlocalPositionTo(hitGo2.transform.localPosition, Constants.AnimationDuration).start();
        hitGo2.transform.ZKlocalPositionTo(_hitGo.transform.localPosition, Constants.AnimationDuration).start();
        yield return new WaitForSeconds(Constants.AnimationDuration * 1.1f);

        //remove selection color from squares
        SetTileColorBase(hitGo2);
        SetTileColorBase(_hitGo);

        //Find matches
        var totalMatches = _shapes.GetMatches(_maxNumber, Level, GameManager.UserInformation.IsControlSession(), true);
        //Find identical strings with no score
        if (Level != 0)
        {
            var sameMatches = _shapes.GetMatches(_maxNumber, 0, false,false);
            totalMatches.CombineMatchesInfo(sameMatches, GameManager.UserInformation.IsControlSession());
        }
        if (totalMatches.NumberOfMatches>0)
            yield return StartCoroutine(HandleMatches(totalMatches));
        else
            _controllerScript.IncreaseScore(-5);
        if (GameManager.Score >= 0)
        {
            _state = GameState.Playing;
            yield break;
        }
        _state = GameState.Lost;
        _controllerScript.LoseGame(LoseReasons.Points);
    }

    private void InstantiateAndPlaceNewCell(int row, int column, GameObject cellPrefab)
    {
        var location = Calculate_cell_location(row, column);
        var go = Instantiate(cellPrefab, new Vector2(location[0], -location[1]), Quaternion.identity);
        go.transform.SetParent(GameField.transform, false);
        go.layer = 5; //5=UI Layer
        var goTransform = go.transform as RectTransform;
        goTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _cellSize.x);
        goTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _cellSize.y);

        var numberValue = _shapes.GenerateNumber(_maxNumber);
        //assign the specific properties
        go.GetComponent<NumberCell>().Assign(numberValue, row, column);
        var numberImage = go.transform.Find("NumberImage").gameObject.GetComponent<Image>();
        numberImage.overrideSprite = _numberSquareSprites[numberValue - 1];
        SetTileColorBase(go);
        _shapes.Add(go);
    }

    private float[] Calculate_cell_location(int row, int col)
    {
        var spacingX = 5 * (col + 1);
        var spacingY = 5 * (row + 1);
        var locationX = col * _cellSize.x + _cellSize.x / 2;
        var locationY = row * _cellSize.y + _cellSize.y / 2;
        return new[] { locationX + spacingX, locationY + spacingY };
    }

    private void DestroyAllCells()
    {
        for (var row = 0; row < _rows; row++)
        {
            for (var column = 0; column < _columns; column++)
            {
                
                Destroy(_shapes[row, column]);
            }
        }
    }

    public void LevelUp()
    {
        _state = GameState.PreGame;
        Debug.Log(string.Format("INFO|201711121030|Level {0} ended",Level));
        Level += 1;
        GameField.gameObject.GetComponent<CanvasGroup>().interactable = false;
        GameField.gameObject.GetComponent<CanvasGroup>().alpha = 0;
		ZestKit.instance.stopAllTweens();
        DestroyAllCells();
    }

    private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects, int distance, bool quickmode = false)
    {
        var duration = Constants.MoveAnimationMinDuration;
        if (quickmode)
            duration = 0.000001f;

        foreach (var item in movedGameObjects)
        {
            var location = Calculate_cell_location(item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column);
            var itemRectTransform = item.GetComponent<RectTransform>();
            itemRectTransform.ZKanchoredPositionTo(new Vector2(location[0], -location[1]), duration * distance).start();
        }
    }
    public static void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        var sp1 = hitGo.GetComponent<CanvasRenderer>();
        var sp2 = hitGo2.GetComponent<CanvasRenderer>();
        if (sp1.transform.GetSiblingIndex() > sp2.transform.GetSiblingIndex()) return;
        sp1.transform.SetSiblingIndex(1);
        sp2.transform.SetSiblingIndex(0);
    }

    private static void RemoveFromScene(GameObject item)
    {
        SetTileColorMatched(item);
        Destroy(item);
    }

    private AlteredCellInfo CreateNewCellsInSpecificColumns(IEnumerable<int> columnsWithMissingCells)
    {
        var newCellsInfo = new AlteredCellInfo();

        //find how many null values the column has
        foreach (var column in columnsWithMissingCells)
        {
            var emptyItems = _shapes.GetEmptyItemsOnColumn(column,_rows);
            foreach (var item in emptyItems)
            {
                var newCell = Instantiate(NumberSquarePrefab, _spawnPositions[column], Quaternion.identity) as GameObject;
                var numberValue = _shapes.GenerateNumber(_maxNumber);
                newCell.layer = 5; //5=UI Layer
                newCell.transform.SetParent(GameField.transform, false);
                newCell.transform.SetAsFirstSibling();
                newCell.GetComponent<NumberCell>().Assign(numberValue, item.Row, item.Column);
                var numberImage = newCell.transform.Find("NumberImage").gameObject.GetComponent<Image>();
                numberImage.overrideSprite = _numberSquareSprites[numberValue - 1];
                SetTileColorBase(newCell);
                var newCellRectTransform = newCell.GetComponent<RectTransform>() as RectTransform;
                newCellRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _cellSize.x);
                newCellRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _cellSize.y);
                //assign the specific properties
                if (_maxNumber - item.Row > newCellsInfo.MaxDistance)
                    newCellsInfo.MaxDistance = _maxNumber - item.Row;
                _shapes.Add(newCell);
                newCellsInfo.AddCell(newCell);
            }
        }
        return newCellsInfo;
    }
    public void StopGame()
    {
        _state = GameState.Lost;
    }
    public void ToggleBoard()
    {
        GameField.gameObject.GetComponent<CanvasGroup>().interactable = !GameField.gameObject.GetComponent<CanvasGroup>().interactable;
        GameField.gameObject.GetComponent<CanvasGroup>().alpha = Math.Abs(GameField.gameObject.GetComponent<CanvasGroup>().alpha-1);
    }

    public void SetNextLevelScore(int score, int level)
    {
        NextLevelScore[level] = score;
        if (GameManager.Score < NextLevelScore[Level]) return;
        LevelUp();
        _controllerScript.LevelUp();
    }
}
