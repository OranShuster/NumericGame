using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NumberCell : MonoBehaviour
{
    public int Column { get; set; }
    public int Row { get; set; }
    public int Value { get; set; }

	public bool IsPartOfSeries(NumberCell other,int delta)
    {
        return Value - other.Value == delta;
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

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        Handles.Label(transform.position, string.Format("({0},{1}",Row,Column));
        Handles.color = Color.black;
        #endif
    }

}



