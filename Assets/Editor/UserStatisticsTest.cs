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
    public static int PlayDatesNum = 3;

    [SetUp]
    public void CreateUserData()
    {
        const int control = 1;
        var mockDates = new PlayDate[PlayDatesNum];
        for (var playDatesInd = 0; playDatesInd < PlayDatesNum; playDatesInd++)
            mockDates[playDatesInd] = new PlayDate
            {
                SessionLength = SessionLength,
                NumberOfSessions = SessionNum,
                DateObject = DateTime.Today.AddDays(playDatesInd),
                Control = control,
                Code = control == 1 ? "desiree2" : "desiree",
                SessionInterval = SessionInterval,
                Email = "oranshuster@gmail.com"

            };
        var userLocalData = new UserLocalData(mockDates, control == 1 ? "desiree2" : "desiree");
        UserLocalData.Save(userLocalData);
    }

    [Test]
    public void Sanity()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay(DateTime.Today.AddHours(8).AddSeconds(1)));
    }

    [Test]
    public void DayBeforeAfter8Am()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay(DateTime.Today.AddDays(-1).AddHours(8).AddSeconds(1)));
    }

    [Test]
    public void DayBeforePre8Am()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay(DateTime.Today.AddDays(-1).AddHours(7).AddMinutes(30)));
    }

    [Test]
    public void DayAfterPre8AmCanPlay()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        for (var sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
            userStatistics.AddPlayTime(SessionLength, 100,DateTime.Today);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay(DateTime.Today.AddDays(1).AddHours(4)));
    }

    [Test]
    public void DayAfterAfter8AmCanPlay()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        for (var sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
            userStatistics.AddPlayTime(SessionLength, 100,DateTime.Today);
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay(DateTime.Today.AddDays(1).AddHours(8).AddSeconds(1)));
    }

    [Test]
    public void DayAfterPre8Am()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay(DateTime.Today.AddDays(1).AddHours(4)));
    }

    [Test]
    public void DayAfterAfter8Am()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay(DateTime.Today.AddDays(1).AddHours(8).AddSeconds(1)));
    }
    [Test]
    public void AfterAllPlayDatesPre8Am()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(1)));
    }
    [Test]
    public void AfterAllPlayDatesAfter8Am()
    {
        var userStatistics = new UserStatistics();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(-1)));
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