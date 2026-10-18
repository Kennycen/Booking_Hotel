public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = null!;
    public Guid RoomId { get; set; }
    public Guid HotelId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public int Guests { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string PaymentMethod { get; set; } = "Pay At Hotel";
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
}