using System.Security.Claims;

public interface ICurrentUser
{
    string? UserId { get; }
}

public sealed class CurrentUser : ICurrentUser
{
    public CurrentUser(IHttpContextAccessor http)
    {
        var principal = http.HttpContext?.User;
        UserId = principal?.FindFirstValue("sub")
            ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public string? UserId { get; }
}