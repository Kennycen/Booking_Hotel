using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public sealed class RoomService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IImageStorage _images;

    public RoomService(AppDbContext db, ICurrentUser current, IImageStorage images)
    {
        _db = db;
        _current = current;
        _images = images;
    }

    public async Task<ServiceResult<List<RoomResponse>>> GetAvailableAsync(CancellationToken cancellationToken)
    {
        var rooms = await _db.Rooms
            .AsNoTracking()
            .Where(room => room.IsAvailable)
            .OrderByDescending(room => room.CreatedAt)
            .Select(room => new RoomResponse(
                room.Id,
                room.RoomType,
                room.PricePerNight,
                room.Amenities,
                room.Images,
                room.IsAvailable,
                new HotelResponse(
                    room.Hotel.Id,
                    room.Hotel.Name,
                    room.Hotel.Address,
                    room.Hotel.City,
                    room.Hotel.Contact,
                    new OwnerResponse(room.Hotel.Owner.Image))))
            .ToListAsync(cancellationToken);

        return ServiceResult<List<RoomResponse>>.Ok(rooms);
    }

    public async Task<ServiceResult<List<RoomResponse>>> GetOwnerRoomsAsync(CancellationToken cancellationToken)
    {
        var hotel = await CurrentHotelAsync(cancellationToken);
        if (hotel is null)
        {
            return ServiceResult<List<RoomResponse>>.Fail("No hotel found");
        }

        var rooms = await _db.Rooms
            .AsNoTracking()
            .Where(room => room.HotelId == hotel.Id)
            .OrderByDescending(room => room.CreatedAt)
            .Select(room => new RoomResponse(
                room.Id,
                room.RoomType,
                room.PricePerNight,
                room.Amenities,
                room.Images,
                room.IsAvailable,
                new HotelResponse(hotel.Id, hotel.Name, hotel.Address, hotel.City, hotel.Contact, null)))
            .ToListAsync(cancellationToken);

        return ServiceResult<List<RoomResponse>>.Ok(rooms);
    }

    public async Task<ServiceResult> CreateAsync(CreateRoomForm form, CancellationToken cancellationToken)
    {
        var hotel = await CurrentHotelAsync(cancellationToken);
        if (hotel is null)
        {
            return ServiceResult.Fail("No hotel found");
        }

        if (form.Images.Count is 0 or > 4)
        {
            return ServiceResult.Fail("Upload between 1 and 4 images.");
        }

        var amenities = JsonSerializer.Deserialize<List<string>>(form.Amenities) ?? [];
        var imageUrls = await _images.UploadAsync(form.Images, cancellationToken);

        _db.Rooms.Add(new Room
        {
            HotelId = hotel.Id,
            RoomType = form.RoomType,
            PricePerNight = form.PricePerNight,
            Amenities = amenities,
            Images = imageUrls.ToList()
        });
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok("Room created successfully");
    }

    public async Task<ServiceResult> ToggleAvailabilityAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var hotel = await CurrentHotelAsync(cancellationToken);
        if (hotel is null)
        {
            return ServiceResult.Fail("No hotel found");
        }

        var room = await _db.Rooms.FirstOrDefaultAsync(
            item => item.Id == roomId && item.HotelId == hotel.Id,
            cancellationToken);
        if (room is null)
        {
            return ServiceResult.Fail("Room not found");
        }

        room.IsAvailable = !room.IsAvailable;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok("Room availability Updated");
    }

    private async Task<Hotel?> CurrentHotelAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_current.UserId))
        {
            return null;
        }

        return await _db.Hotels.FirstOrDefaultAsync(hotel => hotel.OwnerId == _current.UserId, cancellationToken);
    }
}