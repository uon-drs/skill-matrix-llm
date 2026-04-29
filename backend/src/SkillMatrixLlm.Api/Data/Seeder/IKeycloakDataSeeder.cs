namespace SkillMatrixLlm.Api.Data.Seeder;

/// <summary>Seeds Keycloak with the groups, roles, and role-to-group mappings required by the application.</summary>
public interface IKeycloakDataSeeder
{
  /// <summary>Runs all seed operations idempotently.</summary>
  Task SeedKeycloakData();
}
