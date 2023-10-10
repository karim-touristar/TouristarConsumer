using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TouristarConsumer.Contracts;
using TouristarConsumer.Models;
using TouristarModels.Enums;
using TouristarModels.Models;

namespace TouristarConsumer.Services;

public class FlightStatusConsumerService : ConsumerService
{
    private readonly IFlightStatusService _flightStatusService;

    public FlightStatusConsumerService(IOptionsMonitor<RabbitConfig> optionsMonitor,
        ILogger<FlightStatusConsumerService> logger, IServiceScopeFactory scopeFactory) : base(
        Enum.GetName(typeof(Queues), Queues.FlightStatusQueue)!, optionsMonitor.CurrentValue, logger)
    {
        var scope = scopeFactory.CreateScope();
        var flightStatusService = scope.ServiceProvider.GetRequiredService<IFlightStatusService>();
        _flightStatusService = flightStatusService;
    }

    protected override async Task ProcessMessage(string message)
    {
        var data = JsonConvert.DeserializeObject<FlightStatusMessageDto>(message);
        if (data == null)
        {
            throw new JsonException("Could not deserialise message body.");
        }

        await _flightStatusService.FetchAndSaveFlightStatus(data.TicketId);
    }
}