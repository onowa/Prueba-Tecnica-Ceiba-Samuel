using CeibaFunds.Domain.ValueObjects;

namespace CeibaFunds.Domain.Entities;

public class Customer
{
    public CustomerId Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Subscription> _subscriptions = new();
    public IReadOnlyList<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    private readonly List<Transaction> _transactions = new();
    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    public Customer()
    {
        Id = null!;
        FirstName = null!;
        LastName = null!;
        Email = null!;
        PhoneNumber = null!;
    }

    public Customer(CustomerId id, string firstName, string lastName, string email,
                   string phoneNumber, DateTime dateOfBirth, decimal initialBalance = 500000)
    {
        ValidateCustomerData(firstName, lastName, email, phoneNumber, dateOfBirth);

        Id = id ?? throw new ArgumentNullException(nameof(id));
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Balance = initialBalance;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void UpdatePersonalInfo(string firstName, string lastName, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber ?? string.Empty;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool CanSubscribeToFund(decimal amount)
    {
        return IsActive && Balance >= amount;
    }

    public void DeductBalance(decimal amount, string reason)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient balance");

        Balance -= amount;
    }

    public void AddBalance(decimal amount, string reason)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        Balance += amount;
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    private static void ValidateCustomerData(string firstName, string lastName, string email,
                                           string phoneNumber, DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        if (dateOfBirth > DateTime.Now.AddYears(-18))
            throw new ArgumentException("Customer must be at least 18 years old", nameof(dateOfBirth));
    }
}