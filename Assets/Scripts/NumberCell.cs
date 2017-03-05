using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class NumberCell : MonoBehaviour
{
    public int Column { get; set; }
    public int Row { get; set; }
    public int Value { get; set; }

	public bool IsPartOfSeries(NumberCell other,int delta)
    {
        if (!(other is NumberCell))
            throw new ArgumentException("otherShape");
        if (other == null)
            return false;

		return this.Value - (other as NumberCell).Value == delta;
    }

    public void Assign(int value, int row, int column)
    {
        Column 	= column;
        Row 	= row;
		Value 	= value;
    }

    public static void SwapFields(NumberCell a, NumberCell b)
    {
        var temp = a.Row;
        a.Row = b.Row;
        b.Row = temp;

        temp = a.Column;
        a.Column = b.Column;
        b.Column = temp;

	}
    void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        Handles.Label(transform.position, String.Format("({0},{1}",Row,Column));
        Handles.color = Color.black;
        #endif
    }

}



