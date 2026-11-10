using TalentScout.Models.Entities;
using TalentScout.Models.Enums;

namespace TalentScout.Services.Interfaces;

public interface ICastingService
{
    Task AddCandidateAsync(int projectId, int talentId, string? notes = null, CancellationToken cancellationToken = default);
    Task ChangeStatusAsync(int projectId, int talentId, CastingStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CastingApplication>> GetCandidatesAsync(int projectId, CancellationToken cancellationToken = default);
}
