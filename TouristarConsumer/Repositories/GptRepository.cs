using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TouristarConsumer.Contracts;
using TouristarConsumer.Models;
using TouristarModels.Models;

namespace TouristarConsumer.Repositories;

public class GptRepository : IGptRepository
{
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    public GptRepository(IOptionsMonitor<OpenAiConfig> optionsMonitor)
    {
        _baseUrl = optionsMonitor.CurrentValue.BaseUrl;
        _httpClient = new HttpClient();

        var authorisationKey = optionsMonitor.CurrentValue.ApiKey;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorisationKey}");
    }

    public async Task<List<GptTicketData?>?> GetTicketDataFromText(string emailContent, Location destination)
    {
        GptInputMessage firstMessage = new()
        {
            Role = "system",
            Content =
                "You are a helpful assistant that is able to intelligently convert uncategorized text from travel reservation emails to the following JSON structure: [{ DepartureCity: string, DepartureCountry: string, ArrivalCity: string, ArrivalCountry: string, DepartAt: datetime, ArriveAt: datetime, FlightNumber: string?, ReservationNumber: string?, FlightOperator: string, AirlineCarrierCode: string, DepartureAirportCode: string, ArrivalAirportCode: string, TripLeg: 'outbound' | 'inbound' }]. The result should be an array of json objects, with the outbound flight always at position 0, and the inbound flight (where applicable) at position 1. If no inbound flight is provided in the input email, simply return null as the element at position 1 in the array. You should only respond with JSON and nothing else; for instance, you should not add any plain English text before or after your JSON response. If the source text does not contain enough detail for you to parse all the information into a JSON, just answer with the following text: not-applicable. When specifying the flight operator, give the name of the overarching company. For example, don't say BA CityFlyer, but British Airways etc."
        };
        GptInputMessage secondMessage = new()
        {
            Role = "user",
            Content =
                $"Please decode the following travel booking text into a structured JSON object: {emailContent}. Please bear in mind that the user's holiday destination is {destination.City}, {destination.Country}, which will inform your choice of the TripLeg parameter."
        };
        GptChatCompletionInputDto request = new()
        {
            Model = "gpt-3.5-turbo-16k",
            Messages = new List<GptInputMessage>() { firstMessage, secondMessage },
            Temperature = 0
        };

        var response = await _httpClient
            .PostAsJsonAsync($"{_baseUrl}/v1/chat/completions", request);
        var content = await response.Content.ReadAsStringAsync();
        var completion = JsonConvert.DeserializeObject<GptChatCompletionDto>(content);

        var message = completion?.Choices.First().Message.Content;
        if (message == null || message == "not-applicable")
        {
            throw new InvalidOperationException("Gpt could not decode reservation email text.");
        }

        return JsonConvert.DeserializeObject<List<GptTicketData?>>(message);
    }
}