// Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using ChatApp.Data;
using ChatApp.Models;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class ChatHub(ChatDbContext db, PresenceTracker presence) : Hub
{
    private string UserName =>
        Context.User?.Identity?.Name ?? throw new HubException("Not signed in.");

    public async Task JoinRoom(string room)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        presence.Add(Context.ConnectionId, room, UserName);
        await Clients.Group(room).SendAsync("PresenceChanged", presence.UserIn(room));
    }

    public async Task SendMessage(string room, string text)
    {
        text = text.Trim();
        if (text.Length == 0 || text.Length > 500) return;

        var msg = new Message { Room = room, User = UserName, Text = text };
        db.Messages.Add(msg);
        await db.SaveChangesAsync();
        await Clients.Group(room).SendAsync("ReceiveMessage", msg);
    }

    public Task Typing(string room) =>
        Clients.OthersInGroup(room).SendAsync("UserTyping", UserName);

    // OnDisconnectedAsync stays exactly as it was

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var room = presence.Remove(Context.ConnectionId);
        if (room is not null)
            await Clients.Group(room).SendAsync("PresenceChanged", presence.UserIn(room));
        await base.OnDisconnectedAsync(ex);
    }
}