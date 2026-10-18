public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HotelId { get; set; }
    public string RoomType { get; set; } = null!;
    public decimal PricePerNight { get; set; }
    public List<string> Amenities { get; set; } = [];
    public List<string> Images { get; set; } = [];
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Hotel Hotel { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = [];
}