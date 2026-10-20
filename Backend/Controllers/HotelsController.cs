using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public record RegisterHotelRequest(string Name, string Address, string Contact, string City);

[ApiController]
[Route("api/hotels")]
public class HotelsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClerkUserAccessor _users;

    public HotelsController(AppDbContext db, ClerkUserAccessor users)
    {
        _db = db;
        _users = users;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterHotel(RegisterHotelRequest request)
    {
        var ownerId = _users.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Ok(new { success = false, message = "Not authenticated." });
        }

        var user = await _db.Users.FirstOrDefaultAsync(user => user.Id == ownerId);
        if (user is null)
        {
            return Ok(new { success = false, message = "User not found." });
        }

        var existingHotel = await _db.Hotels.AnyAsync(hotel => hotel.OwnerId == ownerId);
        if (existingHotel)
        {
            return Ok(new { success = false, message = "Hotel already registered." });
        }

        _db.Hotels.Add(new Hotel
        {
            Name = request.Name,
            Address = request.Address,
            Contact = request.Contact,
            City = request.City,
            OwnerId = ownerId
        });

        user.Role = UserRole.HotelOwner;
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Hotel Registered Successfully." });
    }
}