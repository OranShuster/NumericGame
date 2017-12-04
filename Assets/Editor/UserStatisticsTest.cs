using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.IO;
using System.Runtime.InteropServices;
using Random = System.Random;

public class UserStatisticsTest
{
    private const int SessionLength = 20 * 60; //1200
    private const int SessionInterval = 8 * 60 * 60; //28800
    private const int SessionNum = 2;
    private const int PlayDatesNum = 3;
    private static UserInformation _userStatistics;

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
                DateObject = DateTime.Today.AddDays(playDatesInd + 1),
                Control = control,
                Code = control == 1 ? "desiree2" : "desiree",
                SessionInterval = SessionInterval,
                Email = "oranshuster@gmail.com"
            };
        var userLocalData = new UserLocalData(mockDates, control == 1 ? "desiree2" : "desiree");
        UserLocalData.Save(userLocalData);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddHours(8));
    }

    [Test]
    public void Sanity()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void DayBeforeAfter8Am()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(-1).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        var expectedTimeSpan = UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddHours(8));
        Assert.AreEqual(expectedTimeSpan, _userStatistics.TimeToNextSession());
    }

    [Test]
    public void DayBeforePre8Am()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(-1).AddHours(7).AddMinutes(30));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        var expectedTimeSpan = UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddHours(8));
        Assert.AreEqual(expectedTimeSpan, _userStatistics.TimeToNextSession());
    }

    [Test]
    public void DayAfterPre8AmCanPlay()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        for (var sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
            AddPlayTime(SessionLength, 100);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(2).AddHours(4));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        var expectedTimeSpan =
            UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(2).AddHours(8));
        Assert.AreEqual(expectedTimeSpan, _userStatistics.TimeToNextSession());
    }

    [Test]
    public void DayAfterAfter8AmCanPlay()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        for (var sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
        {
            AddPlayTime(SessionLength, 100);
        }
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(2).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void DayAfterPre8Am()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(4));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(2).AddHours(4));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void DayAfterAfter8Am()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(2).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void AfterAllPlayDatesPre8Am()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void AfterAllPlayDatesAfter8Am()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void BannedPlayerAfter8Am()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        _userStatistics.DisablePlayer();
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void BannedPlayerBefore8Am()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        _userStatistics.DisablePlayer();
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(-1));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void NoMoreTimeInDayFirstSession()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(23).AddMinutes(30));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void NoMoreTimeInDayLastSession()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        for (var sessionIndex = 0; sessionIndex < SessionNum - 1; sessionIndex++)
        {
            AddPlayTime(SessionLength, 100);
        }
        AddPlayTime(1, 100);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24)
            .AddSeconds(-1 * (SessionLength - 10)));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void MoreTimeInDayPreciseFirstSession()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        var CanPlayResult = _userStatistics.CanPlay();
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, CanPlayResult.Status, CanPlayResult.Message);
        const int totalPlayTime = SessionNum * SessionLength + (SessionNum - 1) * SessionInterval;
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24).AddSeconds(-1 * totalPlayTime - 5));
        CanPlayResult = _userStatistics.CanPlay();
        Assert.AreEqual(CanPlayStatus.CanPlay, CanPlayResult.Status, CanPlayResult.Message);
    }

    [Test]
    public void MoreTimeInDayPreciseLastSession()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        for (var sessionIndex = 0; sessionIndex < SessionNum - 1; sessionIndex++)
        {
            AddPlayTime(SessionLength, 100);
        }
        AddPlayTime(1, 100);

        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24)
            .AddSeconds(-1 * (SessionLength + 1)));
        Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void NoMoreTimeInDayPreciseFirstSession()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        const int totalPlayTime = SessionNum * SessionLength + (SessionNum - 1) * SessionInterval;
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24).AddSeconds(-1 * totalPlayTime)
            .AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void PlayerFullFlow()
    {
        _userStatistics = new UserInformation();
        var rand = new Random();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        for (var dayIndex = 1; dayIndex <= PlayDatesNum; dayIndex++)
        {
            Debug.Log("DEBUG|1111|=====Starting Day " + dayIndex + "=====");
            GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(rand.Next(0, 7)));
            Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
            var expectedTimeSpan =
                TimeSpan.Parse(UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(dayIndex).AddHours(8))).TotalSeconds;
            var firstSessionGameTime = rand.Next(9, 12);
            Assert.AreEqual(expectedTimeSpan, TimeSpan.Parse(_userStatistics.TimeToNextSession()).TotalSeconds,1);
            GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime));
            Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
            AddPlayTime(SessionLength / 2, 500);
            Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
            AddPlayTime(SessionLength / 2, 500);
            Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
            expectedTimeSpan =
                TimeSpan.Parse(
                    UserInformation.GetTimeSpanToDateTime(GameManager.SystemTime.Now()
                        .AddSeconds(SessionInterval + 1))).TotalSeconds;
            Assert.AreEqual(expectedTimeSpan, TimeSpan.Parse(_userStatistics.TimeToNextSession()).TotalSeconds,1);
            GameManager.SystemTime.AddTimeSpanDelta(new TimeSpan(0, 0, 0, SessionInterval + 1));
            Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
            AddPlayTime(SessionLength, 2500);
            GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime)
                .AddSeconds(SessionInterval * 2));
            if (dayIndex < 3)
            {
                Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
                expectedTimeSpan = TimeSpan.Parse(
                    UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(dayIndex + 1).AddHours(8))).TotalSeconds;
                Assert.AreEqual(expectedTimeSpan, TimeSpan.Parse(_userStatistics.TimeToNextSession()).TotalSeconds,1);
            }
            else
            {
                Assert.AreEqual(CanPlayStatus.GameDone, _userStatistics.CanPlay().Status);
            }
        }
    }

    [Test]
    public void RealPlayerFlow()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(14).AddMinutes(40));
        Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
        AddPlayTime(5, 500);
        Assert.AreEqual(CanPlayStatus.CanPlay, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(15).AddMinutes(3));
        AddPlayTime(1195, 500);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(15).AddMinutes(3)
            .AddSeconds(SessionInterval - 1));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
    }

    [Test]
    public void FirstRemainingTimeCheck()
    {
        _userStatistics = new UserInformation();
        Assert.IsNotNull(_userStatistics.UserLocalData);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddHours(24 - SessionInterval).AddSeconds(-1));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, _userStatistics.CanPlay().Status);
        GameManager.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24)
            .AddSeconds(-1 * SessionInterval - 1));
        Assert.AreEqual(CanPlayStatus.PlayerDisabled, _userStatistics.CanPlay().Status);
    }

    [TearDown]
    public void UserStatisticsTestTeardown()
    {
        if (File.Exists(UserInformation.UserDataPath))
            File.Delete(UserInformation.UserDataPath);
        else
        {
            Debug.LogWarning("DEBUG|2222|Save file was not created");
        }
    }

    private static void AddPlayTime(int length, int score)
    {
        GameManager.GameStartTime = GameManager.SystemTime.Now();
        GameManager.SystemTime.AddTimeSpanDelta(new TimeSpan(0, 0, 0, length));
        _userStatistics.AddPlayTime(length, score);
    }
}