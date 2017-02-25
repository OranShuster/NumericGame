using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom class to accomodate useful stuff for our shapes array
/// </summary>
public class ShapesArray
{
    private GameObject[,] _shapes;
    private int[] _numberCounts;

    public ShapesArray(int rows, int columns, int maxNumber)
    {
        _shapes = new GameObject[rows, columns];
        _numberCounts = new int[maxNumber + 1];
    }

    public GameObject this[int row, int column]{
        get{
            try { return _shapes[row, column]; }
            catch { return null; }
        }
        set {
            if (value != null)
                _shapes[row, column] = value;
        }
    }

    /// <summary>
    /// Swaps the position of two items, also keeping a backup
    /// </summary>
    /// <param name="g1"></param>
    /// <param name="g2"></param>
    public void Swap(GameObject g1, GameObject g2)
    {

        var g1Shape = g1.GetComponent<Shape>();
        var g2Shape = g2.GetComponent<Shape>();

        //get array indexes
        var g1Row = g1Shape.Row;
        var g1Column = g1Shape.Column;
        var g2Row = g2Shape.Row;
        var g2Column = g2Shape.Column;

        //swap their respective properties
        Shape.SwapFields(g1Shape, g2Shape);

        //swap them in the array
        var temp = _shapes[g1Row, g1Column];
        _shapes[g1Row, g1Column] = _shapes[g2Row, g2Column];
        _shapes[g2Row, g2Column] = temp;
    }

    /// <summary>
    /// Returns the matches for the board
    /// </summary>
    /// <param name="boardSize"></param>
    /// <param name="seriesDelta"></param>
    /// <param name="countScore"></param>
    /// <returns></returns>
    public MatchesInfo GetMatches(int boardSize,int seriesDelta,IEnumerable<GameObject> matchedGameObjects, bool countScore = true)
    {
        var matchesInfo = new MatchesInfo();
        for (int ind = 0; ind < boardSize; ind++)
        {
            var horizontalMatches = GetMatchesHorizontally(ind, boardSize, seriesDelta, matchedGameObjects);
            matchesInfo.AddObjectRange(horizontalMatches.MatchedCandy);
            if (countScore)
                matchesInfo.AddedScore += horizontalMatches.AddedScore;
            var verticalMatches = GetMatchesVertically(ind, boardSize, seriesDelta, matchedGameObjects);
            matchesInfo.AddObjectRange(verticalMatches.MatchedCandy);
            if (countScore)
                matchesInfo.AddedScore += verticalMatches.AddedScore;
        }
        return matchesInfo;
    }


    /// <summary>
    /// Searches horizontally for matches
    /// </summary>
    /// <param name="rowToCheck"></param>
    /// <param name="rowLength"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    private MatchesInfo GetMatchesHorizontally(int rowToCheck,int rowLength,int delta,IEnumerable<GameObject> matchedGameObjects)
    {
        var allMatches = new MatchesInfo();
        var curMatches = new List<GameObject>();
        int curDiff=0;
        //go left to right
        for (int col = 0; col < rowLength-1; col++)
        {
            curDiff = _shapes[rowToCheck, col + 1].GetComponent<Shape>().Value -
                        _shapes[rowToCheck, col].GetComponent<Shape>().Value ;
            if (curDiff == delta)
            {
                if (curMatches.Count==0)
                    curMatches.Add(_shapes[rowToCheck, col]);
                curMatches.Add(_shapes[rowToCheck, col + 1]);
            }

            else
            {
                if (curMatches.Count >= 3)
                {
                    allMatches.AddObjectRange(curMatches);
                    allMatches.NumberOfMatches++;
                    foreach (var item in curMatches)
                    {
                        allMatches.AddedScore += item.GetComponent<Shape>().Value;
                    }
                }
                curMatches.Clear();
            }
        }
        if (curMatches.Count >= 3)
        {
            allMatches.AddObjectRange(curMatches);
            allMatches.NumberOfMatches++;
            foreach (var item in curMatches)
            {
                allMatches.AddedScore += item.GetComponent<Shape>().Value;
            }
        }
        curMatches.Clear();
        //right to left
        for (int col = rowLength-1; col >0; col--)
        {
            //if (!matchedGameObjects.Contains(_shapes[rowToCheck, col]))
            //{
                curDiff = _shapes[rowToCheck, col - 1].GetComponent<Shape>().Value -
                          _shapes[rowToCheck, col].GetComponent<Shape>().Value;
                if (curDiff == delta)
                {
                    if(curMatches.Count==0)
                        curMatches.Add(_shapes[rowToCheck, col]);
                    curMatches.Add(_shapes[rowToCheck, col - 1]);
                }

                else
                {
                    if (curMatches.Count >= 3)
                    {
                        allMatches.AddObjectRange(curMatches);
                        allMatches.NumberOfMatches++;
                        foreach (var item in curMatches)
                        {
                            allMatches.AddedScore += item.GetComponent<Shape>().Value;
                        }
                    }
                    curMatches.Clear();
                }
            //}
        }
        if (curMatches.Count >= 3)
        {
            allMatches.AddObjectRange(curMatches);
            allMatches.NumberOfMatches++;
            foreach (var item in curMatches)
            {
                allMatches.AddedScore += item.GetComponent<Shape>().Value;
            }
        }
        return allMatches;
    }

