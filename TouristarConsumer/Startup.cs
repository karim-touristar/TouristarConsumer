using Microsoft.EntityFrameworkCore;
using Npgsql;
using TouristarConsumer.Contracts;
using TouristarConsumer.Models;
using TouristarConsumer.Repositories;
using TouristarConsumer.Services;
using TouristarModels.Enums;
using TouristarModels.Models;

namespace TouristarConsumer;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        SetupConfiguration(services, configuration);
        AddDatabaseContext(services, configuration);
        AddServices(services);
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IEmailProcessingService, EmailProcessingService>();
        services.AddScoped<IFlightOperatorService, FlightOperatorService>();
        services.AddScoped<IFlightStatusService, FlightStatusService>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        services.AddHostedService<EmailConsumerService>();
        services.AddHostedService<FlightStatusConsumerService>();
    }

    private static void AddDatabaseContext(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionConfig = configuration
            .GetSection("ConnectionStrings")
            .Get<ConnectionConfig>();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionConfig?.DbConnection);
        dataSourceBuilder.MapEnum<ActivityType>();
        dataSourceBuilder.MapEnum<TicketLeg>();
        dataSourceBuilder.MapEnum<TripDocumentType>();
        var dataSource = dataSourceBuilder.Build();
        services.AddDbContextPool<DatabaseContext>(options =>
        {
            options.UseNpgsql(dataSource);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
    }

    private static void SetupConfiguration(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<RabbitConfig>(configuration.GetSection("RabbitMq"));
        services.Configure<OpenAiConfig>(configuration.GetSection("OpenAi"));
        services.Configure<LogoFetchingConfig>(configuration.GetSection("LogoFetching"));
        services.Configure<CiriumConfig>(configuration.GetSection("Cirium"));
        services.Configure<RadarConfig>(configuration.GetSection("Radar"));
    }
}
