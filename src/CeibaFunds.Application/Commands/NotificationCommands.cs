using CeibaFunds.Domain.Enums;
using MediatR;

namespace CeibaFunds.Application.Commands;

public record NotificationRequest(
    string CustomerId,
    string Email,
    string PhoneNumber,
    NotificationType Type,
    string Message,
    string Subject = ""
);

public record SendNotificationCommand(
    string CustomerId,
    string FundName,
    decimal Amount,
    string Email,
    string PhoneNumber,
    NotificationType PreferredType
) : IRequest<bool>;