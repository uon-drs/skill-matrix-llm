using Anthropic;
using Microsoft.Extensions.Options;
using SkillMatrixLlm.LlmWorker;
using SkillMatrixLlm.LlmWorker.Config;
using SkillMatrixLlm.LlmWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

/// Set up LLM Analysis service
builder.Services.AddSingleton(sp =>
  {
    var opts = sp.GetRequiredService<IOptions<WorkerOptions>>().Value;
    return new AnthropicClient() { ApiKey = opts.AnthropicApiKey };
  }
);
builder.Services.AddSingleton(sp => sp.GetRequiredService<AnthropicClient>().Messages);
builder.Services.AddSingleton<ILlmAnalysisService, ClaudeAnalysisService>();

/// Add worker which executes the LLM Analysis service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
