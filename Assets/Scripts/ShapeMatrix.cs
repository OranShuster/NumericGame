using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public GameObject this[int row, int column]
    {
        get { return _shapes[row, column]; }
        set
        {
            if (value == null) return;
            _shapes[row, column] = value;
            _values[row, column] = value.GetComponent<NumberCell>().Value;
        }
    }

    public int GetValue(int row, int col)
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

    public MatchesInfo GetMatches(int boardSize, bool control, bool withScore)
    {
        var matchesInfo = new MatchesInfo();
        for (var ind = 0; ind < boardSize; ind++)
        {
            var rowMatches = GetMatchesOnIndex(ind, true, control);
            matchesInfo.CombineMatchesInfo(rowMatches, withScore);
            var colMatches = GetMatchesOnIndex(ind, false, control);
            matchesInfo.CombineMatchesInfo(colMatches, withScore);
        }
        if (!withScore)
            matchesInfo.AddedScore = 0;
        return matchesInfo;
    }

    private MatchesInfo GetMatchesOnIndex(int ind, bool isRow, bool control)
    {
        var allMatches = new MatchesInfo();
        var addedScore = 0;
        var values = ToArray(ind, isRow);
        var maxSize = GameManager.Levels.maxSeriesSize;
        var minSize = GameManager.Levels.minSeriesSize;
        var usedStartingIndex = values.Length + 1;
        var usedEndingIndex = -1;
        var seriesIndexes = new List<int>();
        for (var subArraySize = maxSize; subArraySize >= minSize; subArraySize--)
        {
            for (var startIndex = 0; startIndex <= values.Length - subArraySize; startIndex++)
            {
                var subArray = values.SubArray(startIndex, subArraySize);
                var endIndex = startIndex + subArraySize - 1;
                if (startIndex > usedStartingIndex && startIndex < usedEndingIndex && endIndex < usedEndingIndex &&
                    endIndex > usedStartingIndex)
                    continue;
                var seriesInfo = GameManager.Levels.get_series_info(subArray.ToInt()) ??
                                 GameManager.Levels.get_series_info(subArray.Reverse().ToArray().ToInt());
                if (seriesInfo == null)
                    continue;
                usedStartingIndex = startIndex;
                usedEndingIndex = endIndex;
                if (control)
                    addedScore += Constants.ContrlScorePerMatch;
                else
                    addedScore += seriesInfo.Score;
                allMatches.NumberOfMatches++;
                seriesIndexes.AddRange(Enumerable.Range(startIndex, subArraySize));
            }
        }
        if (isRow)
            foreach (var colIndex in seriesIndexes)
                allMatches.AddTile(_shapes[ind, colIndex]);
        else
            foreach (var rowIndex in seriesIndexes)
                allMatches.AddTile(_shapes[rowIndex, ind]);
        allMatches.AddedScore = addedScore;
        return allMatches;
    }

    public void Remove(GameObject item)
    {
        _numberCounts[item.GetComponent<NumberCell>().Value]--;
        _shapes[item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column] = null;
        _values[item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column] = 0;
    }

    public void Add(GameObject item)
    {
        _numberCounts[item.GetComponent<NumberCell>().Value]++;
        _shapes[item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column] = item;
        _values[item.GetComponent<NumberCell>().Row, item.GetComponent<NumberCell>().Column] =
            item.GetComponent<NumberCell>().Value;
    }

    public AlteredCellInfo Collapse(IEnumerable<int> columns)
    {
        var collapseInfo = new AlteredCellInfo();
        //search in every column
        foreach (var column in columns)
        {
            //begin from bottom row
            for (var row = _shapes.GetLength(0) - 1; row >= 0; row--)
            {
                //if you find a null item
                if (_shapes[row, column] != null) continue;
                //start searching for the first non-null
                for (var row2 = row - 1; row2 >= 0; row2--)
                {
                    //if you find one, bring it down (i.e. replace it with the null you found)
                    if (_shapes[row2, column] == null) continue;
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

        return collapseInfo;
    }

    public IEnumerable<CellTuple> GetEmptyItemsOnColumn(int column, int rows)
    {
        var emptyItems = new List<CellTuple>();
        for (var row = 0; row < rows; row++)
            if (_shapes[row, column] == null)
                emptyItems.Add(new CellTuple {Row = row, Column = column});
        return emptyItems;
    }

    public int GenerateNumber(int maxNumber)
    {
        var chances = new List<int>();
        var totalSquares = (int) Math.Pow(maxNumber, 2);
        for (var currentNum = 1; currentNum < maxNumber + 1; currentNum++)
            chances.AddRange(Enumerable.Repeat(currentNum, totalSquares - _numberCounts[currentNum]));
        return chances[Random.Range(0, chances.Count - 1)];
    }

    private int[] ToArray(int ind, bool isRow)
    {
        var values = new int[_shapes.GetLength(0)];
        if (isRow)
            for (var col = 0; col < _shapes.GetLength(0); col++)
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