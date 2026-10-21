using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/hotels")]
public class HotelsController : ControllerBase
{
    private readonly HotelService _hotels;

    public HotelsController(HotelService hotels)
    {
        _hotels = hotels;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterHotel(RegisterHotelRequest request, CancellationToken cancellationToken)
    {
        var result = await _hotels.RegisterAsync(request, cancellationToken);
        return Ok(new { success = result.Success, message = result.Message });
    }
}