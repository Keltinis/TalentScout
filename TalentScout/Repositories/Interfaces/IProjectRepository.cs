using TalentScout.Models.Entities;

namespace TalentScout.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<IReadOnlyList<CastingProject>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CastingProject?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(CastingProject project, CancellationToken cancellationToken = default);
    Task UpdateAsync(CastingProject project, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
