using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var cs = builder.Configuration.GetConnectionString("Chat") ?? "Data Source=chat.db";
builder.Services.AddDbContext<ChatDbContext>(o => o.UseSqlite(cs));builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddSingleton<TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = "name"
        };
        // SignalR WebSockets can't send headers, so the token arrives in the query string
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/chat"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ChatDbContext>().Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chat");
app.MapPost("/api/auth/register", async (AuthRequest req, ChatDbContext db, TokenService tokens) =>
{
    var name = (req.Username ?? "").Trim();
    var password = req.Password ?? "";
    if (!Regex.IsMatch(name, "^[A-Za-z0-9_-]{3,20}$"))
        return Results.BadRequest(new { error = "Username must be 3 to 20 letters, numbers, - or _." });
    if (password.Length < 8 || password.Length > 72)
        return Results.BadRequest(new { error = "Password must be 8 to 72 characters." });
    if (await db.Users.AnyAsync(u => u.Username == name))
        return Results.BadRequest(new { error = "That username is taken." });

    var user = new AppUser { Username = name, PasswordHash = BCrypt.Net.BCrypt.HashPassword(password) };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new AuthResponse(tokens.Create(user), user.Username));
});

app.MapPost("/api/auth/login", async (AuthRequest req, ChatDbContext db, TokenService tokens) =>
{
    var name = (req.Username ?? "").Trim();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == name);
    if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password ?? "", user.PasswordHash))
        return Results.Json(new { error = "Wrong username or password." }, statusCode: 401);
    return Results.Ok(new AuthResponse(tokens.Create(user), user.Username));
});

app.MapGet("/api/rooms/{room}/messages", async (string room, ChatDbContext db) =>
    await db.Messages.AsNoTracking()
        .Where(m => m.Room == room)
        .OrderByDescending(m => m.SentAt).Take(50)
        .OrderBy(m => m.SentAt)
        .ToListAsync());

app.Run();