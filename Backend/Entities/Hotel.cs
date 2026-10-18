public class Hotel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Contact { get; set; } = null!;
    public string City { get; set; } = null!;
    public string OwnerId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}