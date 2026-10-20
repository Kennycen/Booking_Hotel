using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public class ClerkUserAccessor
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public ClerkUserAccessor(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public string? UserId
    {
        get
        {
            var principal = _http.HttpContext?.User;
            if (principal is null)
            {
                return null;
            }

            return principal.FindFirstValue("sub")
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

    public async Task<User?> GetOrCreateAsync()
    {
        var userId = UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var existing = await _db.Users.FirstOrDefaultAsync(user => user.Id == userId);
        if (existing is not null)
        {
            return existing;
        }

        var principal = _http.HttpContext!.User;
        var email = principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue("email");
        var username = principal.FindFirstValue("username")
            ?? principal.FindFirstValue("name")
            ?? "Guest";
        var image = principal.FindFirstValue("image") ?? "";

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var created = new User
        {
            Id = userId,
            Email = email,
            Username = username,
            Image = image
        };

        _db.Users.Add(created);
        await _db.SaveChangesAsync();
        return created;
    }
}
