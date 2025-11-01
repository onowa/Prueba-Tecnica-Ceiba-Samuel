namespace CeibaFunds.Domain.ValueObjects;

public record SubscriptionId
{
    public string Value { get; init; }

    public SubscriptionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Subscription ID cannot be empty", nameof(value));

        Value = value;
    }

    public static SubscriptionId NewId() => new(Guid.NewGuid().ToString());

    public static implicit operator string(SubscriptionId subscriptionId) => subscriptionId.Value;
    public static implicit operator SubscriptionId(string value) => new(value);

    public override string ToString() => Value;
}