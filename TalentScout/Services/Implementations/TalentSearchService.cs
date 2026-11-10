using Microsoft.EntityFrameworkCore;
using TalentScout.Data;
using TalentScout.Models.Dtos;
using TalentScout.Models.Entities;
using TalentScout.Models.Enums;
using TalentScout.Repositories.Interfaces;
using TalentScout.Services.Interfaces;

namespace TalentScout.Services.Implementations;

public sealed class TalentSearchService(
    IDbContextFactory<TalentScoutDbContext> contextFactory,
    IProjectRepository projectRepository) : ITalentSearchService
{
    public async Task<IReadOnlyList<Talent>> SearchAsync(
        TalentSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<Talent> query = context.Talents
            .AsNoTracking()
            .Include(x => x.Photos)
            .Include(x => x.TalentSkills)
                .ThenInclude(x => x.Skill);

        if (!string.IsNullOrWhiteSpace(criteria.Text))
        {
            var term = criteria.Text.Trim();
            query = query.Where(x =>
                x.FirstName.Contains(term) ||
                x.LastName.Contains(term) ||
                (x.City != null && x.City.Contains(term)) ||
                (x.Email != null && x.Email.Contains(term)));
        }

        if (criteria.Gender is not null)
        {
            query = query.Where(x => x.Gender == criteria.Gender);
        }

        if (criteria.MinimumHeightCm is not null)
        {
            query = query.Where(x => x.HeightCm >= criteria.MinimumHeightCm);
        }

        if (criteria.MaximumHeightCm is not null)
        {
            query = query.Where(x => x.HeightCm <= criteria.MaximumHeightCm);
        }

        if (criteria.EyeColor is not null)
        {
            query = query.Where(x => x.EyeColor == criteria.EyeColor);
        }

        if (criteria.HairColor is not null)
        {
            query = query.Where(x => x.HairColor == criteria.HairColor);
        }

        if (criteria.CustomHairColorId is not null)
        {
            query = query.Where(x => x.CustomHairColorId == criteria.CustomHairColorId);
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (criteria.MinimumAge is not null)
        {
            var latestBirthDate = today.AddYears(-criteria.MinimumAge.Value);
            query = query.Where(x => x.BirthDate <= latestBirthDate);
        }

        if (criteria.MaximumAge is not null)
        {
            var earliestBirthDate = today.AddYears(-(criteria.MaximumAge.Value + 1)).AddDays(1);
            query = query.Where(x => x.BirthDate >= earliestBirthDate);
        }

        var skillIds = criteria.SkillIds.Distinct().ToArray();
        if (skillIds.Length > 0)
        {
            query = criteria.SkillMatchMode == SkillMatchMode.All
                ? query.Where(x => x.TalentSkills.Count(ts => skillIds.Contains(ts.SkillId)) == skillIds.Length)
                : query.Where(x => x.TalentSkills.Any(ts => skillIds.Contains(ts.SkillId)));
        }

        return await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Talent>> SearchForProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetDetailsAsync(projectId, cancellationToken)
            ?? throw new InvalidOperationException("Кастинг-проєкт не знайдено.");
        var requirement = project.Requirement;

        if (requirement is null)
        {
            return await SearchAsync(new TalentSearchCriteria(), cancellationToken);
        }

        return await SearchAsync(new TalentSearchCriteria
        {
            Gender = requirement.RequiredGender,
            MinimumAge = requirement.MinimumAge,
            MaximumAge = requirement.MaximumAge,
            MinimumHeightCm = requirement.MinimumHeightCm,
            MaximumHeightCm = requirement.MaximumHeightCm,
            EyeColor = requirement.RequiredEyeColor,
            HairColor = requirement.RequiredHairColor,
            CustomHairColorId = requirement.CustomHairColorId,
            SkillMatchMode = requirement.RequiredSkillsMatchMode,
            SkillIds = requirement.RequiredSkills.Select(x => x.SkillId).ToArray()
        }, cancellationToken);
    }
}
