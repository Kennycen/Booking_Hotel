using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _db;

    public RoomsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _db.Rooms
            .Where(room => room.IsAvailable)
            .OrderByDescending(room => room.CreatedAt)
            .Select(room => new
            {
                room.Id,
                room.RoomType,
                room.PricePerNight,
                room.Amenities,
                room.Images,
                room.IsAvailable,
                Hotel = new
                {
                    room.Hotel.Id,
                    room.Hotel.Name,
                    room.Hotel.Address,
                    room.Hotel.City,
                    room.Hotel.Contact,
                    Owner = new { room.Hotel.Owner.Image }
                }
            })
            .ToListAsync();

        return Ok(new { success = true, rooms });
    }
}