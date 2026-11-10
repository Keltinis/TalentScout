using Microsoft.EntityFrameworkCore;
using TalentScout.Data;
using TalentScout.Models.Entities;
using TalentScout.Repositories.Interfaces;

namespace TalentScout.Repositories.Implementations;

public sealed class HairColorOptionRepository(IDbContextFactory<TalentScoutDbContext> contextFactory) : IHairColorOptionRepository
{
    public async Task<IReadOnlyList<HairColorOption>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.HairColorOptions.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<int> AddAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Вкажіть назву кольору волосся.", nameof(name));
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        if (await context.HairColorOptions.AnyAsync(x => x.Name == normalizedName, cancellationToken))
        {
            throw new InvalidOperationException("Такий колір волосся вже є у довіднику.");
        }

        var option = new HairColorOption { Name = normalizedName };
        context.HairColorOptions.Add(option);
        await context.SaveChangesAsync(cancellationToken);
        return option.Id;
    }
}
