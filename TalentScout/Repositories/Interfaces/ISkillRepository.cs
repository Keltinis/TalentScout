using TalentScout.Models.Entities;

namespace TalentScout.Repositories.Interfaces;

public interface ISkillRepository
{
    Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Skill?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Skill skill, CancellationToken cancellationToken = default);
}
