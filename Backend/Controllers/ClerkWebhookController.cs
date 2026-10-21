using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/clerk")]
public class ClerkWebhookController : ControllerBase
{
    private readonly ClerkWebhookService _webhooks;

    public ClerkWebhookController(ClerkWebhookService webhooks)
    {
        _webhooks = webhooks;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var result = await _webhooks.HandleAsync(
            payload,
            Request.Headers["svix-id"],
            Request.Headers["svix-timestamp"],
            Request.Headers["svix-signature"],
            cancellationToken);

        return Ok(new { success = result.Success, message = result.Message });
    }
}