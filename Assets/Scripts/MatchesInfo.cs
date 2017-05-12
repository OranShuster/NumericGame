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

    public void AddObject(GameObject go,bool withScore=true)
    {
        _matchedCells.Add(go);
        if (withScore)
            AddedScore += go.GetComponent<NumberCell>().Value;

    }

    public void AddObjectRange(IEnumerable<GameObject> gos,bool withScore=true)
    {
        foreach (var item in gos)
        {
            AddObject(item);
        }
        if (gos.Count() >= 3)
            NumberOfMatches++;
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

    public void CombineMatchesInfo(MatchesInfo other,bool withScore=false)
    {
        if (withScore)
            AddedScore += other.AddedScore;
        NumberOfMatches += other.NumberOfMatches;
        AddObjectRange(other.MatchedCells);
    }
}

