using System.Collections.Concurrent;
using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class PresenceTracker
{
    private readonly ConcurrentDictionary<string, (string Room, string User)> _connections = new();

    public void Add(string connectionId, string room, string user) => _connections[connectionId] =(room, user);
    public string? Remove(string ConnectionId) => _connections.TryRemove(ConnectionId, out var c ) ? c.Room : null;
    public string[] UserIn(string room) => _connections.Values.Where(c => c.Room == room)
    .Select(c => c.User).Distinct().Order().ToArray();
    
}