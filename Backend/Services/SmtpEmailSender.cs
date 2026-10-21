using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

public interface IEmailSender
{
    Task SendBookingAsync(string toEmail, string toName, Guid bookingId, string hotelName, string hotelAddress, DateTime checkIn, decimal totalPrice, CancellationToken cancellationToken);
}

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtp;
    private readonly string _currency;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> smtp, IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _smtp = smtp.Value;
        _currency = configuration["Currency"] ?? "$";
        _logger = logger;
    }

    public async Task SendBookingAsync(
        string toEmail,
        string toName,
        Guid bookingId,
        string hotelName,
        string hotelAddress,
        DateTime checkIn,
        decimal totalPrice,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_smtp.User) || string.IsNullOrWhiteSpace(_smtp.From))
        {
            _logger.LogInformation("SMTP is not configured. Skipping booking email.");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_smtp.From));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "Hotel Booking Details";
            message.Body = new BodyBuilder
            {
                HtmlBody = $"""
                    <h2>Your Booking Details</h2>
                    <p>Dear {toName},</p>
                    <p>Thank you for your booking! Here are your details:</p>
                    <ul>
                      <li><strong>Booking ID:</strong> {bookingId}</li>
                      <li><strong>Hotel Name:</strong> {hotelName}</li>
                      <li><strong>Location:</strong> {hotelAddress}</li>
                      <li><strong>Date:</strong> {checkIn:D}</li>
                      <li><strong>Booking Amount:</strong> {_currency} {totalPrice}</li>
                    </ul>
                    <p>We look forward to welcoming you!</p>
                    """
            }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_smtp.User, _smtp.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Booking email failed for {Email}", toEmail);
        }
    }
}