using TouristarModels.Contracts;

namespace TouristarConsumer.Contracts;

public interface IRepositoryManager
{
    IGptRepository Gpt { get; }
    ITripRepository Trip { get; }
    ITicketRepository Ticket { get; }
    ILocationRepository Location { get; }
    IFlightOperatorRepository FlightOperator { get; }
    IFlightStatusRepository FlightStatus { get; }
    IUserRepository User { get; }
    IMessagingRepository Messaging { get; }
    IRadarRepository Radar { get; }
    Task Save();
}
