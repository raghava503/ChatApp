using Microsoft.AspNetCore.SignalR;
using ChatApp.Data;
using ChatApp.Models;
using System.Threading.Tasks;
using System;

namespace ChatApp.Tests;

public class PresenceTrackerTests
{
    [Fact]
    public void Add_MakesUserAppearInRoom()
    {
        var tracker = new PresenceTracker();

        tracker.Add("conn1", "general", "raghava");

        Assert.Equal(new[] { "raghava" }, tracker.UserIn("general"));
    }

    [Fact]
    public void SameUserOnTwoConnections_AppearsOnce()
    {
        var tracker = new PresenceTracker();

        tracker.Add("conn1", "general", "raghava");
        tracker.Add("conn2", "general", "raghava");

        Assert.Single(tracker.UserIn("general"));
    }

    [Fact]
    public void Remove_ReturnsRoomAndRemovesUser()
    {
        var tracker = new PresenceTracker();
        tracker.Add("conn1", "general", "raghava");

        var room = tracker.Remove("conn1");

        Assert.Equal("general", room);
        Assert.Empty(tracker.UserIn("general"));
    }

    [Fact]
    public void Remove_UnknownConnection_ReturnsNull()
    {
        var tracker = new PresenceTracker();

        Assert.Null(tracker.Remove("does-not-exist"));
    }

    [Fact]
    public void Remove_OneOfTwoConnections_UserStaysOnline()
    {
        var tracker = new PresenceTracker();
        tracker.Add("conn1", "general", "raghava");
        tracker.Add("conn2", "general", "raghava");

        tracker.Remove("conn1");

        Assert.Equal(new[] { "raghava" }, tracker.UserIn("general"));
    }

    [Fact]
    public void Rooms_AreIsolatedFromEachOther()
    {
        var tracker = new PresenceTracker();
        tracker.Add("conn1", "general", "raghava");
        tracker.Add("conn2", "random", "sam");

        Assert.Equal(new[] { "raghava" }, tracker.UserIn("general"));
        Assert.Equal(new[] { "sam" }, tracker.UserIn("random"));
    }

    [Fact]
    public void UsersIn_ReturnsNamesSorted()
    {
        var tracker = new PresenceTracker();
        tracker.Add("c1", "general", "zoe");
        tracker.Add("c2", "general", "adam");

        Assert.Equal(new[] { "adam", "zoe" }, tracker.UserIn("general"));
    }
}