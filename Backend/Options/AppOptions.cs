public sealed class ClerkOptions
{
    public string Authority { get; set; } = "";
    public string WebhookSecret { get; set; } = "";
}

public sealed class CloudinaryOptions
{
    public string CloudName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = "smtp-relay.brevo.com";
    public int Port { get; set; } = 587;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
}
