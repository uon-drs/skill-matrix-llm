namespace SkillMatrixLlm.Api.Enums;

/// <summary>Indicates how a team was assembled for a project.</summary>
public enum ProjectSource
{
  /// <summary>The team was proposed by the LLM.</summary>
  LlmGenerated,
  /// <summary>The team was manually assembled by a project manager.</summary>
  ManuallyAssembled
}
