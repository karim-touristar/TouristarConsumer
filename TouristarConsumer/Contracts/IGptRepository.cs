using TouristarModels.Models;

namespace TouristarConsumer.Contracts;

public interface IGptRepository
{
    public Task<List<GptTicketData?>?> GetTicketDataFromText(string emailContent, Location destination);
}