using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Xml.Schema;
using UnityEditor;

public class ShapesManager : MonoBehaviour
{
    public Text DebugText, ScoreText;
    public bool ShowDebugInfo = false;
    //candy graphics taken from http://opengameart.org/content/candy-pack-1

    public ShapesArray Shapes;

    private int _score;
	private int _numberStyle = 0;
    private int _seriesDelta = 0;
    private int _nextLevelScore=1000;


    public readonly Vector2 BottomRight = new Vector2(-2.37f, -4.27f);
    public readonly Vector2 CandySize = new Vector2(0.85f, 0.85f);

    private GameState _state = GameState.None;
    private GameObject _hitGo = null;
    private Vector2[] _spawnPositions;
    public GameObject NumberSquarePrefab;
    public GameObject[] ExplosionPrefabs;
    public Texture2D EmptyProgressBar;
    public Texture2D FullProgressBar;

    public static Texture2D ProgressBarTexture;
    public static GUIStyle ProgressBarStyle;

    public static int MaxNumber;
	public static int Rows;
	public static int Columns;
	public Sprite[] NumberSquareSprites;

    //private IEnumerator CheckPotentialMatchesCoroutine;
    //IEnumerable<GameObject> potentialMatches;

    public SoundManager SoundManager;
    void Awake()
    {
        DebugText.enabled = ShowDebugInfo;
        ProgressBarTexture = new Texture2D(1, 1);
        ProgressBarStyle = new GUIStyle();
    }

    // Use this for initialization
    void Start()
    {
        //InitializeTypesOnPrefabShapesAndBonuses();
		InitializeBoardConstants();
        InitializeCandyAndSpawnPositions();

        //StartCheckForPotentialMatches();
    }

	public void InitializeBoardConstants(){
        NumberSquareSprites = Resources.LoadAll<Sprite>("Images/Numbers");
        NumberSquareSprites = NumberSquareSprites.OrderBy(t =>Convert.ToInt32(t.name)).ToArray();
        MaxNumber = NumberSquareSprites.Count();
        Rows = MaxNumber;
		Columns = MaxNumber;
	}

    public void InitializeCandyAndSpawnPositionsFromPremadeLevel()
    {
        InitializeVariables();

        var premadeLevel = DebugUtilities.FillShapesArrayFromResourcesData();

        if (Shapes != null)
            DestroyAllCandy();

        Shapes = new ShapesArray();
        _spawnPositions = new Vector2[Columns];

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {

                GameObject newCandy = null;

                newCandy = GetSpecificCandyForPremadeLevel(premadeLevel[row, column]);

                InstantiateAndPlaceNewCandy(row, column, newCandy);

            }
        }

        SetupSpawnPositions();
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

				var newCandy = NumberSquarePrefab;
				var newCandyShape = newCandy.GetComponent<Shape>();
                newCandyShape.Value = Shapes.GenerateNumber(MaxNumber);
                InstantiateAndPlaceNewCandy(row, column, newCandy);

                //           //check if two previous horizontal are of the same type
                //           while (column >= 2 && shapes[row, column - 1].GetComponent<Shape>().IsPartOfSeries(newCandyShape,seriesDelta)
                //               && shapes[row, column - 2].GetComponent<Shape>().IsPartOfSeries(newCandyShape,seriesDelta))
                //           {
                //newCandyShape.Value = generateNumber();
                //           }

