using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        return ReverseText(ini.ReadValue("Translation", key, key), lineLength);
    }
    public static string PrintArray(int[] arr)
    {
        return String.Join(",", Array.ConvertAll<int, String>(arr, i => i.ToString()));
    }
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        var ret = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        return Mathf.Clamp(ret,from2,to2);
    }

    public static void CreateMockUserData(int control)
    {
        var mockDates = new PlayDate[3];
        mockDates[0] = new PlayDate() { SessionLength = 10000, NumberOfSessions = 3, Date = DateTime.Today.ToString(Constants.DateFormat), Control = control ,Code= control==1? "desiree2" : "desiree"};
        mockDates[1] = new PlayDate() { SessionLength = 10000, NumberOfSessions = 3, Date = DateTime.Today.AddDays(1).ToString(Constants.DateFormat), Control = control, Code = control == 1 ? "desiree2" : "desiree" };
        mockDates[2] = new PlayDate() { SessionLength = 10000, NumberOfSessions = 3, Date = DateTime.Today.AddDays(2).ToString(Constants.DateFormat), Control = control, Code = control == 1 ? "desiree2" : "desiree" };
        var userLocalData = new UserLocalData(mockDates, "desiree");
        UserStatistics.Save(userLocalData);
    }
    public static int IsTestCode(string usercode)
    {
        if (usercode == Constants.TestCode)
            return 0;
        if (usercode == Constants.TestCode2)
            return 1;
        return -1;
    }

    public static void LoggerCallback(string logString, string stackTrace, LogType type)
    {
        var MinLogLevel = LogType.Log;
        if (MinLogLevel >= type)
        {
            var request = UnityWebRequest.Post(Constants.BaseUrl + "/log", logString);
            request.Send();
        }
    }

    public static string ReverseText(string str, int lineLength=15)
    {
        string individualLine = ""; //Control individual line in the multi-line text component.
        var reversedString = "";
        var listofWords = str.Split(' ').ToList(); //Extract words from the sentence

        foreach (var s in listofWords)
        {
            if (individualLine.Length >= lineLength)
            {
                reversedString += ReverseLine(individualLine) + "\n"; //Add a new line feed at the end, since we cannot accomodate more characters here.
                individualLine = ""; //Reset this string for new line.
            }
            individualLine += s + " ";
        }
        individualLine = individualLine.Substring(0, individualLine.Length - 1);
        if (individualLine != "")
            reversedString += ReverseLine(individualLine);
        return reversedString;
    }

    private static string ReverseLine(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}

public class GithubIssueRequest
{
    public string title;
    public string body;
    public string assignee = "OranShuster";

    public GithubIssueRequest(string title, string body)
    {
        this.title = title;
        this.body = body;
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

