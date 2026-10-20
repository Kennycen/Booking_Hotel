using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public record CreateRoomRequest(
    string RoomType,
    decimal PricePerNight,
    List<string> Amenities,
    List<string> Images);

public record ToggleRoomRequest(Guid RoomId);

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClerkUserAccessor _users;

    public RoomsController(AppDbContext db, ClerkUserAccessor users)
    {
        _db = db;
        _users = users;
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

    [HttpGet("owner")]
    public async Task<IActionResult> GetOwnerRooms()
    {
        var hotel = await CurrentHotel();
        if (hotel is null)
        {
            return Ok(new { success = false, message = "No hotel found" });
        }

        var rooms = await _db.Rooms
            .Where(room => room.HotelId == hotel.Id)
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
                    hotel.Id,
                    hotel.Name,
                    hotel.Address,
                    hotel.City,
                    hotel.Contact
                }
            })
            .ToListAsync();

        return Ok(new { success = true, rooms });
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom(CreateRoomRequest request)
    {
        var hotel = await CurrentHotel();
        if (hotel is null)
        {
            return Ok(new { success = false, message = "No hotel found" });
        }

        _db.Rooms.Add(new Room
        {
            HotelId = hotel.Id,
            RoomType = request.RoomType,
            PricePerNight = request.PricePerNight,
            Amenities = request.Amenities,
            Images = request.Images
        });

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Room created successfully" });
    }

    [HttpPost("toggle-availability")]
    public async Task<IActionResult> ToggleAvailability(ToggleRoomRequest request)
    {
        var hotel = await CurrentHotel();
        if (hotel is null)
        {
            return Ok(new { success = false, message = "No hotel found" });
        }

        var room = await _db.Rooms.FirstOrDefaultAsync(room =>
            room.Id == request.RoomId && room.HotelId == hotel.Id);

        if (room is null)
        {
            return Ok(new { success = false, message = "Room not found" });
        }

        room.IsAvailable = !room.IsAvailable;
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Room availability Updated" });
    }

    private async Task<Hotel?> CurrentHotel()
    {
        var ownerId = _users.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return null;
        }

        return await _db.Hotels.FirstOrDefaultAsync(hotel => hotel.OwnerId == ownerId);
    }
}