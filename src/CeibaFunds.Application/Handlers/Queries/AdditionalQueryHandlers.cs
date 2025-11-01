using MediatR;
using AutoMapper;
using CeibaFunds.Application.Queries;
using CeibaFunds.Application.DTOs;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Application.Handlers.Queries;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerByIdHandler> _logger;

    public GetCustomerByIdHandler(
        ICustomerRepository customerRepository,
        IMapper mapper,
        ILogger<GetCustomerByIdHandler> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving customer: {CustomerId}", request.CustomerId);
        var customer = await _customerRepository.GetByIdAsync(new CustomerId(request.CustomerId), cancellationToken);
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }
}

public class GetAllFundsHandler : IRequestHandler<GetAllFundsQuery, IEnumerable<FundDto>>
{
    private readonly IFundRepository _fundRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllFundsHandler> _logger;

    public GetAllFundsHandler(
        IFundRepository fundRepository,
        IMapper mapper,
        ILogger<GetAllFundsHandler> logger)
    {
        _fundRepository = fundRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<FundDto>> Handle(GetAllFundsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all funds");
        var funds = await _fundRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<FundDto>>(funds);
    }
}

public class GetFundByIdHandler : IRequestHandler<GetFundByIdQuery, FundDto?>
{
    private readonly IFundRepository _fundRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFundByIdHandler> _logger;

    public GetFundByIdHandler(
        IFundRepository fundRepository,
        IMapper mapper,
        ILogger<GetFundByIdHandler> logger)
    {
        _fundRepository = fundRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundDto?> Handle(GetFundByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving fund: {FundId}", request.FundId);
        var fund = await _fundRepository.GetByIdAsync(new FundId(request.FundId), cancellationToken);
        return fund != null ? _mapper.Map<FundDto>(fund) : null;
    }
}

public class GetSubscriptionByIdHandler : IRequestHandler<GetSubscriptionByIdQuery, SubscriptionDto?>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSubscriptionByIdHandler> _logger;

    public GetSubscriptionByIdHandler(
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<GetSubscriptionByIdHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SubscriptionDto?> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving subscription: {SubscriptionId}", request.SubscriptionId);
        var subscription = await _subscriptionRepository.GetByIdAsync(new SubscriptionId(request.SubscriptionId), cancellationToken);
        return subscription != null ? _mapper.Map<SubscriptionDto>(subscription) : null;
    }
}