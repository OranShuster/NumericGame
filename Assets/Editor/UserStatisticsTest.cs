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
    public static int SessionLength = 10 * 60; //600
    public static int SessionInterval = 1 * 60 * 60; //3600
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
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
    }

    [Test]
    public void DayBeforeAfter8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(-1).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        var expectedTimeSpan = UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddHours(8));
        Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
    }

    [Test]
    public void DayBeforePre8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(-1).AddHours(7).AddMinutes(30));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        var expectedTimeSpan = UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddHours(8));
        Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
    }

    [Test]
    public void DayAfterPre8AmCanPlay()
    {
        var userStatistics = new UserInformation();
        var mockDateTime = DateTime.Today.AddDays(1).AddHours(4);
        Assert.IsNotNull(userStatistics.UserLocalData);
        for (var sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
            userStatistics.AddPlayTime(SessionLength, 100);
        UserInformation.SystemTime.SetDateTime(mockDateTime);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        var expectedTimeSpan =
            UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(1).AddHours(8));
        Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
    }

    [Test]
    public void DayAfterAfter8AmCanPlay()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        for (var sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
        {
            userStatistics.AddPlayTime(SessionLength, 100);
        }
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
    }

    [Test]
    public void DayAfterPre8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(4));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }

    [Test]
    public void DayAfterAfter8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }

    [Test]
    public void AfterAllPlayDatesPre8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }

    [Test]
    public void AfterAllPlayDatesAfter8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }

    [Test]
    public void BannedPlayerAfter8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        userStatistics.DisablePlayer();
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }

    [Test]
    public void BannedPlayerBefore8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        userStatistics.DisablePlayer();
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(PlayDatesNum).AddHours(8).AddSeconds(-1));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }
    
    [Test]
    public void NoMoreTimeInDay()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddHours(23).AddMinutes(30));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }
    
    [Test]
    public void MoreTimeInDayPrecise()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        var total_play_time = SessionNum * SessionLength + (SessionNum - 1) * SessionInterval;
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddHours(24).AddSeconds(-1 * total_play_time));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
    }
    
        [Test]
    public void NoMoreTimeInDayPrecise()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        var totalPlayTime = SessionNum * SessionLength + (SessionNum - 1) * SessionInterval;
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddHours(24).AddSeconds(-1 * totalPlayTime).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }

    [Test]
    public void PlayerFullFlow()
    {
        var userStatistics = new UserInformation();
        var rand = new Random();
        Assert.IsNotNull(userStatistics.UserLocalData);
        for (var dayIndex = 0; dayIndex < PlayDatesNum; dayIndex++)
        {
            Debug.Log("DEBUG|1111|=====Starting Day " + dayIndex + "=====");
            UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(rand.Next(0, 7)));
            Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
            var expectedTimeSpan = UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(dayIndex).AddHours(8));
            var firstSessionGameTime = rand.Next(9, 12);
            Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
            UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime));
            Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
            userStatistics.AddPlayTime(SessionLength / 2, 500);
            Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
            userStatistics.AddPlayTime(SessionLength / 2 + 1, 500);
            Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
            expectedTimeSpan =
                UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime)
                    .AddSeconds(SessionInterval));
            Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
            UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime)
                .AddSeconds(SessionInterval));
            Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
            userStatistics.AddPlayTime(SessionLength, 2500);
            Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
            expectedTimeSpan = UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(dayIndex)
                .AddHours(firstSessionGameTime).AddSeconds(SessionInterval * 2));
            Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
            UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime)
                .AddSeconds(SessionInterval * 2));
            userStatistics.AddPlayTime(SessionLength / 3, 2500);
            Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
            userStatistics.AddPlayTime(SessionLength / 3, 2500);
            Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
            userStatistics.AddPlayTime(SessionLength / 3, 2500);
            if (dayIndex < 2)
            {
                Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
                expectedTimeSpan =
                    UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(dayIndex + 1).AddHours(8));
                Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
            }
            else
            {
                Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
            }
        }
    }

    [TearDown]
    public void UserStatisticsTestTeardown()
    {
        UserInformation.SystemTime.ResetDateTime();
        if (File.Exists(UserInformation.UserDataPath))
            File.Delete(UserInformation.UserDataPath);
        else
        {
            Debug.LogWarning("DEBUG|2222|Save file was not created");
        }
    }
}