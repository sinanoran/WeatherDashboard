using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using WeatherDashboard.Server.Domain.Interfaces;
using WeatherDashboard.Server.Domain.Models;

namespace WeatherDashboard.Server.Infrastructure.Redis;

/// <summary>
/// Decorator that adds Redis caching in front of any <see cref="IWeatherProvider"/>.
/// </summary>
public sealed class CachedWeatherProvider(
    IWeatherProvider inner,
    IDistributedCache cache,
    ILogger<CachedWeatherProvider> logger) : IWeatherProvider
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public async Task<WeatherInfo?> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"weather:{city.ToLowerInvariant().Trim()}";

        string? cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for city {City}", city);
            return JsonSerializer.Deserialize<WeatherInfo>(cached);
        }

        logger.LogDebug("Cache miss for city {City}", city);

        WeatherInfo? result = await inner.GetWeatherAsync(city, cancellationToken);

        if (result is not null)
        {
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), CacheOptions, cancellationToken);
        }

        return result;
    }
}
