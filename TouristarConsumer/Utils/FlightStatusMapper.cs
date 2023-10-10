using Org.OpenAPITools.Model;
using DbModels = TouristarModels.Models;

namespace TouristarConsumer.Utils;

public class FlightStatusMapper
{
    public static DbModels.FlightStatus MapToDbFlightStatus(FlightStatus status, long ticketId)
    {
        DbModels.Airline? airline = status.Carrier == null
            ? null
            : new()
            {
                Fs = status.Carrier.Fs,
                Iata = status.Carrier.Iata,
                Icao = status.Carrier.Icao,
                Name = status.Carrier.Name,
                PhoneNumber = status.Carrier.PhoneNumber,
                Active = status.Carrier.Active,
                Category = status.Carrier.Category
            };

        var departureAirport = status.DepartureAirport == null ? null : MapToAirport(status.DepartureAirport);

        var arrivalAirport = status.ArrivalAirport == null ? null : MapToAirport(status.ArrivalAirport);

        var divertedAirport = status.DivertedAirport == null ? null : MapToAirport(status.DivertedAirport);

        var departureDate = status.DepartureDate == null ? null : MapToLocalisedDate(status.DepartureDate);

        var arrivalDate = status.ArrivalDate == null ? null : MapToLocalisedDate(status.ArrivalDate);

        DbModels.FlightSchedule? schedule = status.Schedule == null
            ? null
            : new DbModels.FlightSchedule
            {
                FlightType = status.Schedule.FlightType,
                ServiceClasses = status.Schedule.ServiceClasses,
                Restrictions = status.Schedule.Restrictions
            };

        DbModels.FlightOperationalTimes operationalTimes = new()
        {
            PublishedDeparture = MapToLocalisedDate(status.OperationalTimes.PublishedDeparture),
            PublishedArrival = MapToLocalisedDate(status.OperationalTimes.PublishedArrival),
            ScheduledGateDeparture = MapToLocalisedDate(status.OperationalTimes.ScheduledGateDeparture),
            ScheduledRunwayDeparture = MapToLocalisedDate(status.OperationalTimes.ScheduledRunwayDeparture),
            EstimatedGateDeparture = MapToLocalisedDate(status.OperationalTimes.EstimatedGateDeparture),
            ActualGateDeparture = MapToLocalisedDate(status.OperationalTimes.ActualGateDeparture),
            FlightPlanPlannedDeparture = MapToLocalisedDate(status.OperationalTimes.FlightPlanPlannedDeparture),
            EstimatedRunwayDeparture = MapToLocalisedDate(status.OperationalTimes.EstimatedRunwayDeparture),
            ActualRunwayDeparture = MapToLocalisedDate(status.OperationalTimes.ActualRunwayDeparture),
            ScheduledRunwayArrival = MapToLocalisedDate(status.OperationalTimes.ScheduledRunwayArrival),
            ScheduledGateArrival = MapToLocalisedDate(status.OperationalTimes.ScheduledGateArrival),
            EstimatedGateArrival = MapToLocalisedDate(status.OperationalTimes.EstimatedGateArrival),
            ActualGateArrival = MapToLocalisedDate(status.OperationalTimes.ActualGateArrival),
            FlightPlanPlannedArrival = MapToLocalisedDate(status.OperationalTimes.FlightPlanPlannedArrival),
            EstimatedRunwayArrival = MapToLocalisedDate(status.OperationalTimes.EstimatedRunwayArrival),
            ActualRunwayArrival = MapToLocalisedDate(status.OperationalTimes.ActualRunwayArrival)
        };

        DbModels.FlightDelays delays = status.Delays == null
            ? null
            : new()
            {
                DepartureGateDelayMinutes = status.Delays.DepartureGateDelayMinutes,
                DepartureRunwayDelayMinutes = status.Delays.DepartureRunwayDelayMinutes,
                ArrivalGateDelayMinutes = status.Delays.ArrivalGateDelayMinutes,
                ArrivalRunwayDelayMinutes = status.Delays.ArrivalRunwayDelayMinutes
            };

        DbModels.FlightDurations durations = status.FlightDurations == null
            ? null
            : new()
            {
                ScheduledBlockMinutes = status.FlightDurations.ScheduledBlockMinutes,
                BlockMinutes = status.FlightDurations.BlockMinutes,
                ScheduledAirMinutes = status.FlightDurations.ScheduledAirMinutes,
                AirMinutes = status.FlightDurations.AirMinutes,
                ScheduledTaxiOutMinutes = status.FlightDurations.ScheduledTaxiOutMinutes,
                TaxiOutMinutes = status.FlightDurations.TaxiOutMinutes,
                ScheduledTaxiInMinutes = status.FlightDurations.ScheduledTaxiInMinutes,
                TaxiInMinutes = status.FlightDurations.TaxiInMinutes
            };

        DbModels.AirportResources airportResources = status.AirportResources == null
            ? null
            : new()
            {
                DepartureTerminal = status.AirportResources.DepartureTerminal,
                DepartureGate = status.AirportResources.DepartureGate,
                ArrivalTerminal = status.AirportResources.ArrivalTerminal,
                ArrivalGate = status.AirportResources.ArrivalGate,
                Baggage = status.AirportResources.Baggage
            };

        return new DbModels.FlightStatus
        {
            FlightId = status.FlightId,
            Airline = airline,
            CarrierFsCode = status.CarrierFsCode,
            FlightNumber = status.FlightNumber,
            DepartureAirport = departureAirport,
            DepartureAirportFsCode = status.DepartureAirportFsCode,
            ArrivalAirport = arrivalAirport,
            ArrivalAirportFsCode = status.ArrivalAirportFsCode,
            DivertedAirport = divertedAirport,
            DivertedAirportFsCode = status.DivertedAirportFsCode,
            DepartureDate = departureDate,
            ArrivalDate = arrivalDate,
            Status = status.Status,
            Schedule = schedule,
            OperationalTimes = operationalTimes,
            Delays = delays,
            Durations = durations,
            AirportResources = airportResources,
            LastDataAcquiredDate = status.LastDataAcquiredDate == null
                ? null
                : TimeZoneInfo.ConvertTimeToUtc(status.LastDataAcquiredDate),
            TicketId = ticketId,
        };
    }

