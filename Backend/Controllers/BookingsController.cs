using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly BookingService _bookings;

    public BookingsController(BookingService bookings)
    {
        _bookings = bookings;
    }

    [HttpPost("check-availability")]
    public async Task<IActionResult> CheckAvailability(CheckAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var available = await _bookings.IsAvailableAsync(request.Room, request.CheckInDate, request.CheckOutDate, cancellationToken);
        return Ok(new { success = true, isAvailable = available });
    }

    [Authorize]
    [HttpPost("book")]
    public async Task<IActionResult> CreateBooking(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookings.CreateAsync(request, cancellationToken);
        return Ok(new { success = result.Success, message = result.Message });
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<IActionResult> GetUserBookings(CancellationToken cancellationToken)
    {
        var result = await _bookings.GetForUserAsync(cancellationToken);
        return result.Success
            ? Ok(new { success = true, bookings = result.Data })
            : Ok(new { success = false, message = result.Message });
    }

    [Authorize]
    [HttpGet("hotel")]
    public async Task<IActionResult> GetHotelBookings(CancellationToken cancellationToken)
    {
        var result = await _bookings.GetForHotelAsync(cancellationToken);
        return result.Success
            ? Ok(new { success = true, dashboardData = result.Data })
            : Ok(new { success = false, message = result.Message });
    }
}