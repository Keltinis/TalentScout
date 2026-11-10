using TalentScout.Models.Entities;

namespace TalentScout.Repositories.Interfaces;

public interface ITalentRepository
{
    Task<IReadOnlyList<Talent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Talent?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Talent talent, CancellationToken cancellationToken = default);
    Task UpdateAsync(Talent talent, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
