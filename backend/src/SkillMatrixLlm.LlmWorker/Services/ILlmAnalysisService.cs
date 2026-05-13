namespace SkillMatrixLlm.LlmWorker.Services;

using SkillMatrixLlm.LlmWorker.Models;

public interface ILlmAnalysisService
{
  public Task<SkillRequirementsResult> AnalyseAsync(ProjectDescriptionPayload payload, CancellationToken ct = default);
}
