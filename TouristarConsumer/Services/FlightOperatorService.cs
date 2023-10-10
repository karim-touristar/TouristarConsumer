using Microsoft.Extensions.Options;
using TouristarConsumer.Contracts;
using TouristarConsumer.Models;
using TouristarModels.Models;

namespace TouristarConsumer.Services;

public class FlightOperatorService : IFlightOperatorService
{
    private readonly string _airlineFetchingBaseUrl;
    private readonly IRepositoryManager _repository;

    public FlightOperatorService(IRepositoryManager repository, IOptionsMonitor<LogoFetchingConfig> optionsMonitor)
    {
        _repository = repository;
        _airlineFetchingBaseUrl = optionsMonitor.CurrentValue.AirlineLogoBaseUrl;
    }

    public async Task<FlightOperator?> FindOrCreateOperator(string name, string code)
    {
        var flightOperator = _repository.FlightOperator.FindOperatorByName(name);
        if (flightOperator != null) return flightOperator;

        // Create new operator in db.
        var airlineLogoUrl = await GetAirlineLogoUrl(code);
        FlightOperator newOperator = new()
        {
            Name = name,
            CarrierCode = code,
            LogoUrl = airlineLogoUrl,
        };
        _repository.FlightOperator.CreateOperator(newOperator);
        await _repository.Save();
        return _repository.FlightOperator.FindOperatorById(newOperator.Id);
    }

    private async Task<string?> GetAirlineLogoUrl(string code)
    {
        var airlineLogoUrl = $"{_airlineFetchingBaseUrl}/{code}.svg";
        HttpClient client = new();
        var response = await client.GetAsync(airlineLogoUrl);
        return (await response.Content.ReadAsStringAsync()).ToLower().Contains("not found") ? null : airlineLogoUrl;
    }
}