    /// <summary>
    /// Searches vertically for matches
    /// </summary>
    /// <param name="colToCheck"></param>
    /// <param name="colLength"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    private MatchesInfo GetMatchesVertically(int colToCheck,int colLength,int delta, IEnumerable<GameObject> matchedGameObjects)
    {
        var allMatches = new MatchesInfo();
        var curMatches = new List<GameObject>();
        var curDiff = 0;
        //bottom to top
        for (int row = 0; row < colLength-1; row++)
        {
            //if (!matchedGameObjects.Contains(_shapes[row, colToCheck]))
            //{
                curDiff = _shapes[row + 1, colToCheck].GetComponent<Shape>().Value -
                 _shapes[row, colToCheck].GetComponent<Shape>().Value;
                if (curDiff == delta)
                {
                    if (curMatches.Count==0)
                        curMatches.Add(_shapes[row, colToCheck]);
                    curMatches.Add(_shapes[row + 1, colToCheck]);
                }

                else
                {
                    if (curMatches.Count >= 3)
                    {
                        allMatches.AddObjectRange(curMatches);
                        allMatches.NumberOfMatches++;
                        foreach (var item in curMatches)
                        {
                            allMatches.AddedScore += item.GetComponent<Shape>().Value;
                        }
                    }
                    curMatches.Clear();
                }
            //}
        }
        curMatches.Clear();
        //top to bottom
        for (int row = colLength - 1; row > 0; row--)
        {
            if (!matchedGameObjects.Contains(_shapes[row, colToCheck]))
            {
                curDiff = _shapes[row - 1, colToCheck].GetComponent<Shape>().Value -
                  _shapes[row, colToCheck].GetComponent<Shape>().Value;
                if (curDiff == delta)
                {
                    if (curMatches.Count==0)
                        curMatches.Add(_shapes[row, colToCheck]);
                    curMatches.Add(_shapes[row - 1, colToCheck]);
                }
                else
                {
                    if (curMatches.Count >= 3)
                    {
                        allMatches.AddObjectRange(curMatches);
                        allMatches.NumberOfMatches++;
                        foreach (var item in curMatches)
                        {
                            allMatches.AddedScore += item.GetComponent<Shape>().Value;
                        }
                    }
                    curMatches.Clear();
                }
            }
        }
        return allMatches;
    }

    public void Remove(GameObject item)
    {
        _numberCounts[item.GetComponent<Shape>().Value]--;
        _shapes[item.GetComponent<Shape>().Row, item.GetComponent<Shape>().Column] = null;
        Debug.logger.LogWarning("190217|0104", String.Format("values - {0} total - {1} Removed {2}",
        Utilities.PrintArray(_numberCounts), _numberCounts.Sum(), item.GetComponent<Shape>().Value));
    }

    public void Add(GameObject item)
    {
        _numberCounts[item.GetComponent<Shape>().Value]++;
        _shapes[item.GetComponent<Shape>().Row, item.GetComponent<Shape>().Column] = item;
        Debug.logger.LogWarning("180217|2224", String.Format("values - {0} total - {1} Added - {2}",
        Utilities.PrintArray(_numberCounts), _numberCounts.Sum(), item.GetComponent<Shape>().Value));
    }

    /// <summary>
    /// Collapses the array on the specific columns, after checking for empty items on them
    /// </summary>
    /// <param name="columns"></param>
    /// <returns>Info about the GameObjects that were moved</returns>
    public AlteredCandyInfo Collapse(IEnumerable<int> columns)
    {
        var collapseInfo = new AlteredCandyInfo();
        //search in every column
        foreach (var column in columns)
        {
            //begin from bottom row
			for (var row = 9; row >= 0 ; row--)
            {
                //if you find a null item
                if (_shapes[row, column] == null)
                {
                    //start searching for the first non-null
					for (var row2 = row - 1; row2 >= 0; row2--)
                    {
                        //if you find one, bring it down (i.e. replace it with the null you found)
                        if (_shapes[row2, column] != null)
                        {
                            _shapes[row, column] = _shapes[row2, column];
                            _shapes[row2, column] = null;

                            //calculate the biggest distance
                            if (row2 - row > collapseInfo.MaxDistance) 
                                collapseInfo.MaxDistance = row2 - row;

                            //assign new row and column (name does not change)
                            _shapes[row, column].GetComponent<Shape>().Row = row;
                            _shapes[row, column].GetComponent<Shape>().Column = column;

                            collapseInfo.AddCandy(_shapes[row, column]);
                            break;
                        }
                    }
                }
            }
        }

        return collapseInfo;
    }

    /// <summary>
    /// Searches the specific column and returns info about null items
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IEnumerable<ShapeInfo> GetEmptyItemsOnColumn(int column)
    {
        var emptyItems = new List<ShapeInfo>();
		for (var row = 0; row < ShapesManager.Rows; row++)
        {
            if (_shapes[row, column] == null)
                emptyItems.Add(new ShapeInfo() { Row = row, Column = column});
        }
        return emptyItems;
    }

    public int GenerateNumber(int maxNumber)
    {
        var chances = new List<int>();
        var totalSquares = (int)Math.Pow(maxNumber,2);
        for (var currentNum = 1; currentNum < maxNumber + 1; currentNum++)
        {
            chances.AddRange(Enumerable.Repeat(currentNum, totalSquares - _numberCounts[currentNum]));
        }
        return chances[UnityEngine.Random.Range(0, chances.Count-1)];
    }
}

