using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MatchesInfo
{
    private List<GameObject> _matchedCells;
    public int NumberOfMatches;
    public int AddedScore;

    public IEnumerable<GameObject> MatchedCells
    {
        get
        {
            return _matchedCells;
        }
    }

    public void AddTile(GameObject go)
    {
        _matchedCells.Add(go);
    }

    private void AddTiles(IEnumerable<GameObject> gos)
    {
        foreach (var item in gos)
            AddTile(item);
    }

    public MatchesInfo()
    {
        _matchedCells = new List<GameObject>();
        NumberOfMatches = 0;
    }

    public string PrintMatches()
    {
        var numbers = "";
        foreach (var cell in _matchedCells)
        {
            numbers += "," + cell.GetComponent<NumberCell>().Value;
        }
        var totalScore = this.AddedScore.ToString();
        return string.Format("numbers - {0} Score - {1}",numbers,totalScore);
    }

    public void CombineMatchesInfo(MatchesInfo other, bool withScore)
    {
        NumberOfMatches += other.NumberOfMatches;
        if (withScore)
            AddedScore += other.AddedScore;
        AddTiles(other.MatchedCells);
    }
}

