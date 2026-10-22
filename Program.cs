using Microsoft.EntityFrameworkCore;
using ChatApp.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var cs = builder.Configuration.GetConnectionString("Chat") ?? "Data Source=chat.db";
builder.Services.AddDbContext<ChatDbContext>(o => o.UseSqlite(cs));builder.Services.AddSingleton<PresenceTracker>();

var app = builder.Build();using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ChatDbContext>().Database.Migrate();
}

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