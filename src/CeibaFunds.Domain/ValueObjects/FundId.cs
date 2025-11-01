namespace CeibaFunds.Domain.ValueObjects;

public record FundId
{
    public string Value { get; init; }

    public FundId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Fund ID cannot be empty", nameof(value));

        Value = value;
    }

    public static FundId NewId() => new(Guid.NewGuid().ToString());

    public static implicit operator string(FundId fundId) => fundId.Value;
    public static implicit operator FundId(string value) => new(value);

    public override string ToString() => Value;
}