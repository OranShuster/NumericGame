using System;
using UnityEngine;

public static class Utilities
{
    /// <summary>
    /// Checks if a shape is next to another one
    /// either horizontally or vertically
    /// </summary>
    /// <param name="s1"></param>
    /// <param name="s2"></param>
    /// <returns></returns>
    public static bool AreNeighbors(Shape s1, Shape s2)
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
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static void CreateMockUserData()
    {
        PlayDate[] mockDates = new PlayDate[3];
        mockDates[0] =new PlayDate() {SessionPlayTime = 10,NumOfSessions = 3,SessionDate = DateTime.Today.ToString(Constants.DateFormat)};
        mockDates[1] = new PlayDate() { SessionPlayTime = 10, NumOfSessions = 3, SessionDate = DateTime.Today.AddDays(1).ToString(Constants.DateFormat) };
        mockDates[2] = new PlayDate() { SessionPlayTime = 10, NumOfSessions = 3, SessionDate = DateTime.Today.AddDays(2).ToString(Constants.DateFormat) };
        UserInformation userInfo = new UserInformation {UserPlayDates = mockDates};
        userInfo.Save();
    }
}

