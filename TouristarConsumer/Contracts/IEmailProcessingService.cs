using TouristarModels.Models;

namespace TouristarConsumer.Contracts;

public interface IEmailProcessingService
{
    public Task ProcessEmail(EmailProcessingMessageDto message);
}