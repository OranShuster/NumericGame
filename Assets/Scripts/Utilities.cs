using System;
using UnityEngine;

public static class Utilities
{
    public static bool AreNeighbors(NumberCell s1, NumberCell s2)
    {
        return (s1.Column == s2.Column ||
                        s1.Row == s2.Row)
                        && Mathf.Abs(s1.Column - s2.Column) <= 1
                        && Mathf.Abs(s1.Row - s2.Row) <= 1;
    }
    public static string PrintArray(int[] arr)
    {
        return string.Join(",", Array.ConvertAll<int, String>(arr, i => i.ToString()));
    }
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        var ret = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        return Mathf.Clamp(ret,from2,to2);
    }

    public static void CreateMockUserData()
    {
        var mockDates = new PlayDate[3];
        mockDates[0] =new PlayDate() {session_length = 10000,sessions = 3,date = DateTime.Today.ToString(Constants.DateFormat)};
        mockDates[0].GameRuns.Add(new Runs(20, 55, "18:30:00"));
        mockDates[1] = new PlayDate() { session_length = 10000, sessions = 3, date = DateTime.Today.AddDays(1).ToString(Constants.DateFormat) };
        mockDates[1].GameRuns.Add(new Runs(40, 55, "22:12:00"));
        mockDates[1].GameRuns.Add(new Runs(150, 200,"09:05:55"));
        mockDates[2] = new PlayDate() { session_length = 10000, sessions = 3, date = DateTime.Today.AddDays(2).ToString(Constants.DateFormat) };
        var userInfo = new UserInformation() { UserLocalData =  new UserLocalData(){PlayDates = mockDates,UserCode = "TESTTEST"}};
        userInfo.Save();
    }
}

public static class DebugUtilities
{
    public static void DebugPositions(GameObject hitGo, GameObject hitGo2)
    {
        var lala =
                        hitGo.GetComponent<NumberCell>().Row + "-"
                        + hitGo.GetComponent<NumberCell>().Column + "-"
                         + hitGo2.GetComponent<NumberCell>().Row + "-"
                         + hitGo2.GetComponent<NumberCell>().Column;
        Debug.Log(lala);

    }

    public static void ShowArray(ShapesMatrix shapes, int size)
    {

        Debug.Log(GetArrayContents(shapes, size));
    }

    public static string GetArrayContents(ShapesMatrix shapes, int size)
    {
        var x = string.Empty;
        for (var row = 0; row < size; row++)
        {

            for (var column = 0; column < size; column++)
            {
                if (shapes[row, column] == null)
                    x += "NULL  |";
                else
                {
                    var shape = shapes[row, column].GetComponent<NumberCell>();
                    x += shape.Value.ToString("D2") + " | ";
                }
            }
            x += Environment.NewLine;
        }
        return x;
    }
}

