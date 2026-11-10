using TalentScout.Models.Dtos;
using TalentScout.Models.Entities;

namespace TalentScout.Services.Interfaces;

public interface ITalentSearchService
{
    Task<IReadOnlyList<Talent>> SearchAsync(TalentSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Talent>> SearchForProjectAsync(int projectId, CancellationToken cancellationToken = default);
}
