using MediatR;
using AutoMapper;
using CeibaFunds.Application.Queries;
using CeibaFunds.Application.DTOs;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Application.Handlers.Queries;

public class GetCustomerTransactionHistoryHandler : IRequestHandler<GetCustomerTransactionHistoryQuery, IEnumerable<TransactionDto>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerTransactionHistoryHandler> _logger;

    public GetCustomerTransactionHistoryHandler(
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<GetCustomerTransactionHistoryHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TransactionDto>> Handle(GetCustomerTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving transaction history for customer: {CustomerId}", request.CustomerId);

        var transactions = await _transactionRepository.GetCustomerHistoryAsync(
            new CustomerId(request.CustomerId),
            request.PageSize,
            request.LastTransactionId,
            cancellationToken);

        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }
}

public class GetActiveFundsHandler : IRequestHandler<GetActiveFundsQuery, IEnumerable<FundDto>>
{
    private readonly IFundRepository _fundRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActiveFundsHandler> _logger;

    public GetActiveFundsHandler(
        IFundRepository fundRepository,
        IMapper mapper,
        ILogger<GetActiveFundsHandler> logger)
    {
        _fundRepository = fundRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<FundDto>> Handle(GetActiveFundsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all active funds");

        var funds = await _fundRepository.GetActiveAsync(cancellationToken);
        return _mapper.Map<IEnumerable<FundDto>>(funds);
    }
}

public class GetCustomerSubscriptionsHandler : IRequestHandler<GetCustomerSubscriptionsQuery, IEnumerable<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerSubscriptionsHandler> _logger;

    public GetCustomerSubscriptionsHandler(
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<GetCustomerSubscriptionsHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<SubscriptionDto>> Handle(GetCustomerSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving subscriptions for customer: {CustomerId}", request.CustomerId);

        var subscriptions = await _subscriptionRepository.GetByCustomerIdAsync(
            new CustomerId(request.CustomerId),
            cancellationToken);

        return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
    }
}