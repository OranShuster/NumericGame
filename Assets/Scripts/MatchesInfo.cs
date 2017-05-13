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

    public void AddTile(GameObject go, bool control,bool withScore )
    {
        _matchedCells.Add(go);
        if (!withScore) return;
        if (control)
            AddedScore += 1;
        else
            AddedScore += go.GetComponent<NumberCell>().Value;
    }

    private void AddTiles(IEnumerable<GameObject> gos,bool control, bool withScore=true)
    {
        foreach (var item in gos)
            AddTile(item,control,withScore);
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
        return String.Format("numbers - {0} Score - {1}",numbers,totalScore);
    }

    public void CombineMatchesInfo(MatchesInfo other, bool control, bool withScore=false)
    {
        NumberOfMatches += other.NumberOfMatches;
        AddTiles(other.MatchedCells,control,withScore);
    }
}

