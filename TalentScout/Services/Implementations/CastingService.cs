using Microsoft.EntityFrameworkCore;
using TalentScout.Data;
using TalentScout.Models.Entities;
using TalentScout.Models.Enums;
using TalentScout.Services.Interfaces;

namespace TalentScout.Services.Implementations;

public sealed class CastingService(IDbContextFactory<TalentScoutDbContext> contextFactory) : ICastingService
{
    public async Task AddCandidateAsync(int projectId, int talentId, string? notes = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var projectExists = await context.CastingProjects.AnyAsync(x => x.Id == projectId, cancellationToken);
        var talentExists = await context.Talents.AnyAsync(x => x.Id == talentId, cancellationToken);
        if (!projectExists || !talentExists)
        {
            throw new InvalidOperationException("Проєкт або талант не знайдено.");
        }

        var alreadyAdded = await context.CastingApplications
            .AnyAsync(x => x.CastingProjectId == projectId && x.TalentId == talentId, cancellationToken);
        if (alreadyAdded)
        {
            throw new InvalidOperationException("Цей талант уже доданий до проєкту.");
        }

        context.CastingApplications.Add(new CastingApplication
        {
            CastingProjectId = projectId,
            TalentId = talentId,
            Notes = notes
        });
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeStatusAsync(int projectId, int talentId, CastingStatus status, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var application = await context.CastingApplications.SingleOrDefaultAsync(
            x => x.CastingProjectId == projectId && x.TalentId == talentId,
            cancellationToken) ?? throw new InvalidOperationException("Кандидата не знайдено у цьому проєкті.");

        application.Status = status;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CastingApplication>> GetCandidatesAsync(int projectId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.CastingApplications.AsNoTracking()
            .Where(x => x.CastingProjectId == projectId)
            .Include(x => x.Talent)
                .ThenInclude(x => x.Photos)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.Talent.LastName)
            .ToListAsync(cancellationToken);
    }
}
