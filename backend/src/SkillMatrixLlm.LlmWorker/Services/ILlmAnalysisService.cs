namespace SkillMatrixLlm.LlmWorker.Services;

using SkillMatrixLlm.LlmWorker.Models;

/// <summary>Analyses a project description and returns the roles and skills required to staff it.</summary>
public interface ILlmAnalysisService
{
  /// <summary>Runs LLM analysis on <paramref name="payload"/> and returns structured skill requirements.</summary>
  /// <param name="payload">The project description to analyse.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A <see cref="SkillRequirementsResult"/> containing parsed roles and the raw LLM response.</returns>
  public Task<SkillRequirementsResult> AnalyseAsync(ProjectDescriptionPayload payload, CancellationToken ct = default);
}
