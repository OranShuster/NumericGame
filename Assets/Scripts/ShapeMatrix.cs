﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapesMatrix
{
    private GameObject[,] _shapes;
    private int[,] _values;
    private int[] _numberCounts;

    public ShapesMatrix(int rows, int columns, int maxNumber)
    {
        _shapes = new GameObject[rows, columns];
        _values = new int[rows, columns];
        _numberCounts = new int[maxNumber + 1];
    }

    public GameObject this[int row, int column]{
        get{
            try { return _shapes[row, column]; }
            catch { return null; }
        }
        set {
            if (value != null)
            {
                _shapes[row, column] = value;
                _values[row, column] = value.GetComponent<NumberCell>().Value;
            }
        }
    }

    public int GetValue(int row,int col)
    {
        return _values[row, col];
    }

    public void Swap(GameObject g1, GameObject g2)
    {

        var g1Shape = g1.GetComponent<NumberCell>();
        var g2Shape = g2.GetComponent<NumberCell>();

        //get array indexes
        var g1Row = g1Shape.Row;
        var g1Column = g1Shape.Column;
        var g2Row = g2Shape.Row;
        var g2Column = g2Shape.Column;

        //swap their respective properties
        NumberCell.SwapFields(g1Shape, g2Shape);

        //swap them in the array
        var temp = _shapes[g1Row, g1Column];
        _shapes[g1Row, g1Column] = _shapes[g2Row, g2Column];
        _shapes[g2Row, g2Column] = temp;
    }

    public MatchesInfo GetMatches(int boardSize,int seriesDelta,IEnumerable<GameObject> matchedGameObjects, bool countScore = true)
    {
        var matchesInfo = new MatchesInfo();
        for (var ind = 0; ind < boardSize; ind++)
        {
            var horizontalMatches = GetMatchesOnIndex(ind, boardSize, seriesDelta,true);
            matchesInfo.AddObjectRange(horizontalMatches.MatchedCells,countScore);
            var verticalMatches = GetMatchesOnIndex(ind, boardSize, seriesDelta,false);
            matchesInfo.AddObjectRange(verticalMatches.MatchedCells,countScore);
        }
        return matchesInfo;
    }

    private MatchesInfo GetMatchesOnIndex(int ind, int length, int delta, bool isRow)
    {
        var allMatches = new MatchesInfo();
        var values = ToArray(ind, isRow);
        int numOfMatches;
        //left->right or up->down
        var seriesIndexes = FindSeries(values, delta, out numOfMatches);
        allMatches.NumberOfMatches += numOfMatches;
        if (isRow)
            foreach (var colIndex in seriesIndexes)
                allMatches.AddObject(_shapes[ind, colIndex]);
        else
            foreach (var rowIndex in seriesIndexes)
                allMatches.AddObject(_shapes[rowIndex, ind]);
        if (delta == 0)
            return allMatches;
        //right->left or down->up
        seriesIndexes = FindSeries(values, -delta, out numOfMatches);
        allMatches.NumberOfMatches += numOfMatches;
        if (isRow)
            foreach (var colIndex in seriesIndexes)
                allMatches.AddObject(_shapes[ind, colIndex]);
        else
            foreach (var rowIndex in seriesIndexes)
                allMatches.AddObject(_shapes[rowIndex, ind]);
        return allMatches;
    }

    private List<int> FindSeries(int[] values, int delta,out int numOfMatches)
    {
        numOfMatches = 0;
        var allSeriesIndexes = new List<int>();
        var curSeriesIndexes = new List<int>();
        var curDiff = 0;
        for (var ind = 0; ind < values.Length - 1; ind++)
        {
            curDiff = values[ind + 1] - values[ind];
            if (curDiff == delta)
            {
                if (curSeriesIndexes.Count == 0)
                    curSeriesIndexes.Add(ind);
                curSeriesIndexes.Add(ind+1);
            }
            else
            {
                if (curSeriesIndexes.Count >= 3)
                {
                    allSeriesIndexes.AddRange(curSeriesIndexes);
                    numOfMatches++;
                }
                curSeriesIndexes.Clear();
            }
        }
        if (curSeriesIndexes.Count >= 3)
        {
            allSeriesIndexes.AddRange(curSeriesIndexes);
            numOfMatches++;
        }
        return allSeriesIndexes;
    }

    public void Remove(GameObject item)
    {
        _numberCounts[item.GetComponent<NumberCell>().Value]--;
        _shapes[item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column] = null;
        _values[item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column] = 0;
        //Debug.logger.Log("190217|0104", String.Format("values - {0} total - {1} Removed {2}",
        //Utilities.PrintArray(_numberCounts), _numberCounts.Sum(), item.GetComponent<NumberCell>().Value));
    }

    public void Add(GameObject item)
    {
        _numberCounts[item.GetComponent<NumberCell>().Value]++;
        _shapes[item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column] = item;
        _values[item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column] = item.GetComponent<NumberCell>().Value;
        //Debug.logger.Log("180217|2224", String.Format("values - {0} total - {1} Added - {2}",
        //Utilities.PrintArray(_numberCounts), _numberCounts.Sum(), item.GetComponent<NumberCell>().Value));
    }

    public AlteredCellInfo Collapse(IEnumerable<int> columns)
    {
        var collapseInfo = new AlteredCellInfo();
        //search in every column
        foreach (var column in columns)
        {
            //begin from bottom row
			for (var row = _shapes.GetLength(0)-1; row >= 0 ; row--)
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
                            _shapes[row, column].GetComponent<NumberCell>().Row = row;
                            _shapes[row, column].GetComponent<NumberCell>().Column = column;

                            collapseInfo.AddCell(_shapes[row, column]);
                            break;
                        }
                    }
                }
            }
        }

        return collapseInfo;
    }

    public IEnumerable<CellTuple> GetEmptyItemsOnColumn(int column,int rows)
    {
        var emptyItems = new List<CellTuple>();
		for (var row = 0; row < rows; row++)
        {
            if (_shapes[row, column] == null)
                emptyItems.Add(new CellTuple() { Row = row, Column = column});
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
    int[] ToArray(int ind,bool isRow)
    {
        var values = new int[_shapes.GetLength(0)];
        if (isRow)
            for (var col=0;col<_shapes.GetLength(0);col++)
                values[col] = _shapes[ind, col].GetComponent<NumberCell>().Value;
        else
            for (var row = 0; row < _shapes.GetLength(0); row++)
                values[row] = _shapes[row, ind].GetComponent<NumberCell>().Value;
        return values;
    }
}

public class CellTuple
{
    public int Row { get; set; }
    public int Column { get; set; }
}