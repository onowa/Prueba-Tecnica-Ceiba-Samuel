using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;

namespace CeibaFunds.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(SubscriptionId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetByFundIdAsync(FundId fundId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetActiveByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task DeleteAsync(SubscriptionId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(SubscriptionId id, CancellationToken cancellationToken = default);
    Task<bool> HasActiveSubscriptionAsync(CustomerId customerId, FundId fundId, CancellationToken cancellationToken = default);
}