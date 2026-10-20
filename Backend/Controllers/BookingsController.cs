using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public record CheckAvailabilityRequest(Guid Room, DateTime CheckInDate, DateTime CheckOutDate);

public record CreateBookingRequest(Guid Room, DateTime CheckInDate, DateTime CheckOutDate, int Guests);

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClerkUserAccessor _users;

    public BookingsController(AppDbContext db, ClerkUserAccessor users)
    {
        _db = db;
        _users = users;
    }

    [HttpPost("check-availability")]
    public async Task<IActionResult> CheckAvailability(CheckAvailabilityRequest request)
    {
        var available = await IsAvailable(request.Room, request.CheckInDate, request.CheckOutDate);
        return Ok(new { success = true, isAvailable = available });
    }

    [HttpPost("book")]
    public async Task<IActionResult> CreateBooking(CreateBookingRequest request)
    {
        var userId = _users.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Ok(new { success = false, message = "Not authenticated." });
        }

        var userExists = await _db.Users.AnyAsync(user => user.Id == userId);
        if (!userExists)
        {
            return Ok(new { success = false, message = "User not found." });
        }

        if (!await IsAvailable(request.Room, request.CheckInDate, request.CheckOutDate))
        {
            return Ok(new { success = false, message = "Room is not available" });
        }

        var room = await _db.Rooms
            .Include(room => room.Hotel)
            .FirstOrDefaultAsync(room => room.Id == request.Room);

        if (room is null)
        {
            return Ok(new { success = false, message = "Room is not available" });
        }

        var checkIn = AsUtc(request.CheckInDate);
        var checkOut = AsUtc(request.CheckOutDate);
        var nights = (int)Math.Ceiling((checkOut - checkIn).TotalDays);
        if (nights < 1)
        {
            return Ok(new { success = false, message = "Room is not available" });
        }

        _db.Bookings.Add(new Booking
        {
            UserId = userId,
            RoomId = room.Id,
            HotelId = room.HotelId,
            Guests = request.Guests,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalPrice = room.PricePerNight * nights
        });

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Booking created successfully" });
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUserBookings()
    {
        var userId = _users.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Ok(new { success = false, message = "Not authenticated." });
        }

        var bookings = await _db.Bookings
            .Where(booking => booking.UserId == userId)
            .OrderByDescending(booking => booking.CreatedAt)
            .Select(booking => new
            {
                booking.Id,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.TotalPrice,
                booking.Guests,
                booking.IsPaid,
                booking.PaymentMethod,
                Status = booking.Status.ToString().ToLower(),
                Room = new
                {
                    booking.Room.Id,
                    booking.Room.RoomType,
                    booking.Room.PricePerNight,
                    booking.Room.Images
                },
                Hotel = new
                {
                    booking.Hotel.Id,
                    booking.Hotel.Name,
                    booking.Hotel.Address,
                    booking.Hotel.City
                }
            })
            .ToListAsync();

        return Ok(new { success = true, bookings });
    }

    [HttpGet("hotel")]
    public async Task<IActionResult> GetHotelBookings()
    {
        var ownerId = _users.UserId;
        var hotel = string.IsNullOrWhiteSpace(ownerId)
            ? null
            : await _db.Hotels.FirstOrDefaultAsync(hotel => hotel.OwnerId == ownerId);

        if (hotel is null)
        {
            return Ok(new { success = false, message = "No Hotel found" });
        }

        var bookings = await _db.Bookings
            .Where(booking => booking.HotelId == hotel.Id)
            .OrderByDescending(booking => booking.CreatedAt)
            .Select(booking => new
            {
                booking.Id,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.TotalPrice,
                booking.Guests,
                booking.IsPaid,
                booking.PaymentMethod,
                Status = booking.Status.ToString().ToLower(),
                User = new { booking.User.Id, booking.User.Username },
                Room = new { booking.Room.Id, booking.Room.RoomType },
                Hotel = new { booking.Hotel.Id, booking.Hotel.Name }
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            dashboardData = new
            {
                totalBookings = bookings.Count,
                totalRevenue = bookings.Sum(booking => booking.TotalPrice),
                bookings
            }
        });
    }

    private async Task<bool> IsAvailable(Guid roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        var checkIn = AsUtc(checkInDate);
        var checkOut = AsUtc(checkOutDate);
        if (checkOut <= checkIn)
        {
            return false;
        }

        var roomExists = await _db.Rooms.AnyAsync(room => room.Id == roomId && room.IsAvailable);
        if (!roomExists)
        {
            return false;
        }

        var overlap = await _db.Bookings.AnyAsync(booking =>
            booking.RoomId == roomId
            && booking.Status != BookingStatus.Cancelled
            && booking.CheckInDate <= checkOut
            && booking.CheckOutDate >= checkIn);

        return !overlap;
    }

    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}