                //           //check if two previous vertical are of the same type
                //           while (row >= 2 && shapes[row - 1, column].GetComponent<Shape>().IsPartOfSeries(newCandy.GetComponent<Shape>(),seriesDelta)
                //               && shapes[row - 2, column].GetComponent<Shape>().IsPartOfSeries(newCandyShape,seriesDelta))
                //           {
                //newCandyShape.Value = generateNumber();
                //           }
            }
        }
        SetupSpawnPositions();
        ClearBoardMatches();
    }

    private void ClearBoardMatches()
    {
        var matches = new MatchesInfo();
        for (var row = 0; row < Rows; row++)
            for (var col = 0; col < Columns; col++)
                if (!matches.MatchedCandy.Contains(Shapes[row, col]))
                    matches.AddObjectRange(Shapes.GetMatches(Shapes[row, col],_seriesDelta).MatchedCandy);
        var totalMatches = matches.MatchedCandy;
        StartCoroutine(HandleMatches(totalMatches, false, false));
    }


    private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy)
    {
        var go = Instantiate(newCandy,
            BottomRight + new Vector2(column * CandySize.x, row * CandySize.y), Quaternion.identity)
            as GameObject;

		var numberValue = newCandy.GetComponent<Shape> ().Value;
        //assign the specific properties
		go.GetComponent<Shape>().Assign(numberValue, row, column);
		go.GetComponent<SpriteRenderer> ().sprite =NumberSquareSprites[numberValue-1];
        Shapes[row, column] = go;
    }

    private void SetupSpawnPositions()
    {
        //create the spawn positions for the new shapes (will pop from the 'ceiling')
        for (var column = 0; column < Columns; column++)
        {
            _spawnPositions[column] = BottomRight
                + new Vector2(column * CandySize.x, Rows * CandySize.y);
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

    void OnGUI()
    {
        
    }

    public static void GuiDrawRect(Rect position, Color color)
    {
        ProgressBarTexture.SetPixel(0, 0, color);
        ProgressBarTexture.Apply();
        ProgressBarStyle.normal.background = ProgressBarTexture;
        GUI.Box(position, GUIContent.none, ProgressBarStyle);
    }

    // Update is called once per frame
    void Update()
    {
        if (ShowDebugInfo)
            DebugText.text = DebugUtilities.GetArrayContents(Shapes);

        if (_state == GameState.None)
        {
            //user has clicked or touched
            if (Input.GetMouseButtonDown(0))
            {
                //get the hit position
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null) //we have a hit!!!
                {
                    _hitGo = hit.collider.gameObject;
                    _hitGo.GetComponent<SpriteRenderer>().color = Color.red;
                    _state = GameState.SelectionStarted;
                }
                
            }
        }
        else if (_state == GameState.SelectionStarted)
        {
            //user dragged
            if (Input.GetMouseButton(0))
            {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                //we have a hit
                if (hit.collider != null && _hitGo != hit.collider.gameObject)
                {
                    //if the two shapes are diagonally aligned (different row and column), just return
                    if (!Utilities.AreVerticalOrHorizontalNeighbors(_hitGo.GetComponent<Shape>(),
                        hit.collider.gameObject.GetComponent<Shape>()))
                    {
                        _hitGo.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
                        _state = GameState.None;
                    }
                    else
                    {
                        _state = GameState.Animating;
                        FixSortingLayer(_hitGo, hit.collider.gameObject);
                        StartCoroutine(FindMatchesAndCollapse(hit));
                    }
                }
            }
        }
    }

    private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        var sp1 = hitGo.GetComponent<SpriteRenderer>();
        var sp2 = hitGo2.GetComponent<SpriteRenderer>();
        if (sp1.sortingOrder <= sp2.sortingOrder)
        {
            sp1.sortingOrder = 1;
            sp2.sortingOrder = 0;
        }
    }

    private IEnumerator FindMatchesAndCollapse(RaycastHit2D hit2)
    {
        //get the second item that was part of the swipe
        var _hitGo2 = hit2.collider.gameObject;
        _hitGo2.GetComponent<SpriteRenderer>().color=Color.red;
        Shapes.Swap(_hitGo, _hitGo2);

        //move the swapped ones
        _hitGo.transform.positionTo(Constants.AnimationDuration, _hitGo2.transform.position);
        _hitGo2.transform.positionTo(Constants.AnimationDuration, _hitGo.transform.position);
        yield return new WaitForSeconds(Constants.AnimationDuration);

        //remove selection color from squares
        _hitGo2.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
        _hitGo.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);

        //get the matches via the helper methods
        var hitGomatchesInfo = Shapes.GetMatches(_hitGo,_seriesDelta);
        var hitGo2MatchesInfo = Shapes.GetMatches(_hitGo2,_seriesDelta);

        var totalMatches = hitGomatchesInfo.MatchedCandy.Union(hitGo2MatchesInfo.MatchedCandy).Distinct();

        StartCoroutine(HandleMatches(totalMatches));
        _state = GameState.None;
    }

    public IEnumerator HandleMatches(IEnumerable<GameObject> totalMatches, bool withScore = true, bool withEffects = true)
    {
        int score=0;
        while (totalMatches.Count() >= Constants.MinimumMatches)
        {
            //increase score
            if (withScore)
            {
                foreach (var match in totalMatches)
                {
                    score += match.GetComponent<Shape>().Value;
                }
                IncreaseScore(score);
            }

            SoundManager.PlayCrincle();

            foreach (var item in totalMatches)
            {
                item.GetComponent<SpriteRenderer>().color = Color.blue;
            }
            yield return new WaitForSeconds(1);

            foreach (var item in totalMatches)
            {
                Shapes.Remove(item);
                RemoveFromScene(item,withEffects);
            }

            //get the columns that we had a collapse
            var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

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
            totalMatches = Shapes.GetMatches(collapsedCandyInfo.AlteredCandy,_seriesDelta).
                Union(Shapes.GetMatches(newCandyInfo.AlteredCandy,_seriesDelta)).Distinct();
        }
        if (_score >= _nextLevelScore)
            LevelUp();
    }

    private void LevelUp()
    {
        _nextLevelScore += 1000;
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
				var go = NumberSquarePrefab;
				var newCandyShape = go.GetComponent<Shape> ();
                newCandyShape.Value = Shapes.GenerateNumber(MaxNumber);

                var newCandy = Instantiate(go, _spawnPositions[column], Quaternion.identity)
                    as GameObject;

                newCandy.GetComponent<SpriteRenderer>().sprite = NumberSquareSprites[newCandyShape.Value-1];
				
				newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Value, item.Row, item.Column);

				if (ShapesManager.MaxNumber - item.Row > newCandyInfo.MaxDistance)
					newCandyInfo.MaxDistance = ShapesManager.MaxNumber - item.Row;

                Shapes[item.Row, item.Column] = newCandy;
                newCandyInfo.AddCandy(newCandy);
            }
        }
        return newCandyInfo;
    }

    /// <summary>
    /// Animates gameobjects to their new position
    /// </summary>
    /// <param name="movedGameObjects"></param>
    private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects, int distance)
    {
        foreach (var item in movedGameObjects)
        {
            item.transform.positionTo(Constants.MoveAnimationMinDuration * distance, BottomRight +
                new Vector2(item.GetComponent<Shape>().Column * CandySize.x, item.GetComponent<Shape>().Row * CandySize.y));
        }
    }

    /// <summary>
    /// Destroys the item from the scene and instantiates a new explosion gameobject
    /// </summary>
    /// <param name="item"></param>
    private void RemoveFromScene(GameObject item,bool withEffects=true)
    {
        item.GetComponent<SpriteRenderer>().color = Color.blue;
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
        ScoreText.text = "Score: " + _score.ToString();
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