    static DbModels.Airport MapToAirport(Airport airport)
        => new()
        {
            Iata = airport?.Iata,
            Icao = airport?.Icao,
            Faa = airport?.Faa,
            Name = airport?.Name,
            Street1 = airport?.Street1,
            Street2 = airport?.Street2,
            City = airport?.City,
            District = airport?.District,
            StateCode = airport?.StateCode,
            PostalCode = airport?.PostalCode,
            CountryCode = airport?.CountryCode,
            CountryName = airport?.CountryName,
            RegionName = airport?.RegionName,
            TimeZoneRegionName = airport?.TimeZoneRegionName,
            WeatherZone = airport?.WeatherZone,
            LocalTime = airport?.LocalTime,
            UtcOffsetHours = airport.UtcOffsetHours,
            Latitude = airport.Latitude,
            Longitude = airport.Longitude,
            ElevationFeet = airport.ElevationFeet,
            Classification = airport.Classification,
            Active = airport.Active,
            DelayIndexUrl = airport?.DelayIndexUrl,
            WeatherUrl = airport?.WeatherUrl
        };

    static DbModels.LocalisedDate MapToLocalisedDate(DateUtcAndLocal date)
        => new()
        {
            Local = date?.DateLocal == null
                ? null
                : TimeZoneInfo.ConvertTimeToUtc(date.DateLocal),
            Utc = date?.DateUtc == null
                ? null
                : TimeZoneInfo.ConvertTimeToUtc(date.DateUtc)
        };
}