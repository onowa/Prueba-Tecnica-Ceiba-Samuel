using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<SubscriptionRepository> _logger;

    public SubscriptionRepository(IDynamoDBContext context, ILogger<SubscriptionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Subscription?> GetByIdAsync(SubscriptionId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving subscription by ID: {SubscriptionId}", id.Value);
        return await _context.LoadAsync<Subscription>(id.Value, cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving subscriptions by customer ID: {CustomerId}", customerId.Value);
        var search = _context.ScanAsync<Subscription>(new List<Amazon.DynamoDBv2.DataModel.ScanCondition>());
        var allSubscriptions = await search.GetRemainingAsync(cancellationToken);
        return allSubscriptions.Where(s => s.CustomerId.Value == customerId.Value);
    }

    public async Task<IEnumerable<Subscription>> GetByFundIdAsync(FundId fundId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving subscriptions by fund ID: {FundId}", fundId.Value);
        var search = _context.ScanAsync<Subscription>(new List<Amazon.DynamoDBv2.DataModel.ScanCondition>());
        var allSubscriptions = await search.GetRemainingAsync(cancellationToken);
        return allSubscriptions.Where(s => s.FundId.Value == fundId.Value);
    }

    public async Task<IEnumerable<Subscription>> GetActiveByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving active subscriptions by customer ID: {CustomerId}", customerId.Value);
        var subscriptions = await GetByCustomerIdAsync(customerId, cancellationToken);
        return subscriptions.Where(s => s.IsActive);
    }

    public async Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating subscription: {SubscriptionId}", subscription.Id.Value);
        await _context.SaveAsync(subscription, cancellationToken);
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating subscription: {SubscriptionId}", subscription.Id.Value);
        await _context.SaveAsync(subscription, cancellationToken);
        return subscription;
    }

    public async Task DeleteAsync(SubscriptionId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting subscription: {SubscriptionId}", id.Value);
        await _context.DeleteAsync<Subscription>(id.Value, cancellationToken);
    }

    public async Task<bool> ExistsAsync(SubscriptionId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if subscription exists: {SubscriptionId}", id.Value);
        var subscription = await GetByIdAsync(id, cancellationToken);
        return subscription != null;
    }

    public async Task<bool> HasActiveSubscriptionAsync(CustomerId customerId, FundId fundId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking active subscription for customer {CustomerId} and fund {FundId}", customerId.Value, fundId.Value);
        var activeSubscriptions = await GetActiveByCustomerIdAsync(customerId, cancellationToken);
        return activeSubscriptions.Any(s => s.FundId.Value == fundId.Value);
    }
}