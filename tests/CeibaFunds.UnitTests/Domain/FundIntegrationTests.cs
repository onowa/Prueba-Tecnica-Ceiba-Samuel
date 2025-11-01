using Xunit;
using FluentAssertions;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.Enums;
using CeibaFunds.UnitTests.Builders;

namespace CeibaFunds.UnitTests.Domain;

public class FundIntegrationTests
{
    [Theory]
    [InlineData(FundCategory.ConservativeFixedIncome, 75000)]
    [InlineData(FundCategory.ModerateMixedFund, 125000)]
    [InlineData(FundCategory.AggressiveEquityFund, 250000)]
    public void CreateFund_WithDifferentCategories_ShouldHaveCorrectMinimumAmounts(
        FundCategory category,
        decimal expectedMinimum)
    {
        var fund = FundBuilder
            .Default()
            .WithCategory(category)
            .WithMinimumAmount(expectedMinimum)
            .Build();

        fund.Category.Should().Be(category);
        fund.MinimumAmount.Should().Be(expectedMinimum);
        fund.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Customer_SubscribeToConservativeFund_WhenHasSufficientBalance_ShouldSucceed()
    {
        var customer = CustomerBuilder
            .Default()
            .WithSufficientBalance()
            .Build();

        var conservativeFund = FundBuilder
            .Default()
            .AsConservativeFund()
            .Build();

        var subscription = SubscriptionBuilder
            .Default()
            .ForCustomer(customer)
            .ForFund(conservativeFund)
            .WithAmount(conservativeFund.MinimumAmount)
            .Build();

        var canSubscribe = customer.CanSubscribeToFund(subscription.Amount);

        canSubscribe.Should().BeTrue();
        subscription.CustomerId.Should().Be(customer.Id);
        subscription.FundId.Should().Be(conservativeFund.Id);
        subscription.Amount.Should().BeGreaterThanOrEqualTo(conservativeFund.MinimumAmount);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void Customer_CannotSubscribeToAggressiveFund_WhenHasInsufficientBalance_ShouldFail()
    {
        var customer = CustomerBuilder
            .Default()
            .WithInsufficientBalance() // 50,000
            .Build();

        var aggressiveFund = FundBuilder
            .Default()
            .AsAggressiveFund() // Minimum 250,000
            .Build();

        var canSubscribe = customer.CanSubscribeToFund(aggressiveFund.MinimumAmount);

        canSubscribe.Should().BeFalse();
        customer.Balance.Should().BeLessThan(aggressiveFund.MinimumAmount);
    }

    [Fact]
    public void FundLifecycle_CreateActivateDeactivateUpdate_ShouldWorkProperly()
    {
        var fund = FundBuilder
            .Default()
            .AsModerateFund()
            .WithActiveStatus()
            .Build();

        fund.IsActive.Should().BeTrue();
        fund.Category.Should().Be(FundCategory.ModerateMixedFund);

        fund.Deactivate();

        fund.IsActive.Should().BeFalse();

        fund.Activate();

        fund.IsActive.Should().BeTrue();

        var newName = "Updated Moderate Fund";
        var newDescription = "Enhanced moderate fund with better returns";
        var newMinimum = 150000m;

        fund.UpdateDetails(newName, newDescription, newMinimum);

        fund.Name.Should().Be(newName);
        fund.Description.Should().Be(newDescription);
        fund.MinimumAmount.Should().Be(newMinimum);
        fund.IsActive.Should().BeTrue(); // Should remain active after update
    }

    [Fact]
    public void SubscriptionWorkflow_SubscribeAndCancel_ShouldUpdateStatusProperly()
    {
        var customer = CustomerBuilder
            .Default()
            .WithSufficientBalance()
            .Build();

        var fund = FundBuilder
            .Default()
            .AsConservativeFund()
            .Build();

        var subscription = SubscriptionBuilder
            .Default()
            .ForCustomer(customer)
            .ForFund(fund)
            .WithAmount(fund.MinimumAmount + 25000m) // Extra amount
            .Build();

        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.IsActive.Should().BeTrue();
        subscription.CancellationDate.Should().BeNull();

        subscription.Cancel();

        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.IsActive.Should().BeFalse();
        subscription.CancellationDate.Should().NotBeNull();
        subscription.CancellationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
