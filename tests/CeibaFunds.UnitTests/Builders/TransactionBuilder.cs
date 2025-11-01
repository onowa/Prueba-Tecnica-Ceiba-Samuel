using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;

namespace CeibaFunds.UnitTests.Builders;

public class TransactionBuilder
{
    private CustomerId _customerId = CustomerId.NewId();
    private FundId? _fundId = FundId.NewId();
    private SubscriptionId? _subscriptionId = null;
    private TransactionType _type = TransactionType.Subscription;
    private decimal _amount = 100000m;
    private string _description = "Test transaction";
    private bool _shouldMarkAsFailed = false;
    private bool _shouldMarkAsProcessing = false;

    public TransactionBuilder WithCustomerId(CustomerId customerId)
    {
        _customerId = customerId;
        return this;
    }

    public TransactionBuilder WithFundId(FundId? fundId)
    {
        _fundId = fundId;
        return this;
    }

    public TransactionBuilder WithSubscriptionId(SubscriptionId? subscriptionId)
    {
        _subscriptionId = subscriptionId;
        return this;
    }

    public TransactionBuilder WithType(TransactionType type)
    {
        _type = type;
        return this;
    }

    public TransactionBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public TransactionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TransactionBuilder AsSubscription()
    {
        _type = TransactionType.Subscription;
        _description = "Fund subscription";
        return this;
    }

    public TransactionBuilder AsCancellation()
    {
        _type = TransactionType.Cancellation;
        _description = "Subscription cancellation";
        return this;
    }

    public TransactionBuilder AsDeposit()
    {
        _type = TransactionType.Deposit;
        _fundId = null;
        _description = "Balance deposit";
        return this;
    }

    public TransactionBuilder AsWithdrawal()
    {
        _type = TransactionType.Withdrawal;
        _fundId = null;
        _description = "Balance withdrawal";
        return this;
    }

    public TransactionBuilder ThatFailed()
    {
        _shouldMarkAsFailed = true;
        return this;
    }

    public TransactionBuilder ThatIsProcessing()
    {
        _shouldMarkAsProcessing = true;
        return this;
    }

    public TransactionBuilder ForCustomer(Customer customer)
    {
        _customerId = customer.Id;
        return this;
    }

    public TransactionBuilder ForFund(Fund fund)
    {
        _fundId = fund.Id;
        return this;
    }

    public TransactionBuilder ForSubscription(Subscription subscription)
    {
        _subscriptionId = subscription.Id;
        _customerId = subscription.CustomerId;
        _fundId = subscription.FundId;
        return this;
    }

    public Transaction Build()
    {
        // Transaction constructor: (CustomerId customerId, TransactionType type, decimal amount, 
        // string description, FundId? fundId = null, SubscriptionId? subscriptionId = null)
        var transaction = new Transaction(_customerId, _type, _amount, _description, _fundId, _subscriptionId);

        if (_shouldMarkAsFailed)
        {
            transaction.MarkAsFailed("Test failure reason");
        }
        else if (_shouldMarkAsProcessing)
        {
            transaction.MarkAsProcessing();
        }

        return transaction;
    }

    public static TransactionBuilder Default() => new();
}
