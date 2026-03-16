using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using WeatherDashboard.Server.Domain.Interfaces;
using WeatherDashboard.Server.Infrastructure.OpenWeatherMap;
using WeatherDashboard.Server.Infrastructure.Redis;

namespace WeatherDashboard.Server.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
	public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
	{
		if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("redis")))
		{
			builder.AddRedisDistributedCache("redis");
		}
		else
		{
			builder.Services.AddDistributedMemoryCache();
		}

		builder.Services.Configure<WeatherCacheOptions>(
			builder.Configuration.GetSection(WeatherCacheOptions.SectionPath));

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
				sp.GetRequiredService<IOptions<WeatherCacheOptions>>(),
				sp.GetRequiredService<ILogger<CachedWeatherProvider>>()));

		services.AddSingleton<ILocationRepository, RedisLocationRepository>();

		return builder;
	}
}

