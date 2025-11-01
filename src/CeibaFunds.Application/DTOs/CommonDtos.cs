using CeibaFunds.Domain.Enums;

namespace CeibaFunds.Application.DTOs;

public record CustomerDto
{
    public string Id { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public decimal Balance { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public bool IsActive { get; init; }
    public string FullName { get; init; } = string.Empty;
}

public record FundDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal MinimumAmount { get; init; }
    public FundCategory Category { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record SubscriptionDto
{
    public string Id { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string FundId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime SubscriptionDate { get; init; }
    public DateTime? CancellationDate { get; init; }
    public SubscriptionStatus Status { get; init; }
    public FundDto? Fund { get; init; }
    public CustomerDto? Customer { get; init; }
}

public record TransactionDto
{
    public string Id { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string? FundId { get; init; }
    public string? SubscriptionId { get; init; }
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public TransactionStatus Status { get; init; }
    public FundDto? Fund { get; init; }
}