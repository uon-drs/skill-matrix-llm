namespace SkillMatrixLlm.Api.Tests;

using Data;
using Data.Entities;
using Enums;
using Microsoft.EntityFrameworkCore;
using Models.Recommendations;
using Services;
using Xunit;

public class StaffMatchingServiceTests
{
  private static AppDbContext CreateDb() =>
    new(new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options);

  private static User SeedUser(AppDbContext db, string name = "User")
  {
    var user = new User
    {
      KeycloakId = Guid.NewGuid().ToString(),
      DisplayName = name,
      Email = $"{name.ToLowerInvariant()}@test.com",
    };
    db.Users.Add(user);
    db.SaveChanges();
    return user;
  }

  private static Skill SeedSkill(AppDbContext db, string name)
  {
    var skill = new Skill { Name = name };
    db.Skills.Add(skill);
    db.SaveChanges();
    return skill;
  }

  private static void SeedUserSkill(AppDbContext db, Guid userId, Guid skillId)
  {
    db.UserSkills.Add(new UserSkill { UserId = userId, SkillId = skillId, Level = Level.Intermediate });
    db.SaveChanges();
  }

  // -------------------------------------------------------------------------
  // MatchAsync
  // -------------------------------------------------------------------------

  [Fact]
  public async Task MatchAsync_ReturnsEmpty_WhenNoUsersExist()
  {
    using var db = CreateDb();
    var service = new StaffMatchingService(db);

    var result = await service.MatchAsync([new RoleRequirement("Dev", ["Python"])]);

    Assert.Empty(result);
  }

  [Fact]
  public async Task MatchAsync_AssignsUserWithMostMatchingSkills()
  {
    using var db = CreateDb();
    var alice = SeedUser(db, "Alice");
    var bob = SeedUser(db, "Bob");
    var python = SeedSkill(db, "Python");
    var js = SeedSkill(db, "JavaScript");
    SeedUserSkill(db, alice.Id, python.Id);
    SeedUserSkill(db, bob.Id, python.Id);
    SeedUserSkill(db, bob.Id, js.Id);

    var service = new StaffMatchingService(db);

    // Bob has both Python and JavaScript; Alice only has Python.
    // The role requires JavaScript, so Bob should be chosen.
    var result = await service.MatchAsync([new RoleRequirement("Frontend Dev", ["JavaScript"])]);

    Assert.Single(result);
    Assert.Equal(bob.Id, result[0].UserId);
    Assert.Equal("Frontend Dev", result[0].RoleName);
  }

  [Fact]
  public async Task MatchAsync_AssignsEachUserOnce_WhenRolesAreDistinct()
  {
    using var db = CreateDb();
    var alice = SeedUser(db, "Alice");
    var bob = SeedUser(db, "Bob");
    var python = SeedSkill(db, "Python");
    var js = SeedSkill(db, "JavaScript");
    SeedUserSkill(db, alice.Id, python.Id);
    SeedUserSkill(db, bob.Id, js.Id);

    var service = new StaffMatchingService(db);

    var result = await service.MatchAsync([
      new RoleRequirement("Backend Dev", ["Python"]),
      new RoleRequirement("Frontend Dev", ["JavaScript"]),
    ]);

    Assert.Equal(2, result.Count);
    Assert.Equal(alice.Id, result[0].UserId);
    Assert.Equal("Backend Dev", result[0].RoleName);
    Assert.Equal(bob.Id, result[1].UserId);
    Assert.Equal("Frontend Dev", result[1].RoleName);
  }

  [Fact]
  public async Task MatchAsync_StopsAssigning_WhenCandidatePoolIsExhausted()
  {
    using var db = CreateDb();
    var user = SeedUser(db);
    var python = SeedSkill(db, "Python");
    SeedUserSkill(db, user.Id, python.Id);

    var service = new StaffMatchingService(db);

    // Two roles, only one candidate — second role gets no assignment.
    var result = await service.MatchAsync([
      new RoleRequirement("Role A", ["Python"]),
      new RoleRequirement("Role B", ["Python"]),
    ]);

    Assert.Single(result);
    Assert.Equal(user.Id, result[0].UserId);
  }

  [Fact]
  public async Task MatchAsync_MatchesSkillsCaseInsensitively()
  {
    using var db = CreateDb();
    var user = SeedUser(db);
    var python = SeedSkill(db, "Python");
    SeedUserSkill(db, user.Id, python.Id);

    var service = new StaffMatchingService(db);

    // LLM returns "python" (lowercase); DB stores "Python".
    var result = await service.MatchAsync([new RoleRequirement("Dev", ["python"])]);

    Assert.Single(result);
    Assert.Equal(user.Id, result[0].UserId);
  }
}
