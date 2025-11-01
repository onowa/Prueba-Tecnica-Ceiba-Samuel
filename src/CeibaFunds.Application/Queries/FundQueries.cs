using MediatR;
using CeibaFunds.Application.DTOs;

namespace CeibaFunds.Application.Queries;

public record GetCustomerByIdQuery(string CustomerId) : IRequest<CustomerDto?>;

public record GetAllFundsQuery() : IRequest<IEnumerable<FundDto>>;

public record GetActiveFundsQuery() : IRequest<IEnumerable<FundDto>>;

public record GetCustomerSubscriptionsQuery(string CustomerId) : IRequest<IEnumerable<SubscriptionDto>>;

public record GetCustomerTransactionHistoryQuery(
    string CustomerId,
    int PageSize = 50,
    string? LastTransactionId = null
) : IRequest<IEnumerable<TransactionDto>>;

public record GetSubscriptionByIdQuery(string SubscriptionId) : IRequest<SubscriptionDto?>;

public record GetFundByIdQuery(string FundId) : IRequest<FundDto?>;