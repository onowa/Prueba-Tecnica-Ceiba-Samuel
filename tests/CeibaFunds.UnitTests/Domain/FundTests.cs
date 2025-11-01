using Xunit;
using FluentAssertions;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;
using CeibaFunds.UnitTests.Builders;

namespace CeibaFunds.UnitTests.Domain;

public class FundTests
{
    [Fact]
    public void CreateFund_WithValidData_ShouldSucceed()
    {
        var expectedFundId = FundId.NewId();
        var expectedName = "Conservative Test Fund";
        var expectedCategory = FundCategory.ConservativeFixedIncome;
        var expectedMinimumAmount = 75000m;

        var fund = FundBuilder
            .Default()
            .WithId(expectedFundId)
            .WithName(expectedName)
            .WithCategory(expectedCategory)
            .WithMinimumAmount(expectedMinimumAmount)
            .Build();

        fund.Id.Should().Be(expectedFundId);
        fund.Name.Should().Be(expectedName);
        fund.Category.Should().Be(expectedCategory);
        fund.MinimumAmount.Should().Be(expectedMinimumAmount);
        fund.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateFund_WithInvalidName_ShouldThrowException(string invalidName)
    {
        var builder = FundBuilder
            .Default()
            .WithName(invalidName);

        var act = () => builder.Build();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Fund name cannot be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void CreateFund_WithInvalidMinimumAmount_ShouldThrowException(decimal invalidAmount)
    {
        var builder = FundBuilder
            .Default()
            .WithMinimumAmount(invalidAmount);

        var act = () => builder.Build();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Minimum amount must be greater than zero*");
    }

    [Fact]
    public void Deactivate_ActiveFund_ShouldSetInactive()
    {
        var fund = FundBuilder
            .Default()
            .WithActiveStatus()
            .Build();

        fund.Deactivate();

        fund.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_InactiveFund_ShouldSetActive()
    {
        var fund = FundBuilder
            .Default()
            .WithInactiveStatus()
            .Build();

        fund.Activate();

        fund.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdate()
    {
        var fund = FundBuilder
            .Default()
            .AsConservativeFund()
            .Build();
        var expectedName = "Updated Fund Name";
        var expectedDescription = "Updated fund description";
        var expectedMinimumAmount = 100000m;

        fund.UpdateDetails(expectedName, expectedDescription, expectedMinimumAmount);

        fund.Name.Should().Be(expectedName);
        fund.Description.Should().Be(expectedDescription);
        fund.MinimumAmount.Should().Be(expectedMinimumAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void UpdateDetails_WithInvalidName_ShouldThrowException(string invalidName)
    {
        var fund = FundBuilder
            .Default()
            .Build();

        var act = () => fund.UpdateDetails(invalidName, "Valid description", 75000m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Fund name cannot be empty*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void UpdateDetails_WithInvalidAmount_ShouldThrowException(decimal invalidAmount)
    {
        var fund = FundBuilder
            .Default()
            .Build();

        var act = () => fund.UpdateDetails("Valid Name", "Valid description", invalidAmount);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Minimum amount must be greater than zero*");
    }
}
