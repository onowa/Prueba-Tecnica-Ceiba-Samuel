using MediatR;
using CeibaFunds.Application.Commands;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Application.Handlers.Commands;

public class CancelSubscriptionHandler : IRequestHandler<CancelSubscriptionCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IFundRepository _fundRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CancelSubscriptionHandler> _logger;

    public CancelSubscriptionHandler(
        ICustomerRepository customerRepository,
        IFundRepository fundRepository,
        ISubscriptionRepository subscriptionRepository,
        ITransactionRepository transactionRepository,
        INotificationService notificationService,
        ILogger<CancelSubscriptionHandler> logger)
    {
        _customerRepository = customerRepository;
        _fundRepository = fundRepository;
        _subscriptionRepository = subscriptionRepository;
        _transactionRepository = transactionRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing subscription cancellation for SubscriptionId: {SubscriptionId}, CustomerId: {CustomerId}",
                              request.SubscriptionId, request.CustomerId);

        var subscription = await _subscriptionRepository.GetByIdAsync(new SubscriptionId(request.SubscriptionId), cancellationToken);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found: {SubscriptionId}", request.SubscriptionId);
            throw new ArgumentException($"Subscription with ID {request.SubscriptionId} not found");
        }

        if (subscription.CustomerId.Value != request.CustomerId)
        {
            _logger.LogWarning("Customer {CustomerId} attempted to cancel subscription {SubscriptionId} that belongs to {OwnerCustomerId}",
                              request.CustomerId, request.SubscriptionId, subscription.CustomerId.Value);
            throw new UnauthorizedAccessException("Customer can only cancel their own subscriptions");
        }

        if (!subscription.IsActive)
        {
            _logger.LogWarning("Attempted to cancel inactive subscription: {SubscriptionId}", request.SubscriptionId);
            throw new InvalidOperationException("Subscription is already cancelled or inactive");
        }

        var customer = await _customerRepository.GetByIdAsync(subscription.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {subscription.CustomerId.Value} not found for subscription cancellation");
        }

        var fund = await _fundRepository.GetByIdAsync(subscription.FundId, cancellationToken);
        if (fund == null)
        {
            throw new InvalidOperationException($"Fund {subscription.FundId.Value} not found for subscription cancellation");
        }

        try
        {
            // Cancel the subscription
            subscription.Cancel();

            // Refund the amount to customer balance
            customer.AddBalance(subscription.Amount, $"Refund from cancelled subscription to {fund.Name}");

            var transaction = new Transaction(
                customer.Id,
                TransactionType.Cancellation,
                subscription.Amount,
                $"Cancellation refund for {fund.Name}",
                fund.Id,
                subscription.Id
            );

            // Save changes
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
            await _customerRepository.UpdateAsync(customer, cancellationToken);
            await _transactionRepository.CreateAsync(transaction, cancellationToken);

            _logger.LogInformation("Subscription cancellation successful. SubscriptionId: {SubscriptionId}, RefundAmount: {Amount}",
                                  subscription.Id.Value, subscription.Amount);

            // Send notifications
            try
            {
                await _notificationService.SendCancellationConfirmationAsync(
                    customer.Email,
                    customer.PhoneNumber,
                    customer.GetFullName(),
                    fund.Name,
                    subscription.Amount,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send cancellation notification for {SubscriptionId}", subscription.Id.Value);
                // Don't fail the entire operation if notification fails
            }

            return true;
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException || ex is UnauthorizedAccessException))
        {
            throw new InvalidOperationException($"An error occurred while processing the cancellation for subscription {request.SubscriptionId}", ex);
        }
    }
}