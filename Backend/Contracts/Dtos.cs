using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class ServiceResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }

    public static ServiceResult Ok(string? message = null) => new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, string? message = null) =>
        new() { Success = true, Message = message, Data = data };

    public new static ServiceResult<T> Fail(string message) =>
        new() { Success = false, Message = message };
}

public sealed record RegisterHotelRequest(
    [param: Required] string Name,
    [param: Required] string Address,
    [param: Required] string Contact,
    [param: Required] string City);

public sealed record StoreRecentSearchRequest(
    [param: Required] string RecentSearchedCity);

public sealed class CreateRoomForm
{
    [Required]
    public string RoomType { get; set; } = "";

    [Range(1, 1_000_000)]
    public decimal PricePerNight { get; set; }

    public string Amenities { get; set; } = "[]";

    public List<IFormFile> Images { get; set; } = [];
}

public sealed record ToggleRoomRequest(Guid RoomId);

public sealed record CheckAvailabilityRequest(Guid Room, DateTime CheckInDate, DateTime CheckOutDate);

public sealed record CreateBookingRequest(Guid Room, DateTime CheckInDate, DateTime CheckOutDate, int Guests);

public sealed record OwnerResponse(string Image);

public sealed record HotelResponse(
    [property: JsonPropertyName("_id")] Guid Id,
    string Name,
    string Address,
    string City,
    string Contact,
    OwnerResponse? Owner);

public sealed record RoomResponse(
    [property: JsonPropertyName("_id")] Guid Id,
    string RoomType,
    decimal PricePerNight,
    List<string> Amenities,
    List<string> Images,
    bool IsAvailable,
    HotelResponse Hotel);

public sealed record RoomSummaryResponse(
    [property: JsonPropertyName("_id")] Guid Id,
    string RoomType,
    decimal PricePerNight,
    List<string> Images);

public sealed record HotelSummaryResponse(
    [property: JsonPropertyName("_id")] Guid Id,
    string Name,
    string Address,
    string City);

public sealed record BookingResponse(
    [property: JsonPropertyName("_id")] Guid Id,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    decimal TotalPrice,
    int Guests,
    bool IsPaid,
    string PaymentMethod,
    string Status,
    RoomSummaryResponse Room,
    HotelSummaryResponse Hotel);

public sealed record DashboardUserResponse(
    [property: JsonPropertyName("_id")] string Id,
    string Username);

public sealed record DashboardRoomResponse(
    [property: JsonPropertyName("_id")] Guid Id,
    string RoomType);

public sealed record DashboardBookingResponse(
    [property: JsonPropertyName("_id")] Guid Id,
    decimal TotalPrice,
    bool IsPaid,
    DashboardUserResponse User,
    DashboardRoomResponse Room);

public sealed record DashboardResponse(
    int TotalBookings,
    decimal TotalRevenue,
    List<DashboardBookingResponse> Bookings);