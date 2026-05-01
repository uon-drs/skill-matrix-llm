namespace SkillMatrixLlm.Api.Config;

/// <summary>Configuration for Azure Storage Queue message queues.</summary>
public record MessageQueueOptions
{
    /// <summary>Azure Storage connection string (or "UseDevelopmentStorage=true" for Azurite).</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Queue name for outbound <c>ProjectDescriptionPayload</c> messages.</summary>
    public string ProjectDescriptionQueueName { get; init; } = "project-description";

    /// <summary>Queue name for inbound <c>SkillRequirementsResult</c> messages from the LLM service.</summary>
    public string SkillRequirementsQueueName { get; init; } = "skill-requirements";
}
