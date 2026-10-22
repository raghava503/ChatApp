using Microsoft.EntityFrameworkCore;
using ChatApp.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddDbContext<ChatDbContext>(o => o.UseSqlite("Data Source=chat.db"));
builder.Services.AddSingleton<PresenceTracker>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chat");

app.MapGet("/api/rooms/{room}/messages", async (string room, ChatDbContext db) =>
    await db.Messages.AsNoTracking()
        .Where(m => m.Room == room)
        .OrderByDescending(m => m.SentAt).Take(50)
        .OrderBy(m => m.SentAt)
        .ToListAsync());

app.Run();