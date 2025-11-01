using CeibaFunds.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {Email} with subject: {Subject}", email, subject);

        // PRODUCTION NOTE: Integrate with AWS SES for production deployment
        // Currently simulating email sending for development/testing purposes
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Email sent successfully to {Email}", email);
    }

    public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending SMS to {PhoneNumber}", phoneNumber);

        // PRODUCTION NOTE: Integrate with AWS SNS for production deployment
        // Currently simulating SMS sending for development/testing purposes
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
    }

    public async Task SendSubscriptionConfirmationAsync(string email, string phoneNumber, string customerName,
                                                       string fundName, decimal amount, CancellationToken cancellationToken = default)
    {
        var emailSubject = "Fund Subscription Confirmation";
        var emailBody = $@"
Dear {customerName},

Your subscription to {fundName} has been confirmed successfully.

Subscription Details:
- Fund: {fundName}
- Amount: ${amount:N2} COP
- Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

Thank you for choosing our investment platform.

Best regards,
CeibaFunds Team
";

        var smsMessage = $"CeibaFunds: Your subscription to {fundName} for ${amount:N2} COP has been confirmed. Reference: {Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var tasks = new List<Task>
        {
            SendEmailAsync(email, emailSubject, emailBody, cancellationToken),
            SendSmsAsync(phoneNumber, smsMessage, cancellationToken)
        };

        await Task.WhenAll(tasks);
    }

    public async Task SendCancellationConfirmationAsync(string email, string phoneNumber, string customerName,
                                                       string fundName, decimal refundAmount, CancellationToken cancellationToken = default)
    {
        var emailSubject = "Fund Subscription Cancellation Confirmation";
        var emailBody = $@"
Dear {customerName},

Your subscription to {fundName} has been cancelled successfully.

Cancellation Details:
- Fund: {fundName}
- Refund Amount: ${refundAmount:N2} COP
- Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

The refund has been credited to your account balance.

If you have any questions, please contact our support team.

Best regards,
CeibaFunds Team
";

        var smsMessage = $"CeibaFunds: Your {fundName} subscription has been cancelled. Refund of ${refundAmount:N2} COP processed. Reference: {Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var tasks = new List<Task>
        {
            SendEmailAsync(email, emailSubject, emailBody, cancellationToken),
            SendSmsAsync(phoneNumber, smsMessage, cancellationToken)
        };

        await Task.WhenAll(tasks);
    }
}