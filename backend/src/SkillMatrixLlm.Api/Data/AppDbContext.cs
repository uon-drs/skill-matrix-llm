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

  /// <summary>Projects requiring team assembly.</summary>
  public DbSet<Project> Projects => Set<Project>();

  /// <summary>Proposed or confirmed teams for projects.</summary>
  public DbSet<Team> Teams => Set<Team>();

  /// <summary>Team membership records linking users to teams.</summary>
  public DbSet<TeamMembership> TeamMemberships => Set<TeamMembership>();

  /// <summary>Raw LLM team recommendation audit records.</summary>
  public DbSet<Recommendation> Recommendations => Set<Recommendation>();

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

    modelBuilder.Entity<Project>(e =>
    {
      e.HasKey(p => p.Id);
      e.Property(p => p.Title).IsRequired().HasMaxLength(256);
      e.Property(p => p.Description).IsRequired();
      e.Property(p => p.Timeline).IsRequired().HasMaxLength(256);
      e.Property(p => p.CreatedAt).IsRequired();
      e.HasOne(p => p.CreatedByUser).WithMany().HasForeignKey(p => p.CreatedByUserId);
    });

    modelBuilder.Entity<Team>(e =>
    {
      e.HasKey(t => t.Id);
      e.Property(t => t.CreatedAt).IsRequired();
      e.HasOne(t => t.Project).WithMany().HasForeignKey(t => t.ProjectId);
    });

    modelBuilder.Entity<TeamMembership>(e =>
    {
      e.HasKey(tm => tm.Id);
      e.Property(tm => tm.ProjectRole).IsRequired().HasMaxLength(256);
      e.HasOne(tm => tm.Team).WithMany().HasForeignKey(tm => tm.TeamId);
      e.HasOne(tm => tm.User).WithMany().HasForeignKey(tm => tm.UserId);
    });

    modelBuilder.Entity<Recommendation>(e =>
    {
      e.HasKey(r => r.Id);
      e.Property(r => r.RawResponse).IsRequired().HasColumnType("jsonb");
      e.Property(r => r.CreatedAt).IsRequired();
      e.HasOne(r => r.Project).WithMany().HasForeignKey(r => r.ProjectId);
      e.HasOne(r => r.Team).WithMany().HasForeignKey(r => r.TeamId);
    });
  }
}
