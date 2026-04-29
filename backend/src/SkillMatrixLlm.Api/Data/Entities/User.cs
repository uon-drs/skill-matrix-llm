namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

public class User
{
  public Guid Id { get; set; }

  public required string KeycloakId { get; set; }

  public required string DisplayName { get; set; }

  public required string Email { get; set; }

  public Role Role { get; set; }
}
