public class User
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Image { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
    public List<string> RecentSearchCities { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Hotel? Hotel { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];
}