using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public int Column { get; set; }
    public int Row { get; set; }
    public int Value { get; set; }

	private Shape(int value)
    {
    }

    /// <summary>
    /// Checks if the current shape is the requiered delta from another shape
    /// </summary>
    /// <param name="otherShape"></param>
    /// <returns></returns>
	public bool IsPartOfSeries(Shape otherShape,int delta)
    {
        if (!(otherShape is Shape))
            throw new ArgumentException("otherShape");
        if (otherShape == null)
            return false;

		return this.Value - (otherShape as Shape).Value == delta;
    }

    /// <summary>
    /// Constructor alternative
    /// </summary>
    /// <param name="value"></param>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public void Assign(int value, int row, int column)
    {
        Column 	= column;
        Row 	= row;
		Value 	= value;
    }

    /// <summary>
    /// Swaps properties of the two shapes
    /// We could do a shallow copy/exchange here, but anyway...
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static void SwapFields(Shape a, Shape b)
    {
        var temp = a.Row;
        a.Row = b.Row;
        b.Row = temp;

        temp = a.Column;
        a.Column = b.Column;
        b.Column = temp;

	}
		
}



