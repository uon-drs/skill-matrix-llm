namespace SkillMatrixLlm.Api.Extensions;

using Azure.Storage.Queues;
using Config;
using Messaging;
using Models.Recommendations;
using Services;
using Services.Contracts;
using Services.EmailSender;
using Services.EmailServices;

public static class ServiceCollectionExtensions
{

  public static IServiceCollection AddEmailSender(this IServiceCollection s, IConfiguration c)
  {

    var emailProvider = c["OutboundEmail:Provider"] ?? string.Empty;

    var outboundProvider = emailProvider.ToLowerInvariant();

    switch (outboundProvider)
    {
      case "sendgrid":
        s.Configure<SendGridOptions>(c.GetSection("OutboundEmail"));
        s.AddTransient<IEmailSender, SendGridEmailSender>();
        break;

      case "smtp":
        s.Configure<SmtpOptions>(c.GetSection("OutboundEmail"));
        s.AddTransient<IEmailSender, SmtpEmailSender>();
        break;

      default:
        s.Configure<LocalDiskEmailOptions>(c.GetSection("OutboundEmail"));
        s.AddTransient<IEmailSender, LocalDiskEmailSender>();
        break;
    }

    s
      .AddTransient<RazorViewService>()
      .AddTransient<HealthEmailService>();

    return s;
  }

  /// <summary>
  /// Registers <see cref="IMessageChannel{T}"/> implementations backed by Azure Queue Storage
  /// and a hosted service that creates the queues on startup if they do not already exist.
  /// </summary>
  public static IServiceCollection AddMessageQueues(this IServiceCollection s, IConfiguration c)
  {
    var options = c.GetSection("MessageQueue").Get<MessageQueueOptions>() ?? new MessageQueueOptions();

    var projectDescClient = CreateQueueClient(options.ConnectionString, options.ProjectDescriptionQueueName);
    var skillReqClient = CreateQueueClient(options.ConnectionString, options.SkillRequirementsQueueName);

    s.AddSingleton<IMessageChannel<ProjectDescriptionPayload>>(_ =>
      new AzureStorageQueueMessageChannel<ProjectDescriptionPayload>(projectDescClient));

    s.AddSingleton<IMessageChannel<SkillRequirementsResult>>(_ =>
      new AzureStorageQueueMessageChannel<SkillRequirementsResult>(skillReqClient));

    s.AddHostedService(_ => new QueueInitializerService([projectDescClient, skillReqClient]));

    return s;
  }

  private static QueueClient CreateQueueClient(string connectionString, string queueName) =>
    new(connectionString, queueName, new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
}
