using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<TransactionRepository> _logger;

    public TransactionRepository(IDynamoDBContext context, ILogger<TransactionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Transaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving transaction by ID: {TransactionId}", id);
        return await _context.LoadAsync<Transaction>(id, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving transactions by customer ID: {CustomerId}", customerId.Value);
        var search = _context.ScanAsync<Transaction>(new List<Amazon.DynamoDBv2.DataModel.ScanCondition>());
        var allTransactions = await search.GetRemainingAsync(cancellationToken);
        return allTransactions.Where(t => t.CustomerId.Value == customerId.Value).OrderByDescending(t => t.CreatedAt);
    }

    public async Task<IEnumerable<Transaction>> GetBySubscriptionIdAsync(SubscriptionId subscriptionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving transactions by subscription ID: {SubscriptionId}", subscriptionId.Value);
        var search = _context.ScanAsync<Transaction>(new List<Amazon.DynamoDBv2.DataModel.ScanCondition>());
        var allTransactions = await search.GetRemainingAsync(cancellationToken);
        return allTransactions.Where(t => t.SubscriptionId?.Value == subscriptionId.Value).OrderByDescending(t => t.CreatedAt);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating transaction: {TransactionId}", transaction.Id);
        await _context.SaveAsync(transaction, cancellationToken);
        return transaction;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating transaction: {TransactionId}", transaction.Id);
        await _context.SaveAsync(transaction, cancellationToken);
        return transaction;
    }

    public async Task<IEnumerable<Transaction>> GetCustomerHistoryAsync(CustomerId customerId, int pageSize = 50,
                                                                       string? lastTransactionId = null,
                                                                       CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving customer transaction history: {CustomerId}", customerId.Value);
        var allTransactions = await GetByCustomerIdAsync(customerId, cancellationToken);

        // Simple pagination - in production you'd want more sophisticated pagination
        if (!string.IsNullOrEmpty(lastTransactionId))
        {
            var skipCount = allTransactions.TakeWhile(t => t.Id != lastTransactionId).Count() + 1;
            allTransactions = allTransactions.Skip(skipCount);
        }

        return allTransactions.Take(pageSize);
    }
}