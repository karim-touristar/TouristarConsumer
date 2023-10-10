using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TouristarConsumer.Contracts;
using TouristarConsumer.Models;
using TouristarModels.Enums;
using TouristarModels.Models;

namespace TouristarConsumer.Services;

public class EmailConsumerService : ConsumerService
{
    private readonly IEmailProcessingService _emailProcessingService;

    public EmailConsumerService(IOptionsMonitor<RabbitConfig> optionsMonitor, ILogger<EmailConsumerService> logger,
        IServiceScopeFactory scopeFactory) : base(Enum.GetName(typeof(Queues), Queues.EmailProcessingQueue)!,
        optionsMonitor.CurrentValue, logger)
    {
        var scope = scopeFactory.CreateScope();
        var emailProcessingService = scope.ServiceProvider.GetRequiredService<IEmailProcessingService>();
        _emailProcessingService = emailProcessingService;
    }

    protected override async Task ProcessMessage(string message)
    {
        var emailMessage = JsonConvert.DeserializeObject<EmailProcessingMessageDto>(message);
        if (emailMessage == null)
        {
            throw new JsonException("Could not deserialise message body.");
        }

        await _emailProcessingService.ProcessEmail(emailMessage);
    }
}