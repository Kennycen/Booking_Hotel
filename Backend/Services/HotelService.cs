using Microsoft.EntityFrameworkCore;

public sealed class HotelService
{
    private readonly AppDbContext _db;
    private readonly UserService _users;

    public HotelService(AppDbContext db, UserService users)
    {
        _db = db;
        _users = users;
    }

    public async Task<ServiceResult> RegisterAsync(RegisterHotelRequest request, CancellationToken cancellationToken)
    {
        var current = await _users.GetCurrentAsync(cancellationToken);
        if (!current.Success || current.Data is null)
        {
            return ServiceResult.Fail(current.Message ?? "Not authenticated.");
        }

        var alreadyRegistered = await _db.Hotels.AnyAsync(
            hotel => hotel.OwnerId == current.Data.Id,
            cancellationToken);
        if (alreadyRegistered)
        {
            return ServiceResult.Fail("Hotel already registered.");
        }

        _db.Hotels.Add(new Hotel
        {
            Name = request.Name,
            Address = request.Address,
            Contact = request.Contact,
            City = request.City,
            OwnerId = current.Data.Id
        });
        current.Data.Role = UserRole.HotelOwner;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok("Hotel Registered Successfully.");
    }
}