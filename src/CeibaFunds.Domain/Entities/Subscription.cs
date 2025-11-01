using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;

namespace CeibaFunds.Domain.Entities;

public class Subscription
{
    public SubscriptionId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public FundId FundId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime SubscriptionDate { get; private set; }
    public DateTime? CancellationDate { get; private set; }
    public SubscriptionStatus Status { get; private set; }

    public Customer Customer { get; private set; } = null!;
    public Fund Fund { get; private set; } = null!;

    public Subscription()
    {
        Id = null!;
        CustomerId = null!;
        FundId = null!;
    }

    public Subscription(SubscriptionId id, CustomerId customerId, FundId fundId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Subscription amount must be greater than zero", nameof(amount));

        Id = id ?? throw new ArgumentNullException(nameof(id));
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        FundId = fundId ?? throw new ArgumentNullException(nameof(fundId));
        Amount = amount;
        SubscriptionDate = DateTime.UtcNow;
        Status = SubscriptionStatus.Active;
    }

    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Subscription is already cancelled");

        Status = SubscriptionStatus.Cancelled;
        CancellationDate = DateTime.UtcNow;
    }

    public bool IsActive => Status == SubscriptionStatus.Active && !CancellationDate.HasValue;

    public TimeSpan GetSubscriptionDuration()
    {
        var endDate = CancellationDate ?? DateTime.UtcNow;
        return endDate - SubscriptionDate;
    }
}