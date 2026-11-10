using Microsoft.EntityFrameworkCore;
using TalentScout.Data;
using TalentScout.Models.Entities;
using TalentScout.Repositories.Interfaces;

namespace TalentScout.Repositories.Implementations;

public sealed class ProjectRepository(IDbContextFactory<TalentScoutDbContext> contextFactory) : IProjectRepository
{
    public async Task<IReadOnlyList<CastingProject>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.CastingProjects.AsNoTracking()
            .Include(x => x.Requirement)
                .ThenInclude(x => x!.CustomHairColor)
            .Include(x => x.CastingApplications)
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<CastingProject?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.CastingProjects.AsNoTracking()
            .Include(x => x.Requirement)
                .ThenInclude(x => x!.CustomHairColor)
            .Include(x => x.Requirement)
                .ThenInclude(x => x!.RequiredSkills)
                    .ThenInclude(x => x.Skill)
            .Include(x => x.CastingApplications)
                .ThenInclude(x => x.Talent)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<int> AddAsync(CastingProject project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        project.CreatedUtc = DateTime.UtcNow;

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.CastingProjects.Add(project);
        await context.SaveChangesAsync(cancellationToken);
        return project.Id;
    }

    public async Task UpdateAsync(CastingProject project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.CastingProjects.Update(project);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var project = await context.CastingProjects.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null)
        {
            return;
        }

        context.CastingProjects.Remove(project);
        await context.SaveChangesAsync(cancellationToken);
    }
}
