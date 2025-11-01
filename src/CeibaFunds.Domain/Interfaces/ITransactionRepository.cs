using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;

namespace CeibaFunds.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetBySubscriptionIdAsync(SubscriptionId subscriptionId, CancellationToken cancellationToken = default);
    Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetCustomerHistoryAsync(CustomerId customerId, int pageSize = 50, string? lastTransactionId = null, CancellationToken cancellationToken = default);
}