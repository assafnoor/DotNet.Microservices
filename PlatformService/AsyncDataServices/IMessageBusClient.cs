using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void PublishNewPlatform(PlatformPublishedDto platformPublishedDto);

        Task PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto);
    }
}