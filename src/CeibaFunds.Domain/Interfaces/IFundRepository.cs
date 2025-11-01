using CeibaFunds.Domain.Entities;
using CeibaFunds.Domain.ValueObjects;

namespace CeibaFunds.Domain.Interfaces;

public interface IFundRepository
{
    Task<Fund?> GetByIdAsync(FundId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Fund>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Fund>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Fund> CreateAsync(Fund fund, CancellationToken cancellationToken = default);
    Task<Fund> UpdateAsync(Fund fund, CancellationToken cancellationToken = default);
    Task DeleteAsync(FundId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(FundId id, CancellationToken cancellationToken = default);
}