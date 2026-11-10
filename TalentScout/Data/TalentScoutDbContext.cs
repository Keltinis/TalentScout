using Microsoft.EntityFrameworkCore;
using TalentScout.Models.Entities;

namespace TalentScout.Data;

public sealed class TalentScoutDbContext(DbContextOptions<TalentScoutDbContext> options) : DbContext(options)
{
    public DbSet<Talent> Talents => Set<Talent>();
    public DbSet<TalentPhoto> TalentPhotos => Set<TalentPhoto>();
    public DbSet<HairColorOption> HairColorOptions => Set<HairColorOption>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<TalentSkill> TalentSkills => Set<TalentSkill>();
    public DbSet<CastingProject> CastingProjects => Set<CastingProject>();
    public DbSet<ProjectRequirement> ProjectRequirements => Set<ProjectRequirement>();
    public DbSet<RequirementSkill> RequirementSkills => Set<RequirementSkill>();
    public DbSet<CastingApplication> CastingApplications => Set<CastingApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Talent>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(160);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.ClothingSize).HasMaxLength(20);
            entity.Property(x => x.ShoeSize).HasMaxLength(20);
            entity.HasIndex(x => new { x.LastName, x.FirstName });
            entity.HasIndex(x => x.BirthDate);
            entity.HasIndex(x => x.HeightCm);
            entity.HasOne(x => x.CustomHairColor)
                .WithMany(x => x.Talents)
                .HasForeignKey(x => x.CustomHairColorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<HairColorOption>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<TalentPhoto>(entity =>
        {
            entity.Property(x => x.RelativePath).HasMaxLength(260).IsRequired();
            entity.Property(x => x.Caption).HasMaxLength(250);
            entity.HasOne(x => x.Talent)
                .WithMany(x => x.Photos)
                .HasForeignKey(x => x.TalentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<TalentSkill>(entity =>
        {
            entity.HasKey(x => new { x.TalentId, x.SkillId });
            entity.Property(x => x.Notes).HasMaxLength(250);
            entity.HasOne(x => x.Talent)
                .WithMany(x => x.TalentSkills)
                .HasForeignKey(x => x.TalentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Skill)
                .WithMany(x => x.TalentSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CastingProject>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Client).HasMaxLength(150);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<ProjectRequirement>(entity =>
        {
            entity.HasOne(x => x.CastingProject)
                .WithOne(x => x.Requirement)
                .HasForeignKey<ProjectRequirement>(x => x.CastingProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CustomHairColor)
                .WithMany(x => x.ProjectRequirements)
                .HasForeignKey(x => x.CustomHairColorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RequirementSkill>(entity =>
        {
            entity.HasKey(x => new { x.ProjectRequirementId, x.SkillId });
            entity.HasOne(x => x.ProjectRequirement)
                .WithMany(x => x.RequiredSkills)
                .HasForeignKey(x => x.ProjectRequirementId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Skill)
                .WithMany(x => x.RequirementSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CastingApplication>(entity =>
        {
            entity.HasIndex(x => new { x.CastingProjectId, x.TalentId }).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.CastingProject)
                .WithMany(x => x.CastingApplications)
                .HasForeignKey(x => x.CastingProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Talent)
                .WithMany(x => x.CastingApplications)
                .HasForeignKey(x => x.TalentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
