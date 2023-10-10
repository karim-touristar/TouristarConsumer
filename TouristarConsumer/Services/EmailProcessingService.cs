using System.Text;
using Newtonsoft.Json;
using TouristarConsumer.Contracts;
using TouristarModels.Enums;
using TouristarModels.Models;
using TouristarModels.Models.Push;

namespace TouristarConsumer.Services;

public class EmailProcessingService : IEmailProcessingService
{
    private readonly ILogger<EmailProcessingService> _logger;
    private readonly IRepositoryManager _repository;
    private readonly IFlightOperatorService _flightOperatorService;

    public EmailProcessingService(
        ILogger<EmailProcessingService> logger,
        IRepositoryManager repository,
        IFlightOperatorService flightOperatorService
    )
    {
        _logger = logger;
        _repository = repository;
        _flightOperatorService = flightOperatorService;
    }

    public async Task ProcessEmail(EmailProcessingMessageDto message)
    {
        var messageText = Encoding.UTF8.GetString(Convert.FromBase64String(message.Base64Text));
        var trip = _repository.Trip.FindTrip(message.TripId);
        if (trip.ArrivalLocation == null)
        {
            _logger.LogError("Could not find arrival destination for trip.");
            return;
        }

        var data = await _repository.Gpt.GetTicketDataFromText(messageText, trip.ArrivalLocation);
        if (data == null)
        {
            _logger.LogError("GPT data was null.");
            return;
        }

        _logger.LogInformation($"Retrieved GPT message: {JsonConvert.SerializeObject(data)}");

        var destinationCity = "";
        foreach (var legData in data)
        {
            try
            {
                if (legData == null)
                {
                    _logger.LogInformation("Skipping leg as it is null.");
                    continue;
                }

                if (LegFromString(legData.TripLeg) == TicketLeg.Outbound)
                {
                    destinationCity = legData.ArrivalCity;
                }

                if (!CanSaveTicket(LegFromString(legData.TripLeg), trip.Id))
                {
                    _logger.LogWarning(
                        $"Could not save ticket as one for this trip and leg already exists. Trip id: {trip.Id}, leg: {legData.TripLeg}."
                    );
                    return;
                }
                await CreateTicketFromGptData(legData, trip);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"There was an issue processing email, user id: {trip.UserId}, exception: {e}."
                );
            }
        }

        var user = _repository.User.FindById(trip.UserId);
        await SendTicketProcessingCompleteNotification(trip, destinationCity, user);

        user.IsSyncingTickets = false;
        _repository.User.UpdateUser(user);
        await _repository.Save();
    }

    private async Task CreateTicketFromGptData(GptTicketData data, Trip trip)
    {
        _logger.LogInformation(
            $"Creating ticket for GPT data. Trip id: {trip.Id}, user id: {trip.UserId}"
        );
        var flightOperator = await _flightOperatorService.FindOrCreateOperator(
            data.FlightOperator,
            data.AirlineCarrierCode
        );
        if (flightOperator == null)
        {
            _logger.LogError(
                $"Could not find or create flight operator. Trip id: {trip.Id}, user id: {trip.UserId}, proposed operator name: {data.FlightOperator}, proposed operator code: {data.AirlineCarrierCode}."
            );
            return;
        }

        var departureLocation =
            await FindOrCreateLocation(data.DepartureCity, data.DepartureCountry)
            ?? trip.DepartureLocation;
        var arrivalLocation =
            await FindOrCreateLocation(data.ArrivalCity, data.ArrivalCountry)
            ?? trip.ArrivalLocation;

        Ticket ticket =
            new()
            {
                Leg = LegFromString(data.TripLeg),
                DepartAt = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(data.DepartAt)),
                ArriveAt =
                    data.ArriveAt == null
                        ? null
                        : TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(data.ArriveAt)),
                FlightNumber = FormatFlightNumber(data.FlightNumber),
                ReservationNumber = data.ReservationNumber,
                DepartureAirportCode = data.DepartureAirportCode,
                ArrivalAirportCode = data.ArrivalAirportCode,
                FlightOperatorId = flightOperator.Id,
                DepartureLocationId = departureLocation.Id,
                ArrivalLocationId = arrivalLocation.Id,
                TripId = trip.Id
            };
        _repository.Ticket.CreateTicket(ticket);
        await _repository.Save();
    }

    private async Task SendTicketProcessingCompleteNotification(Trip trip, string destinationCity, User user)
    {
        if (user.DeviceToken == null)
        {
            _logger.LogError(
                $"Could not send successful ticket processing notification as device token was not found. User id: {trip.UserId}."
            );
            return;
        }

        Dictionary<string, string> notificationData =
            new()
            {
                {
                    "type",
                    Enum.GetName(
                        typeof(PushNotifications),
                        PushNotifications.TicketProcessingComplete
                    )!
                },
                { "tripId", trip.Id.ToString() }
            };
        var notification = new TicketProcessingCompletePush(destinationCity);
        await _repository.Messaging.SendPushNotification(
            user.DeviceToken,
            notification.Title,
            notification.Body,
            notificationData
        );
    }

    private bool CanSaveTicket(TicketLeg leg, long tripId) =>
        _repository.Ticket.FindTicketsForTrip(tripId).All(t => t.Leg != leg);

    private static TicketLeg LegFromString(string leg) =>
        leg == "outbound" ? TicketLeg.Outbound : TicketLeg.Inbound;

    private async Task<Location> FindOrCreateLocation(string city, string country)
    {
        var location = _repository.Location.FindByCountryAndCity(city, country);
        if (location != null)
        {
            return location;
        }
        var result = await _repository.Radar.SearchLocations($"{city} {country}");
        if (!result.Any() || result == null || result.First() == null)
        {
            throw new InvalidOperationException(
                $"Could not retrieve Radar location for trip, {city}, {country}."
            );
        }
        var locationToCreate = new List<Location>() { result.First() };
        _repository.Location.CreateLocations(locationToCreate);
        await _repository.Save();
        return locationToCreate.First();
    }

    private string FormatFlightNumber(string? flightNumber)
    {
        if (flightNumber == null)
            return "";
        var numberString = new string(flightNumber.Where(char.IsDigit).ToArray());
        if (numberString.StartsWith("0"))
        {
            return numberString[1..];
        }

        return numberString;
    }
}
