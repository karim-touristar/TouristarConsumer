namespace TouristarConsumer.Contracts;

public interface IFlightStatusService
{
    public Task FetchAndSaveFlightStatus(long ticketId);
}