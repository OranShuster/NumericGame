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
    public static int SessionLength = 20 * 60; //1200
    public static int SessionInterval = 8 * 60 * 60; //28800
    public static int SessionNum = 2;
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
                DateObject = DateTime.Today.AddDays(playDatesInd+1),
                Control = control,
                Code = control == 1 ? "desiree2" : "desiree",
                SessionInterval = SessionInterval,
                Email = "oranshuster@gmail.com"
            };
        var userLocalData = new UserLocalData(mockDates, control == 1 ? "desiree2" : "desiree");
        UserLocalData.Save(userLocalData);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddHours(8));
    }

    [Test]
    public void Sanity()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(8).AddSeconds(1));
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
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        for (var sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
            userStatistics.AddPlayTime(SessionLength, 100);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(2).AddHours(4));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        var expectedTimeSpan =
            UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(2).AddHours(8));
        Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
    }

    [Test]
    public void DayAfterAfter8AmCanPlay()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        for (var sessionIndex = 0; sessionIndex < SessionNum; sessionIndex++)
        {
            userStatistics.AddPlayTime(SessionLength, 100);
        }
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(2).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
    }

    [Test]
    public void DayAfterPre8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(4));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(2).AddHours(4));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }

    [Test]
    public void DayAfterAfter8Am()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(8).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(2).AddHours(8).AddSeconds(1));
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
    public void NoMoreTimeInDayFirstSession()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(23).AddMinutes(30));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }
    
    [Test]
    public void NoMoreTimeInDayLastSession()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        for (var sessionIndex = 0; sessionIndex < SessionNum - 1; sessionIndex++)
        {
            userStatistics.AddPlayTime(SessionLength, 100);
        }        
        userStatistics.AddPlayTime(1,100);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24).AddSeconds(-1 * (SessionLength - 10)));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }
    
    [Test]
    public void MoreTimeInDayPreciseFirstSession()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        var total_play_time = SessionNum * SessionLength + (SessionNum - 1) * SessionInterval;
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24).AddSeconds(-1 * total_play_time));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
    }
    
    [Test]
    public void MoreTimeInDayPreciseLastSession()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(9));
        for (var sessionIndex = 0; sessionIndex < SessionNum - 1; sessionIndex++)
        {
            userStatistics.AddPlayTime(SessionLength, 100);
        }
        userStatistics.AddPlayTime(1,100);
        
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24).AddSeconds(-1 * (SessionLength + 1)));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
    }
    
    [Test]
    public void NoMoreTimeInDayPreciseFirstSession()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        var totalPlayTime = SessionNum * SessionLength + (SessionNum - 1) * SessionInterval;
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(24).AddSeconds(-1 * totalPlayTime).AddSeconds(1));
        Assert.AreEqual(CanPlayStatus.NoMoreTimeSlots, userStatistics.CanPlay());
    }

    [Test]
    public void PlayerFullFlow()
    {
        var userStatistics = new UserInformation();
        var rand = new Random();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        for (var dayIndex = 1; dayIndex <= PlayDatesNum; dayIndex++)
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
            userStatistics.AddPlayTime(SessionLength / 2, 500);
            Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
            expectedTimeSpan =
                UserInformation.GetTimeSpanToDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime)
                    .AddSeconds(SessionInterval));
            Assert.AreEqual(expectedTimeSpan, userStatistics.TimeToNextSession());
            UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime)
                .AddSeconds(SessionInterval));
            Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
            userStatistics.AddPlayTime(SessionLength, 2500);
            UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(dayIndex).AddHours(firstSessionGameTime)
                .AddSeconds(SessionInterval * 2));
            if (dayIndex < 3)
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

    [Test]
    public void RealPlayerFlow()
    {
        var userStatistics = new UserInformation();
        Assert.IsNotNull(userStatistics.UserLocalData);
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(14).AddMinutes(40));
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
        userStatistics.AddPlayTime(5, 500);
        Assert.AreEqual(CanPlayStatus.CanPlay, userStatistics.CanPlay());
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(15).AddMinutes(3));
        userStatistics.AddPlayTime(1195, 500);
        UserInformation.SystemTime.SetDateTime(DateTime.Today.AddDays(1).AddHours(15).AddMinutes(3).AddSeconds(SessionInterval-1));
        Assert.AreEqual(CanPlayStatus.HasNextTimeslot, userStatistics.CanPlay());
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