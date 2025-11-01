using Xunit;
using FluentAssertions;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;
using CeibaFunds.Domain.Enums;
using CeibaFunds.UnitTests.Builders;

namespace CeibaFunds.UnitTests.Domain;

public class SubscriptionTests
{
    [Fact]
    public void CreateSubscription_WithValidData_ShouldSucceed()
    {
        var subscriptionId = SubscriptionId.NewId();
        var customerId = CustomerId.NewId();
        var fundId = FundId.NewId();
        var expectedAmount = 100000m;

        var subscription = SubscriptionBuilder
            .Default()
            .WithId(subscriptionId)
            .WithCustomerId(customerId)
            .WithFundId(fundId)
            .WithAmount(expectedAmount)
            .Build();

        subscription.Id.Should().Be(subscriptionId);
        subscription.CustomerId.Should().Be(customerId);
        subscription.FundId.Should().Be(fundId);
        subscription.Amount.Should().Be(expectedAmount);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.IsActive.Should().BeTrue();
        subscription.CancellationDate.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void CreateSubscription_WithInvalidAmount_ShouldThrowException(decimal invalidAmount)
    {
        var builder = SubscriptionBuilder
            .Default()
            .WithAmount(invalidAmount);

        var act = () => builder.Build();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Subscription amount must be greater than zero*");
    }

    [Fact]
    public void Cancel_ActiveSubscription_ShouldUpdateStatusAndDate()
    {
        var subscription = SubscriptionBuilder
            .Default()
            .WithValidAmount()
            .Build();
        var beforeCancel = DateTime.UtcNow;

        subscription.Cancel();

        var afterCancel = DateTime.UtcNow;
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.IsActive.Should().BeFalse();
        subscription.CancellationDate.Should().NotBeNull();
        subscription.CancellationDate.Should().BeOnOrAfter(beforeCancel);
        subscription.CancellationDate.Should().BeOnOrBefore(afterCancel);
    }

    [Fact]
    public void Cancel_AlreadyCancelledSubscription_ShouldThrowException()
    {
        var subscription = SubscriptionBuilder
            .Default()
            .ThatIsCancelled()
            .Build();

        var act = () => subscription.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Subscription is already cancelled*");
    }

    [Fact]
    public async Task GetSubscriptionDuration_ForActiveSubscription_ShouldReturnCorrectDuration()
    {
        var subscription = SubscriptionBuilder
            .Default()
            .WithValidAmount()
            .Build();

        await Task.Delay(10); // Ensure time has passed

        var duration = subscription.GetSubscriptionDuration();

        duration.Should().BePositive();
        duration.TotalMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSubscriptionDuration_ForCancelledSubscription_ShouldReturnCorrectDuration()
    {
        var subscription = SubscriptionBuilder
            .Default()
            .WithValidAmount()
            .Build();

        await Task.Delay(10); // Ensure time has passed

        subscription.Cancel();
        var duration = subscription.GetSubscriptionDuration();

        duration.Should().BePositive();
        duration.TotalMilliseconds.Should().BeGreaterThan(0);
    }
}
