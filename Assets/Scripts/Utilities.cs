﻿using System;
using System.Collections;
using System.Collections.Generic;
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
        if (type == LogType.Exception || type == LogType.Assert)
            OpenGitHubIssue(logString + "\n" + stackTrace);
        if (MinLogLevel >= type)
        {
            var request = UnityWebRequest.Post(Constants.BaseUrl + "/log", logString);
            request.Send();
        }
    }

    public static void OpenGitHubIssue(string exceptionData)
    {
        var title = string.Format("Exception occured! {0}", Guid.NewGuid());
        var body = JsonUtility.ToJson(new GithubIssueRequest(title, exceptionData));
        var request = NewIssueRequest(body);
        if (!ListIssuesRequest(body))
            request.Send();
    }

    public static UnityWebRequest NewIssueRequest(string body)
    {
        var request = new UnityWebRequest(Constants.GitHubIssueBaseUrl);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.uploadHandler.contentType = "application/json";
        request.SetRequestHeader("content-type", "application/json");
        request.SetRequestHeader("authorization", "token 19207a29da1926f4c2bdbbf1598473186544829f");
        return request;
    }

    public static bool ListIssuesRequest(string body)
    {
        var request = UnityWebRequest.Get(Constants.GitHubIssueBaseUrl);
        request.Send();
        while (!request.isDone)
            System.Threading.Thread.Sleep(5000);
        var issuesArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(request.downloadHandler.text);
        foreach (var issue in issuesArray)
            if (issue["body"] == body)
                return true;
        return false;
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

