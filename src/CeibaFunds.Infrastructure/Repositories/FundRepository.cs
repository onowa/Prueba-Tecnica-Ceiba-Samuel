using Amazon.DynamoDBv2.DataModel;
using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CeibaFunds.Infrastructure.Repositories;

public class FundRepository : IFundRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<FundRepository> _logger;

    public FundRepository(IDynamoDBContext context, ILogger<FundRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Fund?> GetByIdAsync(FundId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving fund by ID: {FundId}", id.Value);
        return await _context.LoadAsync<Fund>(id.Value, cancellationToken);
    }

    public async Task<IEnumerable<Fund>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving all funds");
        var search = _context.ScanAsync<Fund>(new List<Amazon.DynamoDBv2.DataModel.ScanCondition>());
        return await search.GetRemainingAsync(cancellationToken);
    }

    public async Task<IEnumerable<Fund>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving active funds");
        var allFunds = await GetAllAsync(cancellationToken);
        return allFunds.Where(f => f.IsActive);
    }

    public async Task<Fund> CreateAsync(Fund fund, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating fund: {FundId}", fund.Id.Value);
        await _context.SaveAsync(fund, cancellationToken);
        return fund;
    }

    public async Task<Fund> UpdateAsync(Fund fund, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating fund: {FundId}", fund.Id.Value);
        await _context.SaveAsync(fund, cancellationToken);
        return fund;
    }

    public async Task DeleteAsync(FundId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting fund: {FundId}", id.Value);
        await _context.DeleteAsync<Fund>(id.Value, cancellationToken);
    }

    public async Task<bool> ExistsAsync(FundId id, CancellationToken cancellationToken = default)
    {
        var fund = await GetByIdAsync(id, cancellationToken);
        return fund != null;
    }
}