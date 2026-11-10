using Microsoft.EntityFrameworkCore;
using TalentScout.Data;
using TalentScout.Models.Entities;
using TalentScout.Repositories.Interfaces;

namespace TalentScout.Repositories.Implementations;

public sealed class SkillRepository(IDbContextFactory<TalentScoutDbContext> contextFactory) : ISkillRepository
{
    public async Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Skills.AsNoTracking()
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Skill?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Skills.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<int> AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(skill);
        skill.Name = skill.Name.Trim();
        if (string.IsNullOrWhiteSpace(skill.Name))
        {
            throw new ArgumentException("Вкажіть назву навички.", nameof(skill));
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        if (await context.Skills.AnyAsync(x => x.Name == skill.Name, cancellationToken))
        {
            throw new InvalidOperationException("Така навичка вже є у довіднику.");
        }

        context.Skills.Add(skill);
        await context.SaveChangesAsync(cancellationToken);
        return skill.Id;
    }
}
