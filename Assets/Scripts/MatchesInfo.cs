using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MatchesInfo
{
    private List<GameObject> _matchedCandies;

    /// <summary>
    /// Returns distinct list of matched candy
    /// </summary>
    public IEnumerable<GameObject> MatchedCandy
    {
        get
        {
            return _matchedCandies.Distinct();
        }
    }

    public void AddObject(GameObject go)
    {
        if (!_matchedCandies.Contains(go))
            _matchedCandies.Add(go);
    }

    public void AddObjectRange(IEnumerable<GameObject> gos)
    {
        foreach (var item in gos)
        {
            AddObject(item);
        }
    }

    public MatchesInfo()
    {
        _matchedCandies = new List<GameObject>();
    }
}

