using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


public static class DebugUtilities
{

    public static string[,] FillShapesArrayFromResourcesData()
    {
		var shapes = new string[ShapesManager.Rows,ShapesManager.Columns];

        var txt = Resources.Load("level") as TextAsset;
        var level = txt.text;

        var lines = level.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		for (var row = ShapesManager.Rows - 1; row >= 0; row--)
        {
            var items = lines[row].Split('|');
			for (var column = 0; column < ShapesManager.Columns; column++)
            {
                shapes[row, column] = items[column];
            }
        }
        return shapes;

    }

    public static void DebugRotate(GameObject go)
    {
        go.transform.Rotate(0, 0, 80f);
    }

    public static void DebugAlpha(GameObject go)
    {
        var c = go.GetComponent<SpriteRenderer>().color;
        c.a = 0.6f;
        go.GetComponent<SpriteRenderer>().color = c;
    }

    public static void DebugPositions(GameObject hitGo, GameObject hitGo2)
    {
        var lala =
                        hitGo.GetComponent<Shape>().Row + "-"
                        + hitGo.GetComponent<Shape>().Column + "-"
                         + hitGo2.GetComponent<Shape>().Row + "-"
                         + hitGo2.GetComponent<Shape>().Column;
        Debug.Log(lala);

    }

    public static void ShowArray(ShapesArray shapes)
    {

        Debug.Log(GetArrayContents(shapes));
    }

    public static string GetArrayContents(ShapesArray shapes)
    {
        var x = string.Empty;
		for (var row = ShapesManager.Rows - 1; row >= 0; row--)
        {

			for (var column = 0; column < ShapesManager.Columns; column++)
            {
                if (shapes[row, column] == null)
                    x += "NULL  |";
                else
                {
                    var shape = shapes[row, column].GetComponent<Shape>();
                    x += shape.Row.ToString("D2")
                        + "-" + shape.Column.ToString("D2")
                        + "-" + shape.Value.ToString("D2")
                        + " | ";
                }
            }
            x += Environment.NewLine;
        }
        return x;
    }
}

