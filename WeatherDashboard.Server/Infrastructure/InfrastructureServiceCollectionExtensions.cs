using Microsoft.Extensions.Caching.Distributed;
using WeatherDashboard.Server.Domain.Interfaces;
using WeatherDashboard.Server.Infrastructure.OpenWeatherMap;
using WeatherDashboard.Server.Infrastructure.Redis;

namespace WeatherDashboard.Server.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
	public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
	{
		builder.AddRedisDistributedCache("redis");

		IServiceCollection services = builder.Services;

		services.AddHttpClient<OpenWeatherMapProvider>(client =>
		{
			client.BaseAddress = new Uri("https://api.openweathermap.org/");
			client.Timeout = TimeSpan.FromSeconds(10);
		});

		services.AddScoped<IWeatherProvider>(sp =>
			new CachedWeatherProvider(
				sp.GetRequiredService<OpenWeatherMapProvider>(),
				sp.GetRequiredService<IDistributedCache>(),
				sp.GetRequiredService<ILogger<CachedWeatherProvider>>()));

		services.AddSingleton<ILocationRepository, RedisLocationRepository>();

		return builder;
	}
}
