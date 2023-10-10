using TouristarModels.Models;

namespace TouristarConsumer.Contracts;

public interface IFlightOperatorService
{
    public Task<FlightOperator?> FindOrCreateOperator(string name, string code);
}