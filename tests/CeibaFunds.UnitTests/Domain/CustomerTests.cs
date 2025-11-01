using Xunit;
using FluentAssertions;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;
using CeibaFunds.UnitTests.Builders;

namespace CeibaFunds.UnitTests.Domain;

public class CustomerTests
{
    [Fact]
    public void CreateCustomer_WithValidData_ShouldSucceed()
    {
        var expectedCustomerId = CustomerId.NewId();
        var expectedFirstName = "John";
        var expectedLastName = "Doe";
        var expectedEmail = "john.doe@example.com";
        var expectedDateOfBirth = DateTime.Now.AddYears(-25);
        var expectedBalance = 1000000m;

        var customer = CustomerBuilder
            .Default()
            .WithId(expectedCustomerId)
            .WithFirstName(expectedFirstName)
            .WithLastName(expectedLastName)
            .WithEmail(expectedEmail)
            .WithDateOfBirth(expectedDateOfBirth)
            .WithBalance(expectedBalance)
            .Build();

        customer.Id.Should().Be(expectedCustomerId);
        customer.FirstName.Should().Be(expectedFirstName);
        customer.LastName.Should().Be(expectedLastName);
        customer.Email.Should().Be(expectedEmail);
        customer.DateOfBirth.Should().Be(expectedDateOfBirth);
        customer.Balance.Should().Be(expectedBalance);
        customer.IsActive.Should().BeTrue();
        customer.GetFullName().Should().Be($"{expectedFirstName} {expectedLastName}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateCustomer_WithInvalidFirstName_ShouldThrowException(string invalidFirstName)
    {
        var builder = CustomerBuilder
            .Default()
            .WithFirstName(invalidFirstName)
            .WithAdultAge();

        var act = () => builder.Build();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name cannot be empty*");
    }

    [Fact]
    public void CreateCustomer_WithUnderage_ShouldThrowException()
    {
        var builder = CustomerBuilder
            .Default()
            .WithMinorAge();

        var act = () => builder.Build();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be at least 18 years old*");
    }

    [Fact]
    public void CanSubscribeToFund_WithSufficientBalance_ShouldReturnTrue()
    {
        var customer = CustomerBuilder
            .Default()
            .WithSufficientBalance()
            .Build();
        var subscriptionAmount = 100000m;

        var canSubscribe = customer.CanSubscribeToFund(subscriptionAmount);

        canSubscribe.Should().BeTrue();
    }

    [Fact]
    public void CanSubscribeToFund_WithInsufficientBalance_ShouldReturnFalse()
    {
        var customer = CustomerBuilder
            .Default()
            .WithInsufficientBalance()
            .Build();
        var subscriptionAmount = 100000m; // More than available balance

        var canSubscribe = customer.CanSubscribeToFund(subscriptionAmount);

        canSubscribe.Should().BeFalse();
    }

    [Fact]
    public void DeductBalance_WithValidAmount_ShouldDecrementBalance()
    {
        var initialBalance = 1000000m;
        var deductionAmount = 100000m;
        var customer = CustomerBuilder
            .Default()
            .WithBalance(initialBalance)
            .Build();

        customer.DeductBalance(deductionAmount, "Test subscription");

        customer.Balance.Should().Be(initialBalance - deductionAmount);
    }

    [Fact]
    public void DeductBalance_WithExcessiveAmount_ShouldThrowException()
    {
        var customer = CustomerBuilder
            .Default()
            .WithInsufficientBalance() // 50000m
            .Build();
        var deductionAmount = 100000m; // More than available

        var act = () => customer.DeductBalance(deductionAmount, "Test subscription");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient balance*");
    }

    [Fact]
    public void AddBalance_WithValidAmount_ShouldIncrementBalance()
    {
        var initialBalance = 1000000m;
        var additionAmount = 100000m;
        var customer = CustomerBuilder
            .Default()
            .WithBalance(initialBalance)
            .Build();

        customer.AddBalance(additionAmount, "Test refund");

        customer.Balance.Should().Be(initialBalance + additionAmount);
    }
}
