using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IO;
using TalentScout.Models.Entities;
using TalentScout.Models.Enums;

namespace TalentScout.Data;

public sealed class DatabaseInitializer(IDbContextFactory<TalentScoutDbContext> contextFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureAppearanceSchemaAsync(context, cancellationToken);

        if (!await context.Skills.AnyAsync(cancellationToken))
        {
            context.Skills.AddRange(
                new Skill { Name = "Акторська майстерність", Category = SkillCategory.Acting }, new Skill { Name = "Вокал", Category = SkillCategory.Vocal },
                new Skill { Name = "Верхова їзда", Category = SkillCategory.Sport }, new Skill { Name = "Катання на роликах", Category = SkillCategory.Sport },
                new Skill { Name = "Танці", Category = SkillCategory.Dance }, new Skill { Name = "Англійська мова", Category = SkillCategory.Language },
                new Skill { Name = "Французька мова", Category = SkillCategory.Language });
            await context.SaveChangesAsync(cancellationToken);
        }

        await SeedDemoDataAsync(context, cancellationToken);
    }

    private static async Task SeedDemoDataAsync(TalentScoutDbContext context, CancellationToken cancellationToken)
    {
        var names = new[] { ("Анна", "Мельник"), ("Софія", "Левченко"), ("Марія", "Ткаченко"), ("Ірина", "Романюк"), ("Олена", "Гнатюк"), ("Денис", "Коваль"), ("Максим", "Бондар"), ("Артем", "Савчук"), ("Катерина", "Шевченко"), ("Вікторія", "Лисенко") };
        var missingTalents = Math.Max(0, 10 - await context.Talents.CountAsync(cancellationToken));
        for (var index = 0; index < missingTalents; index++)
        {
            var name = names[index];
            context.Talents.Add(new Talent { FirstName = name.Item1, LastName = name.Item2, BirthDate = new DateOnly(1998 + index % 6, 3 + index % 8, 12), Gender = index is 5 or 6 or 7 ? Gender.Male : Gender.Female, City = index % 2 == 0 ? "Київ" : "Львів", HeightCm = 168 + index * 2, HairColor = index % 3 == 0 ? HairColor.Blonde : HairColor.Brunette, EyeColor = EyeColor.Brown });
        }
        await context.SaveChangesAsync(cancellationToken);

        var talentsWithoutPhoto = await context.Talents.Include(x => x.Photos).Where(x => !x.Photos.Any()).ToListAsync(cancellationToken);
        foreach (var talent in talentsWithoutPhoto)
        {
            talent.Photos.Add(new TalentPhoto { RelativePath = Path.Combine("Assets", "DemoPhotos", "demo-portrait.png"), IsPrimary = true, SortOrder = 0 });
        }
        if (talentsWithoutPhoto.Count > 0) await context.SaveChangesAsync(cancellationToken);

        var titles = new[] { "Реклама соку", "Кіно «Тиша»", "Кампанія косметики", "Музичний кліп", "Каталог одягу" };
        var missingProjects = Math.Max(0, 5 - await context.CastingProjects.CountAsync(cancellationToken));
        for (var index = 0; index < missingProjects; index++)
        {
            context.CastingProjects.Add(new CastingProject { Title = titles[index], Client = "TalentScout Demo", City = "Київ", Status = ProjectStatus.Active, Requirement = new ProjectRequirement { MinimumAge = 20, MaximumAge = 35, MinimumHeightCm = 165 } });
        }
        await context.SaveChangesAsync(cancellationToken);

        if (!await context.CastingApplications.AnyAsync(cancellationToken))
        {
            var talents = await context.Talents.OrderBy(x => x.Id).Take(10).ToListAsync(cancellationToken);
            var projects = await context.CastingProjects.OrderBy(x => x.Id).Take(5).ToListAsync(cancellationToken);
            for (var index = 0; index < talents.Count; index++) context.CastingApplications.Add(new CastingApplication { TalentId = talents[index].Id, CastingProjectId = projects[index % projects.Count].Id, Status = (CastingStatus)(index % 4) });
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task EnsureAppearanceSchemaAsync(TalentScoutDbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync(
            "CREATE TABLE IF NOT EXISTS HairColorOptions (Id INTEGER NOT NULL CONSTRAINT PK_HairColorOptions PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL);",
            cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_HairColorOptions_Name ON HairColorOptions (Name);",
            cancellationToken);

        if (!await ColumnExistsAsync(context, "Talents", "CustomHairColorId", cancellationToken))
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE Talents ADD COLUMN CustomHairColorId INTEGER NULL;", cancellationToken);
        }

        if (!await ColumnExistsAsync(context, "ProjectRequirements", "CustomHairColorId", cancellationToken))
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE ProjectRequirements ADD COLUMN CustomHairColorId INTEGER NULL;", cancellationToken);
        }
    }

    private static async Task<bool> ColumnExistsAsync(
        TalentScoutDbContext context,
        string tableName,
        string columnName,
        CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var mustClose = connection.State != ConnectionState.Open;
        if (mustClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info([{tableName}]);";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
        finally
        {
            if (mustClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
