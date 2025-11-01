namespace CeibaFunds.Domain.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string email, string subject, string body, CancellationToken cancellationToken = default);
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task SendSubscriptionConfirmationAsync(string email, string phoneNumber, string customerName, string fundName, decimal amount, CancellationToken cancellationToken = default);
    Task SendCancellationConfirmationAsync(string email, string phoneNumber, string customerName, string fundName, decimal refundAmount, CancellationToken cancellationToken = default);
}