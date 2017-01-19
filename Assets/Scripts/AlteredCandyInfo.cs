using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class AlteredCandyInfo
{
    private List<GameObject> NewCandy { get; set; }
    public int MaxDistance { get; set; }

    /// <summary>
    /// Returns distinct list of altered candy
    /// </summary>
    public IEnumerable<GameObject> AlteredCandy
    {
        get
        {
            return NewCandy.Distinct();
        }
    }

    public void AddCandy(GameObject go)
    {
        if (!NewCandy.Contains(go))
            NewCandy.Add(go);
    }

    public AlteredCandyInfo()
    {
        NewCandy = new List<GameObject>();
    }
}
