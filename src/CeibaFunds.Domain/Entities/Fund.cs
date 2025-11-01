using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;
using Amazon.DynamoDBv2.DataModel;

namespace CeibaFunds.Domain.Entities;

[DynamoDBTable("Funds")]
public class Fund
{
    [DynamoDBHashKey("Id")]
    [DynamoDBProperty]
    public string IdString
    {
        get => Id?.Value ?? string.Empty;
        set => Id = string.IsNullOrEmpty(value) ? null! : new FundId(value);
    }

    [DynamoDBIgnore]
    public FundId Id { get; private set; }

    [DynamoDBProperty]
    public string Name { get; private set; }

    [DynamoDBProperty]
    public string Description { get; private set; }

    [DynamoDBProperty]
    public decimal MinimumAmount { get; private set; }

    [DynamoDBProperty("Category")]
    public string CategoryString
    {
        get => Category.ToString();
        set => Category = Enum.TryParse<FundCategory>(value, out var category) ? category : FundCategory.ConservativeFixedIncome;
    }

    [DynamoDBIgnore]
    public FundCategory Category { get; private set; }

    [DynamoDBProperty]
    public bool IsActive { get; private set; }

    [DynamoDBProperty]
    public DateTime CreatedAt { get; private set; }

    public Fund()
    {
        Id = null!;
        Name = string.Empty;
        Description = string.Empty;
        Category = FundCategory.ConservativeFixedIncome;
    }

    public Fund(FundId id, string name, string description, decimal minimumAmount, FundCategory category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Fund name cannot be empty", nameof(name));

        if (minimumAmount <= 0)
            throw new ArgumentException("Minimum amount must be greater than zero", nameof(minimumAmount));

        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name;
        Description = description ?? string.Empty;
        MinimumAmount = minimumAmount;
        Category = category;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateDetails(string name, string description, decimal minimumAmount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Fund name cannot be empty", nameof(name));

        if (minimumAmount <= 0)
            throw new ArgumentException("Minimum amount must be greater than zero", nameof(minimumAmount));

        Name = name;
        Description = description ?? string.Empty;
        MinimumAmount = minimumAmount;
    }
}