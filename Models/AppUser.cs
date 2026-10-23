// Models/AppUser.cs
public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Models/AuthDtos.cs
public record AuthRequest(string? Username, string? Password);
public record AuthResponse(string Token, string Username);