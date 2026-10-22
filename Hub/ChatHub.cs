// Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using ChatApp.Data;
using ChatApp.Models;
using System.Threading.Tasks;
using System;

public class ChatHub(ChatDbContext db, PresenceTracker presence) : Hub
{
    public async Task JoinRoom(string room, string user)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        presence.Add(Context.ConnectionId, room, user);
        await Clients.Group(room).SendAsync("PresenceChanged", presence.UserIn(room));
    }

    public async Task SendMessage(string room, string user, string text)
    {
        text = text.Trim();
        if (text.Length == 0 || text.Length > 500) return;

        var msg = new Message { Room = room, User = user, Text = text };
        db.Messages.Add(msg);
        await db.SaveChangesAsync();

        await Clients.Group(room).SendAsync("ReceiveMessage", msg);
    }

    public Task Typing(string room, string user) =>
        Clients.OthersInGroup(room).SendAsync("UserTyping", user);

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var room = presence.Remove(Context.ConnectionId);
        if (room is not null)
            await Clients.Group(room).SendAsync("PresenceChanged", presence.UserIn(room));
        await base.OnDisconnectedAsync(ex);
    }
}