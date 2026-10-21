using Microsoft.EntityFrameworkCore;

public sealed class BookingService
{
    private readonly AppDbContext _db;
    private readonly UserService _users;
    private readonly IEmailSender _email;

    public BookingService(AppDbContext db, UserService users, IEmailSender email)
    {
        _db = db;
        _users = users;
        _email = email;
    }

    public async Task<bool> IsAvailableAsync(Guid roomId, DateTime checkInDate, DateTime checkOutDate, CancellationToken cancellationToken)
    {
        var checkIn = AsUtc(checkInDate);
        var checkOut = AsUtc(checkOutDate);
        if (checkOut <= checkIn)
        {
            return false;
        }

        var roomExists = await _db.Rooms.AnyAsync(
            room => room.Id == roomId && room.IsAvailable,
            cancellationToken);
        if (!roomExists)
        {
            return false;
        }

        var overlaps = await _db.Bookings.AnyAsync(booking =>
            booking.RoomId == roomId
            && booking.Status != BookingStatus.Cancelled
            && booking.CheckInDate <= checkOut
            && booking.CheckOutDate >= checkIn, cancellationToken);

        return !overlaps;
    }

    public async Task<ServiceResult> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var current = await _users.GetCurrentAsync(cancellationToken);
        if (!current.Success || current.Data is null)
        {
            return ServiceResult.Fail(current.Message ?? "Not authenticated.");
        }

        if (!await IsAvailableAsync(request.Room, request.CheckInDate, request.CheckOutDate, cancellationToken))
        {
            return ServiceResult.Fail("Room is not available");
        }

        var room = await _db.Rooms
            .Include(item => item.Hotel)
            .FirstOrDefaultAsync(item => item.Id == request.Room, cancellationToken);
        if (room is null)
        {
            return ServiceResult.Fail("Room is not available");
        }

        var checkIn = AsUtc(request.CheckInDate);
        var checkOut = AsUtc(request.CheckOutDate);
        var nights = (int)Math.Ceiling((checkOut - checkIn).TotalDays);
        if (nights < 1)
        {
            return ServiceResult.Fail("Room is not available");
        }

        var booking = new Booking
        {
            UserId = current.Data.Id,
            RoomId = room.Id,
            HotelId = room.HotelId,
            Guests = request.Guests,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalPrice = room.PricePerNight * nights
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(cancellationToken);

        await _email.SendBookingAsync(
            current.Data.Email,
            current.Data.Username,
            booking.Id,
            room.Hotel.Name,
            room.Hotel.Address,
            booking.CheckInDate,
            booking.TotalPrice,
            cancellationToken);

        return ServiceResult.Ok("Booking created successfully");
    }

    public async Task<ServiceResult<List<BookingResponse>>> GetForUserAsync(CancellationToken cancellationToken)
    {
        var current = await _users.GetCurrentAsync(cancellationToken);
        if (!current.Success || current.Data is null)
        {
            return ServiceResult<List<BookingResponse>>.Fail(current.Message ?? "Not authenticated.");
        }

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == current.Data.Id)
            .OrderByDescending(booking => booking.CreatedAt)
            .Select(booking => new BookingResponse(
                booking.Id,
                booking.CheckInDate,
                booking.CheckOutDate,
                booking.TotalPrice,
                booking.Guests,
                booking.IsPaid,
                booking.PaymentMethod,
                booking.Status.ToString().ToLower(),
                new RoomSummaryResponse(booking.Room.Id, booking.Room.RoomType, booking.Room.PricePerNight, booking.Room.Images),
                new HotelSummaryResponse(booking.Hotel.Id, booking.Hotel.Name, booking.Hotel.Address, booking.Hotel.City)))
            .ToListAsync(cancellationToken);

        return ServiceResult<List<BookingResponse>>.Ok(bookings);
    }

    public async Task<ServiceResult<DashboardResponse>> GetForHotelAsync(CancellationToken cancellationToken)
    {
        var current = await _users.GetCurrentAsync(cancellationToken);
        if (!current.Success || current.Data is null)
        {
            return ServiceResult<DashboardResponse>.Fail(current.Message ?? "Not authenticated.");
        }

        var hotel = await _db.Hotels.AsNoTracking()
            .FirstOrDefaultAsync(item => item.OwnerId == current.Data.Id, cancellationToken);
        if (hotel is null)
        {
            return ServiceResult<DashboardResponse>.Fail("No Hotel found");
        }

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(booking => booking.HotelId == hotel.Id)
            .OrderByDescending(booking => booking.CreatedAt)
            .Select(booking => new DashboardBookingResponse(
                booking.Id,
                booking.TotalPrice,
                booking.IsPaid,
                new DashboardUserResponse(booking.User.Id, booking.User.Username),
                new DashboardRoomResponse(booking.Room.Id, booking.Room.RoomType)))
            .ToListAsync(cancellationToken);

        return ServiceResult<DashboardResponse>.Ok(new DashboardResponse(
            bookings.Count,
            bookings.Sum(booking => booking.TotalPrice),
            bookings));
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