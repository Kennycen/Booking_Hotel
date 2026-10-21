using Microsoft.EntityFrameworkCore;

public sealed class UserService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _current;

    public UserService(AppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<ServiceResult<User>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_current.UserId))
        {
            return ServiceResult<User>.Fail("Not authenticated.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == _current.UserId, cancellationToken);
        return user is null
            ? ServiceResult<User>.Fail("User not found.")
            : ServiceResult<User>.Ok(user);
    }

    public async Task<ServiceResult> StoreRecentSearchAsync(string city, CancellationToken cancellationToken)
    {
        var current = await GetCurrentAsync(cancellationToken);
        if (!current.Success || current.Data is null)
        {
            return ServiceResult.Fail(current.Message ?? "Not authenticated.");
        }

        var cities = current.Data.RecentSearchCities.ToList();
        if (cities.Count >= 3)
        {
            cities.RemoveAt(0);
        }

        cities.Add(city);
        current.Data.RecentSearchCities = cities;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok("City added");
    }
}