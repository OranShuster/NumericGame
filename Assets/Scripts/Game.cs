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

    private int SeriesDelta
    {
        get { return ApplicationState.SeriesDelta; }
        set { ApplicationState.SeriesDelta=value; }
    }

    private Vector2 _cellSize;
    private GameObject _hitGo = null;
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
    public int[] NextLevelScore;
    public Text DebugText;
    public bool DebugMode;

    private SoundManager _soundManager;
    private string _tileImagesFolder;

    void Awake()
    {
        _state = GameState.Animating;
        _controllerScript = Manager.GetComponent<IControllerInterface>();
        _soundManager = GetComponent<SoundManager>();
    }

    void Start()
    {
        _tileImagesFolder = ApplicationState.UserStatistics.IsControl() ? "Images/Control" : "Images/Numbers";
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
        if (DebugMode && DebugText!=null)
            DebugText.text = DebugUtilities.GetArrayContents(_shapes,_maxNumber);
        if (Input.GetMouseButtonDown(0) && !_controllerScript.IsPaused())
        {
            var cursor = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var objectsHit = new List<RaycastResult>();
            EventSystem.current.RaycastAll(cursor, objectsHit);
            var hit = objectsHit.Find(x => x.gameObject.name == "NumberTile(Clone)").gameObject;
            if (hit != null)
            {
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
                                _controllerScript.MoveMade();
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
        }
    }

    public GameState GetState()
    {
        return _state;
    }
    private void SetTileColorBase(GameObject go)
    {
        go.GetComponent<Image>().color = ApplicationState.UserStatistics.IsControl()
            ? Constants.ControlBaseColors[go.GetComponent<NumberCell>().Value - 1]
            : Constants.ColorBase;
    }
    private void SetTileColorSelected(GameObject go)
    {
        go.GetComponent<Image>().color = ApplicationState.UserStatistics.IsControl()
            ? Constants.ControlSelectedColors[go.GetComponent<NumberCell>().Value - 1]
            : Constants.ColorSelected;
    }
    private void SetTileColorMatched(GameObject go)
    {
        go.GetComponent<Image>().color = ApplicationState.UserStatistics.IsControl()
            ? Constants.ControlMatchedColors[go.GetComponent<NumberCell>().Value - 1]
            : Constants.ColorMatched;
    }

    public IEnumerator InitializeCellAndSpawnPositions()
    {
        _state = GameState.Animating;
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
        var totalMatches = _shapes.GetMatches(_maxNumber, SeriesDelta, ApplicationState.UserStatistics.IsControl(), false);
        var sameMatches = _shapes.GetMatches(_maxNumber, 0, ApplicationState.UserStatistics.IsControl(), false);
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
            var location = calculate_cell_location(0, column);
            _spawnPositions[column] = new Vector2(location[0], +_cellSize.y / 2);
        }
    }

    public IEnumerator HandleMatches(MatchesInfo totalMatches, bool withScore = true, bool withEffects = true,
        bool quickMode = false)
    {
        var first_run = 1;
        while (totalMatches.MatchedCells.Count() >= Constants.MinimumMatches)
        {
            if (withScore)
            {
                _controllerScript.IncreaseScore(totalMatches.AddedScore + (first_run * -5));
                first_run = 0;
                _controllerScript.IncreaseGameTimer(5 * totalMatches.NumberOfMatches);
            }
            foreach (var item in totalMatches.MatchedCells.Distinct())
            {
                SetTileColorMatched(item);
            }
            if (!quickMode)
            {
                Debug.logger.Log("Match_Score{02061724}", totalMatches.PrintMatches());
                _soundManager.PlayCrincle();
                yield return new WaitForSeconds(0.75f);
            }

            foreach (var item in totalMatches.MatchedCells.Distinct())
            {
                //SoundManager.PlayCrincle();
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

            if (ApplicationState.Score >= NextLevelScore[SeriesDelta])
                break;

            //Check for new matches with new tiles
            totalMatches = _shapes.GetMatches(_maxNumber, SeriesDelta, ApplicationState.UserStatistics.IsControl(), withScore);

            //Search identical matches 
            if (SeriesDelta != 0)
            {
                var sameMatches = _shapes.GetMatches(_maxNumber, 0, ApplicationState.UserStatistics.IsControl(), false);
                totalMatches.CombineMatchesInfo(sameMatches, ApplicationState.UserStatistics.IsControl());
            }

        }
        if (ApplicationState.Score >= NextLevelScore[SeriesDelta])
        {
            LevelUp();
            _controllerScript.LevelUp(SeriesDelta);
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
        var totalMatches = _shapes.GetMatches(_maxNumber, SeriesDelta, ApplicationState.UserStatistics.IsControl(), true);
        //Find identical strings with no score
        if (SeriesDelta != 0)
        {
            var sameMatches = _shapes.GetMatches(_maxNumber, 0, false,false);
            totalMatches.CombineMatchesInfo(sameMatches, ApplicationState.UserStatistics.IsControl());
        }
        if (totalMatches.NumberOfMatches>0)
            yield return StartCoroutine(HandleMatches(totalMatches));
        else
            if (!ApplicationState.UserStatistics.IsControl())
                _controllerScript.IncreaseScore(-5);
        if (ApplicationState.Score >= 0)
        {
            _state = GameState.Playing;
            yield break;
        }
        _state = GameState.Lost;
        _controllerScript.LoseGame(LoseReasons.Points);
    }

    private void InstantiateAndPlaceNewCell(int row, int column, GameObject cellPrefab)
    {
        var location = calculate_cell_location(row, column);
        var go = Instantiate(cellPrefab, new Vector2(location[0], -location[1]), Quaternion.identity);
        go.transform.SetParent(GameField.transform, false);
        go.layer = 5; //5=UI Layer
        var goTransform = go.transform as RectTransform;
        goTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _cellSize.x);
        goTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _cellSize.y);

        var numberValue = _shapes.GenerateNumber(_maxNumber);
        //assign the specific properties
        go.GetComponent<NumberCell>().Assign(numberValue, row, column);
        go.GetComponent<Image>().overrideSprite = _numberSquareSprites[numberValue - 1];
        SetTileColorBase(go);
        _shapes.Add(go);
    }

    private float[] calculate_cell_location(int row, int col)
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
        _state = GameState.Animating;
        SeriesDelta += 1;
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
            var location = calculate_cell_location(item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column);
            var itemRectTransform = item.GetComponent<RectTransform>() as RectTransform;
            itemRectTransform.ZKanchoredPositionTo(new Vector2(location[0], -location[1]), duration * distance).start();
            //Debug.logger.Log("{180217|2143",String.Format("Moved object to x={0} y={1}", location[0], -location[1]));
        }
    }
    public static void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        var sp1 = hitGo.GetComponent<CanvasRenderer>();
        var sp2 = hitGo2.GetComponent<CanvasRenderer>();
        if (sp1.transform.GetSiblingIndex() <= sp2.transform.GetSiblingIndex())
        {
            sp1.transform.SetSiblingIndex(1);
            sp2.transform.SetSiblingIndex(0);
        }
    }

    private void RemoveFromScene(GameObject item)
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
                newCell.GetComponent<Image>().overrideSprite = _numberSquareSprites[numberValue-1];
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
        if (ApplicationState.Score < NextLevelScore[SeriesDelta]) return;
        LevelUp();
        _controllerScript.LevelUp(SeriesDelta);
    }
}
