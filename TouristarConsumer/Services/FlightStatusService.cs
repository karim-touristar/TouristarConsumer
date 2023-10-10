using Microsoft.Extensions.Options;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;
using TouristarConsumer.Contracts;
using TouristarConsumer.Models;
using TouristarConsumer.Utils;
using DbModels = TouristarModels.Models;

namespace TouristarConsumer.Services;

public class FlightStatusService : IFlightStatusService
{
    private readonly IRepositoryManager _repository;
    private readonly ILogger<FlightStatusService> _logger;
    private readonly string _appId;
    private readonly string _appKey;

    public FlightStatusService(IRepositoryManager repository, ILogger<FlightStatusService> logger,
        IOptionsMonitor<CiriumConfig> optionsMonitor)
    {
        _repository = repository;
        _logger = logger;
        _appId = optionsMonitor.CurrentValue.AppId;
        _appKey = optionsMonitor.CurrentValue.AppKey;
    }

    public async Task FetchAndSaveFlightStatus(long ticketId)
    {
        try
        {
            _logger.LogInformation($"About to fetch flight status info, ticket id: {ticketId}.");
            var ticket = _repository.Ticket.FindTicket(ticketId);
            if (ticket.FlightOperator == null)
            {
                _logger.LogError($"Missing FlightOperator object on ticket id: {ticket.Id}, trip id: {ticket.TripId}.");
                return;
            }

            if (ticket.FlightNumber == null)
            {
                _logger.LogError(
                    $"Missing FlightNumber parameter on ticket id: {ticket.Id}, trip id: {ticket.TripId}.");
                return;
            }

            if (ticket.ArriveAt == null)
            {
                _logger.LogWarning(
                    $"Missing ArriveAt parameter on ticket id: {ticket.Id}, trip id: {ticket.TripId}. DepartAt will be used instead for flight status calculation.");
            }

            Configuration config = new Configuration();
            config.ApiKey.Add("appId", _appId);
            config.ApiKey.Add("appKey", _appKey);

            var relevantDate = ticket.ArriveAt ?? ticket.DepartAt;
            var client = new ByFlightApi(config);

            CurrentFlightStatusResponse response = await client.ByFlightByArrivalAsync(
                "json", ticket.FlightOperator.CarrierCode, ticket.FlightNumber, relevantDate.Year.ToString(),
                relevantDate.Month,
                relevantDate.Day);

            if (response.FlightStatuses == null || !response.FlightStatuses.Any())
            {
                _logger.LogError(
                    $"Missing flight statuses in response for ticket id: {ticket.Id}, trip id: {ticket.TripId}.");
                return;
            }

            var flightStatus = FlightStatusMapper.MapToDbFlightStatus(response.FlightStatuses.First(), ticketId);
            _repository.FlightStatus.CreateFlightStatus(flightStatus);
            await _repository.Save();

            _logger.LogInformation($"Completed flight status fetching for ticket id: {ticket.Id}.");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }
}