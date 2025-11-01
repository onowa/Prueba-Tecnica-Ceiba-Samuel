using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;

namespace CeibaFunds.UnitTests.Builders;

public class FundBuilder
{
    private FundId _id = FundId.NewId();
    private string _name = "Conservative Fund";
    private FundCategory _category = FundCategory.ConservativeFixedIncome;
    private decimal _minimumAmount = 75000m;
    private bool _isActive = true;

    public FundBuilder WithId(FundId id)
    {
        _id = id;
        return this;
    }

    public FundBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public FundBuilder WithCategory(FundCategory category)
    {
        _category = category;
        return this;
    }

    public FundBuilder WithMinimumAmount(decimal minimumAmount)
    {
        _minimumAmount = minimumAmount;
        return this;
    }

    public FundBuilder WithActiveStatus()
    {
        _isActive = true;
        return this;
    }

    public FundBuilder WithInactiveStatus()
    {
        _isActive = false;
        return this;
    }

    public FundBuilder AsConservativeFund()
    {
        _name = "Conservative Fixed Income Fund";
        _category = FundCategory.ConservativeFixedIncome;
        _minimumAmount = 75000m;
        return this;
    }

    public FundBuilder AsModerateFund()
    {
        _name = "Moderate Mixed Fund";
        _category = FundCategory.ModerateMixedFund;
        _minimumAmount = 125000m;
        return this;
    }

    public FundBuilder AsAggressiveFund()
    {
        _name = "Aggressive Equity Fund";
        _category = FundCategory.AggressiveEquityFund;
        _minimumAmount = 250000m;
        return this;
    }

    public FundBuilder WithHighMinimumAmount()
    {
        return WithMinimumAmount(500000m);
    }

    public FundBuilder WithLowMinimumAmount()
    {
        return WithMinimumAmount(50000m);
    }

    public Fund Build()
    {
        // Constructor: Fund(FundId id, string name, string description, decimal minimumAmount, FundCategory category)
        var fund = new Fund(_id, _name, "Fund description", _minimumAmount, _category);

        if (!_isActive)
        {
            fund.Deactivate();
        }

        return fund;
    }

    public static FundBuilder Default() => new();
}
