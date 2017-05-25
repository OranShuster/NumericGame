using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
using Object = UnityEngine.Object;

public class UserStatisticsTest
{

    public static int SessionLength = 10 * 60;
    public static int SessionInterval = 1 * 60 * 60;
    public static int SessionNum = 3;

    [SetUp]
    public void CreateUserData()
    {
        const int control = 1;
        var mockDates = new PlayDate[3];
        mockDates[0] = new PlayDate
        {
            SessionLength = SessionLength,
            NumberOfSessions = SessionNum,
            DateObject = DateTime.Today,
            Control = control,
            Code = control == 1 ? "desiree2" : "desiree",
            SessionInterval = SessionInterval
        };
        mockDates[1] = new PlayDate
        {
            SessionLength = SessionLength,
            NumberOfSessions = SessionNum,
            DateObject = DateTime.Today.AddDays(1),
            Control = control,
            Code = control == 1 ? "desiree2" : "desiree",
            SessionInterval = SessionInterval
        };
        mockDates[2] = new PlayDate
        {
            SessionLength = SessionLength,
            NumberOfSessions = SessionNum,
            DateObject = DateTime.Today.AddDays(2),
            Control = control,
            Code = control == 1 ? "desiree2" : "desiree",
            SessionInterval = SessionInterval
        };
        var userLocalData = new UserLocalData(mockDates, control == 1 ? "desiree2" : "desiree");
        UserLocalData.Save(userLocalData);
    }

    [Test]
    public void Sanity()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.Equals(userStatistics.CanPlay(DateTime.Today.AddHours(9)), CanPlayStatus.CanPlay);
    }

    [Test]
    public void DayBefore()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.Equals(userStatistics.CanPlay(DateTime.Today.AddDays(-1).AddHours(9)), CanPlayStatus.HasNextTimeslot);
    }

    [Test]
    public void DayBeforePre8Am()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.Equals(userStatistics.CanPlay(DateTime.Today.AddDays(-1).AddHours(4)), CanPlayStatus.HasNextTimeslot);
    }

    [Test]
    public void DayAfterPre8AmCanPlay()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        for (int sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
            userStatistics.AddPlayTime(SessionLength, 100);
        Assert.Equals(userStatistics.CanPlay(DateTime.Today.AddDays(1).AddHours(4)), CanPlayStatus.HasNextTimeslot);
    }

    [Test]
    public void DayAfterAfter8AmCanPlay()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        for (int sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
            userStatistics.AddPlayTime(SessionLength, 100);
        Assert.Equals(userStatistics.CanPlay(DateTime.Today.AddDays(1).AddHours(8)), CanPlayStatus.HasNextTimeslot);
    }

    [Test]
    public void DayAfterPre8Am()
    {

    }

    [Test]
    public void DayAfterAfter8Am()
    {

    }

    [TearDown]
    public void DeleteUserData()
    {
        if (File.Exists(UserStatistics.UserDataPath))
            File.Delete(UserStatistics.UserDataPath);
        else
        {
            Debug.LogWarning("Save file was not created");
        }
    }
}