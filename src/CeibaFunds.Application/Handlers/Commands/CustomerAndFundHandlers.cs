using MediatR;
using AutoMapper;
using CeibaFunds.Application.Commands;
using CeibaFunds.Application.DTOs;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Application.Handlers.Commands;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCustomerHandler> _logger;

    public CreateCustomerHandler(
        ICustomerRepository customerRepository,
        IMapper mapper,
        ILogger<CreateCustomerHandler> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating customer with email: {Email}", request.Email);

        var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"Customer with email {request.Email} already exists");
        }

        var customer = new Customer(
            CustomerId.NewId(),
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DateOfBirth
        );

        var createdCustomer = await _customerRepository.CreateAsync(customer, cancellationToken);

        _logger.LogInformation("Customer created successfully with ID: {CustomerId}", createdCustomer.Id.Value);

        return _mapper.Map<CustomerDto>(createdCustomer);
    }
}

public class CreateFundHandler : IRequestHandler<CreateFundCommand, FundDto>
{
    private readonly IFundRepository _fundRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFundHandler> _logger;

    public CreateFundHandler(
        IFundRepository fundRepository,
        IMapper mapper,
        ILogger<CreateFundHandler> logger)
    {
        _fundRepository = fundRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FundDto> Handle(CreateFundCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating fund: {Name}", request.Name);

        var fund = new Fund(
            FundId.NewId(),
            request.Name,
            request.Description,
            request.MinimumAmount,
            (FundCategory)request.Category
        );

        var createdFund = await _fundRepository.CreateAsync(fund, cancellationToken);

        _logger.LogInformation("Fund created successfully with ID: {FundId}", createdFund.Id.Value);

        return _mapper.Map<FundDto>(createdFund);
    }
}