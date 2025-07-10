using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        if (env.IsProduction())
        {
            Console.WriteLine("In Production mode, using SQL Server database");
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("PlatformsConn")));
        }
        else
        {
            Console.WriteLine("In Development mode, using InMemory database");
            services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("InMem"));
        }
    
        services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
        services.AddSingleton<IMessageBusClient, MessageBusClient>();
        services.AddGrpc();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}