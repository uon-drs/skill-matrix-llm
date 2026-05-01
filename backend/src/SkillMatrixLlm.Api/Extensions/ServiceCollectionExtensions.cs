namespace SkillMatrixLlm.Api.Extensions;

using Azure.Storage.Queues;
using Config;
using Messaging;
using Models.Recommendations;
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
  /// Registers <see cref="IMessageQueue{T}"/> implementations backed by Azure Queue Storage.
  /// </summary>
  public static IServiceCollection AddMessageQueues(this IServiceCollection s, IConfiguration c)
  {
    var options = c.GetSection("MessageQueue").Get<MessageQueueOptions>() ?? new MessageQueueOptions();

    s.AddSingleton<IMessageQueue<ProjectDescriptionPayload>>(
      new AzureStorageQueueMessageQueue<ProjectDescriptionPayload>(
        CreateQueueClient(options.ConnectionString, options.ProjectDescriptionQueueName)));

    s.AddSingleton<IMessageQueue<SkillRequirementsResult>>(
      new AzureStorageQueueMessageQueue<SkillRequirementsResult>(
        CreateQueueClient(options.ConnectionString, options.SkillRequirementsQueueName)));

    return s;
  }

  private static QueueClient CreateQueueClient(string connectionString, string queueName) =>
    new(connectionString, queueName, new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
}
