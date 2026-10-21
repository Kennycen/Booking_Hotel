using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly RoomService _rooms;

    public RoomsController(RoomService rooms)
    {
        _rooms = rooms;
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms(CancellationToken cancellationToken)
    {
        var result = await _rooms.GetAvailableAsync(cancellationToken);
        return Ok(new { success = true, rooms = result.Data });
    }

    [Authorize]
    [HttpGet("owner")]
    public async Task<IActionResult> GetOwnerRooms(CancellationToken cancellationToken)
    {
        var result = await _rooms.GetOwnerRoomsAsync(cancellationToken);
        return result.Success
            ? Ok(new { success = true, rooms = result.Data })
            : Ok(new { success = false, message = result.Message });
    }

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> CreateRoom([FromForm] CreateRoomForm form, CancellationToken cancellationToken)
    {
        var result = await _rooms.CreateAsync(form, cancellationToken);
        return Ok(new { success = result.Success, message = result.Message });
    }

    [Authorize]
    [HttpPost("toggle-availability")]
    public async Task<IActionResult> ToggleAvailability(ToggleRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await _rooms.ToggleAvailabilityAsync(request.RoomId, cancellationToken);
        return Ok(new { success = result.Success, message = result.Message });
    }
}