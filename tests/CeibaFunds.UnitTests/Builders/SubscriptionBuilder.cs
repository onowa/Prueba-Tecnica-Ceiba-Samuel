using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;

namespace CeibaFunds.UnitTests.Builders;

public class SubscriptionBuilder
{
    private SubscriptionId _id = SubscriptionId.NewId();
    private CustomerId _customerId = CustomerId.NewId();
    private FundId _fundId = FundId.NewId();
    private decimal _amount = 100000m;
    private bool _shouldCancel = false;

    public SubscriptionBuilder WithId(SubscriptionId id)
    {
        _id = id;
        return this;
    }

    public SubscriptionBuilder WithCustomerId(CustomerId customerId)
    {
        _customerId = customerId;
        return this;
    }

    public SubscriptionBuilder WithFundId(FundId fundId)
    {
        _fundId = fundId;
        return this;
    }

    public SubscriptionBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public SubscriptionBuilder WithValidAmount()
    {
        return WithAmount(100000m);
    }

    public SubscriptionBuilder WithHighAmount()
    {
        return WithAmount(500000m);
    }

    public SubscriptionBuilder WithLowAmount()
    {
        return WithAmount(75000m);
    }

    public SubscriptionBuilder WithInvalidAmount()
    {
        return WithAmount(0m);
    }

    public SubscriptionBuilder WithNegativeAmount()
    {
        return WithAmount(-100m);
    }

    public SubscriptionBuilder ThatIsCancelled()
    {
        _shouldCancel = true;
        return this;
    }

    public SubscriptionBuilder ForCustomer(Customer customer)
    {
        _customerId = customer.Id;
        return this;
    }

    public SubscriptionBuilder ForFund(Fund fund)
    {
        _fundId = fund.Id;
        return this;
    }

    public Subscription Build()
    {
        var subscription = new Subscription(_id, _customerId, _fundId, _amount);

        if (_shouldCancel)
        {
            subscription.Cancel();
        }

        return subscription;
    }

    public static SubscriptionBuilder Default() => new();
}
