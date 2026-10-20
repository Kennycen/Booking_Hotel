using Microsoft.AspNetCore.Mvc;

public record StoreRecentSearchRequest(string RecentSearchedCity);

[ApiController]
[Route("api/user")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClerkUserAccessor _users;

    public UsersController(AppDbContext db, ClerkUserAccessor users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserData()
    {
        var user = await _users.GetOrCreateAsync();
        if (user is null)
        {
            return Ok(new { success = false, message = "Not authenticated." });
        }

        return Ok(new 
        {
            success = true,
            role = user.Role == UserRole.HotelOwner ? "hotelOwner" : "user",
            recentSearchedCities = user.RecentSearchCities
        });
    }

    [HttpPost("store-recent-search")]
    public async Task<IActionResult> StoreRecentSearch(StoreRecentSearchRequest request)
    {
        var user = await _users.GetOrCreateAsync();
        if (user is null)
        {
            return Ok(new { success = false, message = "Not authenticated." });
        }

        var cities = user.RecentSearchCities.ToList();
        if (cities.Count >= 3)
        {
            cities.RemoveAt(0);
        }

        cities.Add(request.RecentSearchedCity);
        user.RecentSearchCities = cities;
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "City added" });
    }

}