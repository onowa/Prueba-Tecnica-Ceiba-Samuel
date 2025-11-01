using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;

namespace CeibaFunds.Domain.Entities;

public class Transaction
{
    public string Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public FundId? FundId { get; private set; }
    public SubscriptionId? SubscriptionId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public TransactionStatus Status { get; private set; }

    public Customer Customer { get; private set; } = null!;
    public Fund? Fund { get; init; }
    public Subscription? Subscription { get; init; }

    public Transaction()
    {
        Id = null!;
        CustomerId = null!;
        Description = null!;
    }

    public Transaction(CustomerId customerId, TransactionType type, decimal amount,
                      string description, FundId? fundId = null, SubscriptionId? subscriptionId = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be greater than zero", nameof(amount));

        Id = Guid.NewGuid().ToString();
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        FundId = fundId;
        SubscriptionId = subscriptionId;
        Type = type;
        Amount = amount;
        Description = description ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        Status = TransactionStatus.Completed;
    }

    public void MarkAsFailed(string reason)
    {
        Status = TransactionStatus.Failed;
        Description += $" - Failed: {reason}";
    }

    public void MarkAsProcessing()
    {
        Status = TransactionStatus.Processing;
    }

    public bool IsCompleted => Status == TransactionStatus.Completed;
    public bool IsFailed => Status == TransactionStatus.Failed;
}