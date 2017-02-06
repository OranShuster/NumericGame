using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MatchesInfo
{
    private List<GameObject> _matchedCandies;
    public int NumberOfMatches;
    public int AddedScore;

    /// <summary>
    /// Returns list of matched candy
    /// </summary>
    public IEnumerable<GameObject> MatchedCandy
    {
        get
        {
            return _matchedCandies;
        }
    }

    public void AddObject(GameObject go)
    {
        _matchedCandies.Add(go);
    }

    public void AddObjectRange(IEnumerable<GameObject> gos)
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
        _matchedCandies = new List<GameObject>();
        NumberOfMatches = 0;
    }

    public string PrintMatches()
    {
        string numbers = "";
        foreach (var candy in _matchedCandies)
        {
            numbers += "," + candy.GetComponent<Shape>().Value;
        }
        string totalScore = this.AddedScore.ToString();
        return String.Format("numbers - {0} Score - {1}",numbers,totalScore);
    }
}

