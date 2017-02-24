using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
using Prime31.ZestKit;
using UnityEngine.EventSystems;

public class ShapesManager : MonoBehaviour
{
    //private int _numberStyle = 0;
    //private Texture2D EmptyProgressBar;
    //private Texture2D FullProgressBar;
    private int _score;
    private int _seriesDelta = 1;
    private int _nextLevelScore = Constants.NextLevelScore;
    private float _gameTimer = Constants.StartingGameTimer;
    private Vector2 _candySize;
    private GameObject _hitGo = null;
    private Vector2[] _spawnPositions;

    public GameState State;
    public Text ScoreText, TimerText;
    public Image GameField;
    public ShapesArray Shapes;
    public GameObject NumberSquarePrefab;
    public GameObject[] ExplosionPrefabs;
    public SoundManager SoundManager;
    public Image GameTimerBar;


    public static int MaxNumber;
	public static int Rows;
	public static int Columns;
	public Sprite[] NumberSquareSprites;

    void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        InitializeBoardConstants();
        InitializeCandyAndSpawnPositions();
    }

    // Update is called once per frame
    void Update()
    {
        if (State == GameState.Playing || State == GameState.SelectionStarted)
        {
            //update timer
            _gameTimer -= Time.deltaTime;
            TimerText.text = Math.Ceiling(_gameTimer).ToString();

            var gameTimerColor = _gameTimer.Remap(0, Constants.TimerMax, 0, 510);
            var gameTimerColorBlue = Math.Max(0, gameTimerColor - 255);
            var gameTimerColorGreen = gameTimerColor - gameTimerColorBlue;
            GameTimerBar.color = new Color(1, gameTimerColorGreen / 255f, gameTimerColorBlue / 255f);
            GameTimerBar.rectTransform.localScale = (new Vector3(Math.Min(_gameTimer / Constants.TimerMax,1), 1, 1));

            if (Input.GetMouseButtonDown(0))
            {
                var cursor = new PointerEventData(EventSystem.current);
                cursor.position = Input.mousePosition;
                List<RaycastResult> objectsHit = new List<RaycastResult>();
                EventSystem.current.RaycastAll(cursor, objectsHit);
                var hit = objectsHit.Find(x => x.gameObject.name == "NumberTile(Clone)").gameObject;
                if (hit!=null)
                {
                    if (State == GameState.Playing)
                    {
                        _hitGo = hit;
                        _hitGo.GetComponent<Image>().color = Constants.ColorSelected;
                        State = GameState.SelectionStarted;
                    }
                    else if (State == GameState.SelectionStarted)
                    {
                        if (hit != _hitGo)
                        {
                            //if the two shapes are diagonally aligned (different row and column), just return
                            if (!Utilities.AreNeighbors(_hitGo.GetComponent<Shape>(), hit.GetComponent<Shape>()))
                            {
                                _hitGo.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                State = GameState.Playing;
                            }
                            else
                            {
                                State = GameState.Animating;
                                FixSortingLayer(_hitGo, hit);
                                StartCoroutine(FindMatchesAndCollapse(hit));
                            }
                        }
                    }
                }
            }
        }
    }

    public void InitializeBoardConstants(){
        NumberSquareSprites = Resources.LoadAll<Sprite>("Images/Numbers");
        NumberSquareSprites = NumberSquareSprites.OrderBy(t =>Convert.ToInt32(t.name)).ToArray();
        MaxNumber = NumberSquareSprites.Count();
        Rows = MaxNumber;
		Columns = MaxNumber;
        int spacingSize = 5 * (MaxNumber+1);
        int playWidth = (int)GameField.rectTransform.rect.width - spacingSize;
        int playHeight = (int)GameField.rectTransform.rect.height - spacingSize;
        _candySize = new Vector2(playWidth / (float)MaxNumber, playHeight / (float)MaxNumber);
	}

    public void InitializeCandyAndSpawnPositions()
    {
        InitializeVariables();

        if (Shapes != null)
            DestroyAllCandy();

        Shapes = new ShapesArray();
        _spawnPositions = new Vector2[Columns];

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                InstantiateAndPlaceNewCandy(row, column, NumberSquarePrefab);
            }
        }
        SetupSpawnPositions();
        ClearBoardMatches();
    }

    private void ClearBoardMatches()
    {
        var totalMatches = Shapes.GetMatches(MaxNumber, _seriesDelta,new List<GameObject>(),false);
        var sameMatches = Shapes.GetMatches(MaxNumber, 0,totalMatches.MatchedCandy,false);
        totalMatches.AddObjectRange(sameMatches.MatchedCandy);
        StartCoroutine(HandleMatches(totalMatches,false, false));
        _gameTimer = Constants.StartingGameTimer;
    }


    private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy)
    {
        var location = calculate_cell_location(row, column);
        var go = Instantiate(newCandy, new Vector2(location[0], -(location[1])), Quaternion.identity) as GameObject;
        go.transform.SetParent(GameField.transform,false);
        go.layer = 5; //5=UI Layer
        var goTransform = go.transform as RectTransform;
        goTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,_candySize.x);
        goTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _candySize.y);


        var numberValue = Shapes.GenerateNumber(MaxNumber);
        //assign the specific properties
		go.GetComponent<Shape>().Assign(numberValue, row, column);
        go.GetComponentInChildren<Text>().text = numberValue.ToString();
        Shapes.Add(go);
    }

    private void SetupSpawnPositions()
    {
        //create the spawn positions for the new shapes (will pop from the 'ceiling')
        for (var column = 0; column < Columns; column++)
        {
            var location = calculate_cell_location(0, column);
            _spawnPositions[column] = new Vector2(location[0], +_candySize.y/2);
        }
    }

    private void DestroyAllCandy()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                Destroy(Shapes[row, column]);
            }
        }
    }

    private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        var sp1 = hitGo.GetComponent<CanvasRenderer>();
        var sp2 = hitGo2.GetComponent<CanvasRenderer>();
        if (sp1.transform.GetSiblingIndex() <= sp2.transform.GetSiblingIndex())
        {
            sp1.transform.SetSiblingIndex(1);
            sp2.transform.SetSiblingIndex(0);
        }
    }

    private IEnumerator FindMatchesAndCollapse(GameObject hit2)
    {
        MatchesInfo sameMatches=new MatchesInfo();
        //get the second item that was part of the swipe
        var _hitGo2 = hit2;
        _hitGo2.GetComponent<Image>().color=Constants.ColorSelected;
        Shapes.Swap(_hitGo, _hitGo2);

        //move the swapped ones
        _hitGo.transform.ZKlocalPositionTo(_hitGo2.transform.localPosition, Constants.AnimationDuration).start();
        _hitGo2.transform.ZKlocalPositionTo(_hitGo.transform.localPosition, Constants.AnimationDuration).start();
        yield return new WaitForSeconds(Constants.AnimationDuration);

        //remove selection color from squares
        _hitGo2.GetComponent<Image>().color = Constants.ColorBase;
        _hitGo.GetComponent<Image>().color = Constants.ColorBase;

        //get the matches via the helper methods
        var totalMatches = Shapes.GetMatches(MaxNumber, _seriesDelta,new List<GameObject>());
        if (_seriesDelta != 0)
        {
            sameMatches = Shapes.GetMatches(MaxNumber, 0,totalMatches.MatchedCandy, false);
        }
        _gameTimer += 5 * totalMatches.NumberOfMatches;
        totalMatches.AddObjectRange(sameMatches.MatchedCandy);
        StartCoroutine(HandleMatches(totalMatches));
        if(totalMatches.NumberOfMatches==0)
        {
            IncreaseScore(-5);
            if (_score < 0)
            {
                //LostText.enabled = true;
                //_state = GameState.Lost;
            }
        }
        State = GameState.Playing;
    }

    public IEnumerator HandleMatches(MatchesInfo totalMatches,bool withScore=true, bool withEffects = true)
    {
        while (totalMatches.MatchedCandy.Count() >= Constants.MinimumMatches)
        {
            Debug.logger.LogWarning("Match_Score{02061724}",totalMatches.PrintMatches());
            if (withScore)
            {                
                IncreaseScore(totalMatches.AddedScore);
            }
            foreach (var item in totalMatches.MatchedCandy.Distinct())
            {
                item.GetComponent<Image>().color = Constants.ColorMatched;
            }
            yield return new WaitForSeconds(2);

            foreach (var item in totalMatches.MatchedCandy.Distinct())
            {
                SoundManager.PlayCrincle();
                Shapes.Remove(item);
                RemoveFromScene(item,withEffects);
            }

            //get the columns that we had a collapse
            var columns = totalMatches.MatchedCandy.Select(go => go.GetComponent<Shape>().Column).Distinct();

            //the order the 2 methods below get called is important!!!
            //collapse the ones gone
            var collapsedCandyInfo = Shapes.Collapse(columns);
            //create new ones
            var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

            var maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

            MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
            MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);

            //will wait for both of the above animations
            yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

            //search if there are matches with the new/collapsed items
            totalMatches = Shapes.GetMatches(MaxNumber, _seriesDelta,new List<GameObject>());
            
            //search if there are empty mathes
            totalMatches.AddObjectRange(Shapes.GetMatches(MaxNumber,0,totalMatches.MatchedCandy,false).MatchedCandy);   
        }
        if (_score >= _nextLevelScore)
            LevelUp();
        State = GameState.Playing;
    }

    private void LevelUp()
    { 
        _nextLevelScore += Constants.NextLevelScore;
        _seriesDelta += 1;
    }

    /// <summary>
    /// Spawns new candy in columns that have missing ones
    /// </summary>
    /// <param name="columnsWithMissingCandy"></param>
    /// <returns>Info about new candies created</returns>
    private AlteredCandyInfo CreateNewCandyInSpecificColumns(IEnumerable<int> columnsWithMissingCandy)
    {
        var newCandyInfo = new AlteredCandyInfo();

        //find how many null values the column has
        foreach (var column in columnsWithMissingCandy)
        {
            var emptyItems = Shapes.GetEmptyItemsOnColumn(column);
            foreach (var item in emptyItems)
            {
                var newCandy = Instantiate(NumberSquarePrefab, _spawnPositions[column], Quaternion.identity) as GameObject;
                var numberValue = Shapes.GenerateNumber(MaxNumber);
                newCandy.layer = 5; //5=UI Layer
                newCandy.transform.SetParent(GameField.transform,false);
                newCandy.GetComponent<Shape>().Assign(numberValue, item.Row, item.Column);
                newCandy.GetComponentInChildren<Text>().text = numberValue.ToString();
                var newCandyRectTransform = newCandy.GetComponent<RectTransform>() as RectTransform;
                newCandyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _candySize.x);
                newCandyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _candySize.y);
                //assign the specific properties
                if (ShapesManager.MaxNumber - item.Row > newCandyInfo.MaxDistance)
					newCandyInfo.MaxDistance = ShapesManager.MaxNumber - item.Row;
                Shapes.Add(newCandy);
                newCandyInfo.AddCandy(newCandy);
            }
        }
        return newCandyInfo;
    }

    /// <summary>
    /// Animates gameobjects to their new position
    /// </summary>
    /// <param name="movedGameObjects"></param>
    /// <param name="distance"></param>
    private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects, int distance)
    {
        foreach (var item in movedGameObjects)
        {
            var location = calculate_cell_location(item.GetComponent<Shape>().Row, item.GetComponent<Shape>().Column);
            var itemRectTransform = item.GetComponent<RectTransform>() as RectTransform;
            itemRectTransform.ZKanchoredPositionTo(new Vector2(location[0], -location[1]), Constants.MoveAnimationMinDuration * distance).start();
            Debug.logger.LogWarning("{180217|2143", String.Format("Moved object to x={0} y={1}",location[0],-location[1]));
        }
    }

    private float[] calculate_cell_location(int row, int col)
    {
        var spacingX = 5 * (col + 1);
        var spacingY = 5 * (row + 1);
        var locationX = col * _candySize.x + _candySize.x / 2;
        var locationY = row * _candySize.y + _candySize.y / 2;
        return new [] { locationX + spacingX, locationY + spacingY };
    }

    /// <summary>
    /// Destroys the item from the scene and instantiates a new explosion gameobject
    /// </summary>
    /// <param name="item"></param>
    private void RemoveFromScene(GameObject item,bool withEffects=true)
    {
        item.GetComponent<Image>().color = Constants.ColorMatched;
        if (withEffects)
        {
            var explosion = GetRandomExplosion();
            var newExplosion = Instantiate(explosion, item.transform.position, Quaternion.identity) as GameObject;
            Destroy(newExplosion, Constants.ExplosionDuration);
        }
        Destroy(item);
    }

    private void InitializeVariables()
    {
        _score = 0;
        ShowScore();
    }

    private void IncreaseScore(int amount)
    {
        _score += amount;
        ShowScore();
    }

    private void ShowScore()
    {
        ScoreText.text = _score.ToString();
    }

    /// <summary>
    /// Get a random explosion
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomExplosion()
    {
        return ExplosionPrefabs[UnityEngine.Random.Range(0, ExplosionPrefabs.Length)];
    }

    /// <summary>
    /// Gets a specific candy or Bonus based on the premade level information.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private GameObject GetSpecificCandyForPremadeLevel(string info)
    {
		var newCandy = NumberSquarePrefab;
		newCandy.GetComponent<Shape>().Value = int.Parse(info);
		return newCandy;
    }
}
