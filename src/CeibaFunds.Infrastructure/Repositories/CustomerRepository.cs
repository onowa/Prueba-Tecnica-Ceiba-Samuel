using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(IDynamoDBContext context, ILogger<CustomerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving customer by ID: {CustomerId}", id.Value);
        return await _context.LoadAsync<Customer>(id.Value, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving customer by email: {Email}", email);

        var allCustomers = await GetAllAsync(cancellationToken);
        return allCustomers.FirstOrDefault(c => c.Email == email);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving all customers");

        var search = _context.ScanAsync<Customer>(new List<ScanCondition>());
        return await search.GetRemainingAsync(cancellationToken);
    }

    public async Task<Customer> CreateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating customer: {CustomerId}", customer.Id.Value);

        await _context.SaveAsync(customer, cancellationToken);
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating customer: {CustomerId}", customer.Id.Value);

        await _context.SaveAsync(customer, cancellationToken);
        return customer;
    }

    public async Task DeleteAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting customer: {CustomerId}", id.Value);

        await _context.DeleteAsync<Customer>(id.Value, cancellationToken);
    }

    public async Task<bool> ExistsAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        var customer = await GetByIdAsync(id, cancellationToken);
        return customer != null;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var customer = await GetByEmailAsync(email, cancellationToken);
        return customer != null;
    }
}