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

public class SubscribeToFundHandler : IRequestHandler<SubscribeToFundCommand, SubscriptionDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IFundRepository _fundRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscribeToFundHandler> _logger;

    public SubscribeToFundHandler(
        ICustomerRepository customerRepository,
        IFundRepository fundRepository,
        ISubscriptionRepository subscriptionRepository,
        ITransactionRepository transactionRepository,
        INotificationService notificationService,
        IMapper mapper,
        ILogger<SubscribeToFundHandler> logger)
    {
        _customerRepository = customerRepository;
        _fundRepository = fundRepository;
        _subscriptionRepository = subscriptionRepository;
        _transactionRepository = transactionRepository;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SubscriptionDto> Handle(SubscribeToFundCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing fund subscription for Customer: {CustomerId}, Fund: {FundId}, Amount: {Amount}",
                              request.CustomerId, request.FundId, request.Amount);

        var customer = await _customerRepository.GetByIdAsync(new CustomerId(request.CustomerId), cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found: {CustomerId}", request.CustomerId);
            throw new ArgumentException($"Customer with ID {request.CustomerId} not found");
        }

        if (!customer.IsActive)
        {
            _logger.LogWarning("Inactive customer attempted subscription: {CustomerId}", request.CustomerId);
            throw new InvalidOperationException("Customer account is inactive");
        }

        var fund = await _fundRepository.GetByIdAsync(new FundId(request.FundId), cancellationToken);
        if (fund == null)
        {
            _logger.LogWarning("Fund not found: {FundId}", request.FundId);
            throw new ArgumentException($"Fund with ID {request.FundId} not found");
        }

        if (!fund.IsActive)
        {
            _logger.LogWarning("Inactive fund subscription attempted: {FundId}", request.FundId);
            throw new InvalidOperationException("Fund is not available for subscription");
        }

        if (request.Amount < fund.MinimumAmount)
        {
            _logger.LogWarning("Insufficient amount for fund subscription. Required: {MinAmount}, Provided: {Amount}",
                              fund.MinimumAmount, request.Amount);
            throw new InvalidOperationException($"Minimum subscription amount for this fund is {fund.MinimumAmount:C}");
        }

        if (!customer.CanSubscribeToFund(request.Amount))
        {
            _logger.LogWarning("Insufficient customer balance. Balance: {Balance}, Required: {Amount}",
                              customer.Balance, request.Amount);
            throw new InvalidOperationException("Insufficient balance for subscription");
        }

        var hasActiveSubscription = await _subscriptionRepository.HasActiveSubscriptionAsync(
            customer.Id, fund.Id, cancellationToken);

        if (hasActiveSubscription)
        {
            _logger.LogWarning("Customer already has active subscription to fund: {CustomerId}, {FundId}",
                              request.CustomerId, request.FundId);
            throw new InvalidOperationException("Customer already has an active subscription to this fund");
        }

        try
        {
            var subscription = new Subscription(
                SubscriptionId.NewId(),
                customer.Id,
                fund.Id,
                request.Amount
            );

            customer.DeductBalance(request.Amount, $"Subscription to {fund.Name}");

            var transaction = new Transaction(
                customer.Id,
                TransactionType.Subscription,
                request.Amount,
                $"Subscription to {fund.Name}",
                fund.Id,
                subscription.Id
            );

            // Save entities
            await _subscriptionRepository.CreateAsync(subscription, cancellationToken);
            await _customerRepository.UpdateAsync(customer, cancellationToken);
            await _transactionRepository.CreateAsync(transaction, cancellationToken);

            _logger.LogInformation("Fund subscription successful. SubscriptionId: {SubscriptionId}", subscription.Id.Value);

            // Send notifications
            try
            {
                await _notificationService.SendSubscriptionConfirmationAsync(
                    customer.Email,
                    customer.PhoneNumber,
                    customer.GetFullName(),
                    fund.Name,
                    request.Amount,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send subscription notification for {SubscriptionId}", subscription.Id.Value);
                // Don't fail the entire operation if notification fails
            }

            return _mapper.Map<SubscriptionDto>(subscription);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error processing fund subscription for Customer: {CustomerId}, Fund: {FundId}",
                            request.CustomerId, request.FundId);
            throw new InvalidOperationException("An error occurred while processing the subscription", ex);
        }
    }
}