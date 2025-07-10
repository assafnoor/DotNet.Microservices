using CommandsService.Data;
using Microsoft.EntityFrameworkCore;
using CommandsService.AsyncDataServices;
using CommandsService.Data;
using CommandsService.EventProcessing;
using CommandsService.SyncDataServices.Grpc;
namespace CommandsService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMen"));
        services.AddScoped<ICommandRepo, CommandRepo>();
        services.AddControllers();

        services.AddHostedService<MessageBusSubscriber>();

        services.AddSingleton<IEventProcessor, EventProcessor>(); 
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddScoped<IPlatformDataClient, PlatformDataClient>();
        services.AddControllers();

        services.AddOpenApi();
        return services;
    }
}