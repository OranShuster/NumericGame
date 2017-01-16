﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Custom class to accomodate useful stuff for our shapes array
/// </summary>
public class ShapesArray
{
	private GameObject[,] _shapes = new GameObject[ShapesManager.Rows, ShapesManager.Columns];
    private int[] _numberCounts = new int[ShapesManager.MaxNumber+1];

    public GameObject this[int row, int column]
    {
        get
        {
            try
            {
                return _shapes[row, column];
            }
            catch
            {
                return null;
            }
        }
        set
        {
            if (value != null)
                this._numberCounts[value.GetComponent<Shape>().Value]++;
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

        //swap them in the array
        var temp = _shapes[g1Row, g1Column];
        _shapes[g1Row, g1Column] = _shapes[g2Row, g2Column];
        _shapes[g2Row, g2Column] = temp;

        //swap their respective properties
        Shape.SwapFields(g1Shape, g2Shape);

    }

    /// <summary>
    /// Returns the matches found for a list of GameObjects
    /// MatchesInfo class is not used as this method is called on subsequent collapses/checks, 
    /// not the one inflicted by user's drag
    /// </summary>
    /// <param name="gos"></param>
    /// <param name="seriesDelta"></param>
    /// <returns></returns>
    public IEnumerable<GameObject> GetMatches(IEnumerable<GameObject> gos,int seriesDelta)
    {
        var matches = new List<GameObject>();
        foreach (var go in gos)
        {
            matches.AddRange(GetMatches(go,seriesDelta).MatchedCandy);
        }
        return matches.Distinct();
    }

    /// <summary>
    /// Returns the matches found for a single GameObject
    /// </summary>
    /// <param name="go"></param>
    /// <param name="seriesDelta"></param>
    /// <returns></returns>
    public MatchesInfo GetMatches(GameObject go,int seriesDelta)
    {
        var matchesInfo = new MatchesInfo();

        matchesInfo.AddObjectRange(GetMatchesHorizontally(go,seriesDelta));
        matchesInfo.AddObjectRange(GetMatchesHorizontally(go,-seriesDelta));

        matchesInfo.AddObjectRange(GetMatchesVertically(go,seriesDelta));
        matchesInfo.AddObjectRange(GetMatchesVertically(go,-seriesDelta));

        return matchesInfo;
    }

    /// <summary>
    /// Searches horizontally for matches
    /// </summary>
    /// <param name="go"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    private IEnumerable<GameObject> GetMatchesHorizontally(GameObject go,int delta)
    {
        var matches = new List<GameObject> {go};
        var shape = go.GetComponent<Shape>();
        //check left
        if (shape.Column != 0)
            for (var column = shape.Column - 1; column >= 0; column--)
            {
                var curDelta = delta * Math.Abs(shape.Column - column);
                if (_shapes[shape.Row, column].GetComponent<Shape>().IsPartOfSeries(shape,curDelta))
                    matches.Add(_shapes[shape.Row, column]);
                else
                    break;
            }

        //check right
        if (shape.Column != ShapesManager.Columns - 1)
			for (var column = shape.Column + 1; column < ShapesManager.Columns; column++)
            {
                var curDelta = delta * Math.Abs(shape.Column - column);
                if (_shapes[shape.Row, column].GetComponent<Shape>().IsPartOfSeries(shape,curDelta))
                    matches.Add(_shapes[shape.Row, column]);
                else
                    break;
            }

        //we want more than three matches
        if (matches.Count < Constants.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    /// <summary>
    /// Searches vertically for matches
    /// </summary>
    /// <param name="go"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    private IEnumerable<GameObject> GetMatchesVertically(GameObject go,int delta)
    {
        var matches = new List<GameObject> {go};
        var shape = go.GetComponent<Shape>();
        //check bottom
        if (shape.Row != 0)
            for (var row = shape.Row - 1; row >= 0; row--)
            {
                var curDelta = delta * Math.Abs(shape.Row - row); 
                if (_shapes[row, shape.Column] != null &&
                    _shapes[row, shape.Column].GetComponent<Shape>().IsPartOfSeries(shape,curDelta))
                {
                    matches.Add(_shapes[row, shape.Column]);
                }
                else
                    break;
            }

        //check top
		if (shape.Row != ShapesManager.Rows - 1)
			for (var row = shape.Row + 1; row < ShapesManager.Rows; row++)
            {
                var curDelta = delta * Math.Abs(shape.Row - row);
                if (_shapes[row, shape.Column] != null && 
                    _shapes[row, shape.Column].GetComponent<Shape>().IsPartOfSeries(shape,curDelta))
                {
                    matches.Add(_shapes[row, shape.Column]);
                }
                else
                    break;
            }


        if (matches.Count < Constants.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    /// <summary>
    /// Removes (sets as null) an item from the array
    /// </summary>
    /// <param name="item"></param>
    public void Remove(GameObject item)
    {
        _numberCounts[item.GetComponent<Shape>().Value]--;
        _shapes[item.GetComponent<Shape>().Row, item.GetComponent<Shape>().Column] = null;
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
			for (var row = 0; row < ShapesManager.Rows - 1; row++)
            {
                //if you find a null item
                if (_shapes[row, column] == null)
                {
                    //start searching for the first non-null
					for (var row2 = row + 1; row2 < ShapesManager.Rows; row2++)
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
                emptyItems.Add(new ShapeInfo() { Row = row, Column = column });
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

