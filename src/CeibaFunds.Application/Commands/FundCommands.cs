using MediatR;
using CeibaFunds.Application.DTOs;

namespace CeibaFunds.Application.Commands;

public record SubscribeToFundCommand(
    string CustomerId,
    string FundId,
    decimal Amount
) : IRequest<SubscriptionDto>;

public record CancelSubscriptionCommand(
    string SubscriptionId,
    string CustomerId
) : IRequest<bool>;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime DateOfBirth
) : IRequest<CustomerDto>;

public record CreateFundCommand(
    string Name,
    string Description,
    decimal MinimumAmount,
    int Category
) : IRequest<FundDto>;