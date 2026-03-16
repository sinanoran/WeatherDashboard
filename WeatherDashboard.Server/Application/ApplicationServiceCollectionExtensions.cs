using WeatherDashboard.Server.Application.Interfaces;
using WeatherDashboard.Server.Application.Services;

namespace WeatherDashboard.Server.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddSingleton<ILocationService, LocationService>();

        return services;
    }
}
