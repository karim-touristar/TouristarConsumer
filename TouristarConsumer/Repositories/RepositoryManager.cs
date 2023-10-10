using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using TouristarConsumer.Contracts;
using TouristarConsumer.Models;
using TouristarModels.Contracts;
using TouristarModels.Models;
using TouristarModels.Repositories;

namespace TouristarConsumer.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private readonly DatabaseContext _databaseContext;

    private IGptRepository? _gptRepository;
    private ITripRepository? _tripRepository;
    private ITicketRepository? _ticketRepository;
    private ILocationRepository? _locationRepository;
    private IFlightOperatorRepository? _flightOperatorRepository;
    private IFlightStatusRepository? _flightStatusRepository;
    private IUserRepository? _userRepository;
    private IMessagingRepository? _messagingRepository;
    private RadarRepository? _radarRepository;

    private IOptionsMonitor<OpenAiConfig> _openAiOptions;
    private IOptionsMonitor<RadarConfig> _radarOptions;
    private ILogger<IMessagingRepository> _messagingLogger;

    public RepositoryManager(
        DatabaseContext databaseContext,
        IOptionsMonitor<OpenAiConfig> openAiOptions,
        IOptionsMonitor<RadarConfig> radarOptions,
        ILogger<IMessagingRepository> messagingLogger
    )
    {
        _databaseContext = databaseContext;
        _openAiOptions = openAiOptions;
        _messagingLogger = messagingLogger;
        _radarOptions = radarOptions;
    }

    public IGptRepository Gpt
    {
        get
        {
            _gptRepository ??= new GptRepository(_openAiOptions);
            return _gptRepository;
        }
    }

    public ITripRepository Trip
    {
        get
        {
            _tripRepository ??= new TripRepository(_databaseContext);
            return _tripRepository;
        }
    }

    public ITicketRepository Ticket
    {
        get
        {
            _ticketRepository ??= new TicketRepository(_databaseContext);
            return _ticketRepository;
        }
    }

    public ILocationRepository Location
    {
        get
        {
            _locationRepository ??= new LocationRepository(_databaseContext);
            return _locationRepository;
        }
    }

    public IFlightOperatorRepository FlightOperator
    {
        get
        {
            _flightOperatorRepository ??= new FlightOperatorRepository(_databaseContext);
            return _flightOperatorRepository;
        }
    }

    public IFlightStatusRepository FlightStatus
    {
        get
        {
            _flightStatusRepository ??= new FlightStatusRepository(_databaseContext);
            return _flightStatusRepository;
        }
    }

    public IUserRepository User
    {
        get
        {
            _userRepository ??= new UserRepository(_databaseContext);
            return _userRepository;
        }
    }
    public IMessagingRepository Messaging
    {
        get
        {
            var googleCredential = GoogleCredential.FromFile(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json")
            );
            _messagingRepository ??= new MessagingRepository(_messagingLogger, googleCredential);
            return _messagingRepository;
        }
    }
    public IRadarRepository Radar
    {
        get
        {
            var googleCredential = GoogleCredential.FromFile(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json")
            );
            _radarRepository ??= new RadarRepository(_radarOptions.CurrentValue);
            return _radarRepository;
        }
    }

    async Task IRepositoryManager.Save()
    {
        await Task.Run(() => _databaseContext.SaveChangesAsync());
    }
}
