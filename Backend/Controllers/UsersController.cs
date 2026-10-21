using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/user")]
public class UsersController : ControllerBase
{
    private readonly UserService _users;

    public UsersController(UserService users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserData(CancellationToken cancellationToken)
    {
        var result = await _users.GetCurrentAsync(cancellationToken);
        if (!result.Success || result.Data is null)
        {
            return Ok(new { success = false, message = result.Message });
        }

        return Ok(new
        {
            success = true,
            role = result.Data.Role == UserRole.HotelOwner ? "hotelOwner" : "user",
            recentSearchedCities = result.Data.RecentSearchCities
        });
    }

    [HttpPost("store-recent-search")]
    public async Task<IActionResult> StoreRecentSearch(StoreRecentSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _users.StoreRecentSearchAsync(request.RecentSearchedCity, cancellationToken);
        return Ok(new { success = result.Success, message = result.Message });
    }
}