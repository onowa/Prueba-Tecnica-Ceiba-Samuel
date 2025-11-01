namespace CeibaFunds.Domain.ValueObjects;

public record CustomerId
{
    public string Value { get; init; }

    public CustomerId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Customer ID cannot be empty", nameof(value));

        Value = value;
    }

    public static CustomerId NewId() => new(Guid.NewGuid().ToString());

    public static implicit operator string(CustomerId customerId) => customerId.Value;
    public static implicit operator CustomerId(string value) => new(value);

    public override string ToString() => Value;
}