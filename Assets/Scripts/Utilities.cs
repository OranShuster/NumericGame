using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public static class Utilities
{
    public static bool AreNeighbors(NumberCell s1, NumberCell s2)
    {
        return (s1.Column == s2.Column ||
                        s1.Row == s2.Row)
                        && Mathf.Abs(s1.Column - s2.Column) <= 1
                        && Mathf.Abs(s1.Row - s2.Row) <= 1;
    }

    public static string LoadStringFromFile(string key, int lineLength=15)
    {
        INIParser ini = new INIParser();
        TextAsset asset = Resources.Load("StringsFile") as TextAsset;
        ini.Open(asset);
        return Flipfont.ReverseText(ini.ReadValue("Translation", key, key), lineLength);
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
        mockDates[0] = new PlayDate() {session_length = 10000,sessions = 3,date = DateTime.Today.ToString(Constants.DateFormat)};
        mockDates[0].GameRounds.Add(new Rounds(20, 55, "18:30:00"));
        mockDates[1] = new PlayDate() { session_length = 10000, sessions = 3, date = DateTime.Today.AddDays(1).ToString(Constants.DateFormat) };
        mockDates[1].GameRounds.Add(new Rounds(40, 55, "22:12:00"));
        mockDates[1].GameRounds.Add(new Rounds(10, 200,"09:05:55"));
        mockDates[1].GameRounds.Add(new Rounds(20, 200, "10:05:55"));
        mockDates[1].GameRounds.Add(new Rounds(30, 200, "11:05:55"));
        mockDates[1].GameRounds.Add(new Rounds(40, 200, "12:05:55"));
        mockDates[1].GameRounds.Add(new Rounds(50, 200, "13:05:55"));
        mockDates[1].GameRounds.Add(new Rounds(60, 200, "14:05:55"));
        mockDates[1].GameRounds.Add(new Rounds(70, 200, "15:05:55"));
        mockDates[1].GameRounds.Add(new Rounds(80, 200, "16:05:55"));
        mockDates[1].GameRounds.Add(new Rounds(90, 200, "17:05:55"));
        mockDates[2] = new PlayDate() { session_length = 10000, sessions = 3, date = DateTime.Today.AddDays(2).ToString(Constants.DateFormat) };
        var userLocalData = new UserLocalData(mockDates, "desiree");
        UserStatistics.Save(userLocalData);
    }
    public static bool IsTestCode(string usercode)
    {
        return usercode == Constants.TestCode;
    }
    public static void SendLogsToServer(string logString, string stackTrace, LogType type)
    {
        var MinLogLevel = LogType.Log;
        if (MinLogLevel >= type)
        {
            var request = UnityWebRequest.Post(Constants.BaseUrl + "/log", logString);
            request.Send();
        }
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
    public static Color HexToRgb(string hex)
    {
        var bigint = Convert.ToUInt32(hex, 16);
        var r = (bigint >> 16) & 255;
        var g = (bigint >> 8) & 255;
        var b = bigint & 255;

        return new Color(r,g,b);
    }
}

