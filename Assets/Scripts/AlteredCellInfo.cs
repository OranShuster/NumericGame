using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NumericalGame
{
    public class AlteredCellInfo
    {
        private List<GameObject> NewCell { get; set; }
        public int MaxDistance { get; set; }

        public IEnumerable<GameObject> AlteredCell
        {
            get { return NewCell.Distinct(); }
        }

        public void AddCell(GameObject go)
        {
            if (!NewCell.Contains(go))
                NewCell.Add(go);
        }

        public AlteredCellInfo()
        {
            NewCell = new List<GameObject>();
        }
    }
}