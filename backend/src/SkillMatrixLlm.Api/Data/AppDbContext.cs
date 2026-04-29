using Microsoft.EntityFrameworkCore;
using SkillMatrixLlm.Api.Data.Entities;

namespace SkillMatrixLlm.Api.Data;

/// <summary>EF Core database context for the application.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Application users.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Skill catalogue.</summary>
    public DbSet<Skill> Skills => Set<Skill>();

    /// <summary>User skill proficiency records.</summary>
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.KeycloakId).IsUnique();
            e.Property(u => u.KeycloakId).IsRequired().HasMaxLength(256);
            e.Property(u => u.DisplayName).IsRequired().HasMaxLength(256);
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
        });

        modelBuilder.Entity<Skill>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(256);
        });

        modelBuilder.Entity<UserSkill>(e =>
        {
            e.HasKey(us => us.Id);
            e.HasIndex(us => new { us.UserId, us.SkillId }).IsUnique();
            e.HasOne(us => us.User).WithMany().HasForeignKey(us => us.UserId);
            e.HasOne(us => us.Skill).WithMany().HasForeignKey(us => us.SkillId);
        });
    }
}
