using TalentScout.Models.Entities;

namespace TalentScout.Repositories.Interfaces;

public interface IHairColorOptionRepository
{
    Task<IReadOnlyList<HairColorOption>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> AddAsync(string name, CancellationToken cancellationToken = default);
}
