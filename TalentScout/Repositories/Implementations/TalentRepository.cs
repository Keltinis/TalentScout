using Microsoft.EntityFrameworkCore;
using TalentScout.Data;
using TalentScout.Models.Entities;
using TalentScout.Repositories.Interfaces;

namespace TalentScout.Repositories.Implementations;

public sealed class TalentRepository(IDbContextFactory<TalentScoutDbContext> contextFactory) : ITalentRepository
{
    public async Task<IReadOnlyList<Talent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Talents
            .AsNoTracking()
            .Include(x => x.Photos)
            .Include(x => x.CustomHairColor)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Talent?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Talents
            .AsNoTracking()
            .Include(x => x.Photos)
            .Include(x => x.CustomHairColor)
            .Include(x => x.TalentSkills)
                .ThenInclude(x => x.Skill)
            .Include(x => x.CastingApplications)
                .ThenInclude(x => x.CastingProject)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<int> AddAsync(Talent talent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(talent);
        if (talent.Photos.Count == 0 || talent.Photos.Any(photo => string.IsNullOrWhiteSpace(photo.RelativePath)))
        {
            throw new InvalidOperationException("Для нової анкети потрібно додати щонайменше одну фотографію.");
        }
        talent.CreatedUtc = DateTime.UtcNow;
        talent.UpdatedUtc = talent.CreatedUtc;

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Talents.Add(talent);
        await context.SaveChangesAsync(cancellationToken);
        return talent.Id;
    }

    public async Task UpdateAsync(Talent talent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(talent);
        talent.UpdatedUtc = DateTime.UtcNow;

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Talents.Update(talent);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var talent = await context.Talents.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (talent is null)
        {
            return;
        }

        context.Talents.Remove(talent);
        await context.SaveChangesAsync(cancellationToken);
    }
}
