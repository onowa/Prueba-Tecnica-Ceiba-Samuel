using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;

namespace CeibaFunds.UnitTests.Builders;

public class CustomerBuilder
{
    private CustomerId _id = CustomerId.NewId();
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string _email = "john.doe@example.com";
    private string _phoneNumber = "+1234567890";
    private DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-30);
    private decimal _initialBalance = 1000000m;

    public CustomerBuilder WithId(CustomerId id)
    {
        _id = id;
        return this;
    }

    public CustomerBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public CustomerBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CustomerBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public CustomerBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public CustomerBuilder WithAge(int age)
    {
        _dateOfBirth = DateTime.UtcNow.AddYears(-age);
        return this;
    }

    public CustomerBuilder WithBalance(decimal balance)
    {
        _initialBalance = balance;
        return this;
    }

    public CustomerBuilder WithMinorAge()
    {
        return WithAge(17);
    }

    public CustomerBuilder WithAdultAge()
    {
        return WithAge(25);
    }

    public CustomerBuilder WithInsufficientBalance()
    {
        return WithBalance(50000m);
    }

    public CustomerBuilder WithSufficientBalance()
    {
        return WithBalance(1000000m);
    }

    public Customer Build()
    {
        return new Customer(_id, _firstName, _lastName, _email, _phoneNumber, _dateOfBirth, _initialBalance);
    }

    public static CustomerBuilder Default() => new();
}
