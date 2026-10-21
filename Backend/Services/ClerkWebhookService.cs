using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Svix;

public sealed class ClerkWebhookService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _db;
    private readonly ClerkOptions _clerk;
    private readonly ILogger<ClerkWebhookService> _logger;

    public ClerkWebhookService(AppDbContext db, IOptions<ClerkOptions> clerk, ILogger<ClerkWebhookService> logger)
    {
        _db = db;
        _clerk = clerk.Value;
        _logger = logger;
    }

    public async Task<ServiceResult> HandleAsync(
        string payload,
        string? svixId,
        string? svixTimestamp,
        string? svixSignature,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_clerk.WebhookSecret)
            || string.IsNullOrWhiteSpace(svixId)
            || string.IsNullOrWhiteSpace(svixTimestamp)
            || string.IsNullOrWhiteSpace(svixSignature))
        {
            return ServiceResult.Fail("Missing webhook signature.");
        }

        try
        {
            var headers = new WebHeaderCollection
            {
                ["svix-id"] = svixId,
                ["svix-timestamp"] = svixTimestamp,
                ["svix-signature"] = svixSignature
            };
            new Webhook(_clerk.WebhookSecret).Verify(payload, headers);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Clerk webhook signature check failed.");
            return ServiceResult.Fail("Invalid webhook signature.");
        }

        var clerkEvent = JsonSerializer.Deserialize<ClerkEvent>(payload, JsonOptions);
        if (clerkEvent?.Data is null || string.IsNullOrWhiteSpace(clerkEvent.Data.Id))
        {
            return ServiceResult.Fail("Invalid webhook payload.");
        }

        switch (clerkEvent.Type)
        {
            case "user.created":
                await CreateAsync(clerkEvent.Data, cancellationToken);
                break;
            case "user.updated":
                await UpdateAsync(clerkEvent.Data, cancellationToken);
                break;
            case "user.deleted":
                await _db.Users.Where(user => user.Id == clerkEvent.Data.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                break;
        }

        return ServiceResult.Ok("Webhook Received");
    }

    private async Task CreateAsync(ClerkUserData data, CancellationToken cancellationToken)
    {
        if (await _db.Users.AnyAsync(user => user.Id == data.Id, cancellationToken))
        {
            await UpdateAsync(data, cancellationToken);
            return;
        }

        var email = data.EmailAddresses.FirstOrDefault()?.EmailAddress;
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        _db.Users.Add(new User
        {
            Id = data.Id,
            Email = email,
            Username = DisplayName(data),
            Image = data.ImageUrl ?? ""
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateAsync(ClerkUserData data, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == data.Id, cancellationToken);
        if (user is null)
        {
            await CreateAsync(data, cancellationToken);
            return;
        }

        var email = data.EmailAddresses.FirstOrDefault()?.EmailAddress;
        if (!string.IsNullOrWhiteSpace(email))
        {
            user.Email = email;
        }

        user.Username = DisplayName(data);
        user.Image = data.ImageUrl ?? "";
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string DisplayName(ClerkUserData data)
    {
        var name = string.Join(" ", new[] { data.FirstName, data.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(name) ? "Guest" : name;
    }

    private sealed class ClerkEvent
    {
        public string Type { get; set; } = "";
        public ClerkUserData? Data { get; set; }
    }

    private sealed class ClerkUserData
    {
        public string Id { get; set; } = "";

        [JsonPropertyName("email_addresses")]
        public List<ClerkEmail> EmailAddresses { get; set; } = [];

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }
    }

    private sealed class ClerkEmail
    {
        [JsonPropertyName("email_address")]
        public string EmailAddress { get; set; } = "";
    }
}