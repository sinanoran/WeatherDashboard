using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using WeatherDashboard.Server.Domain.Interfaces;
using WeatherDashboard.Server.Domain.Models;

namespace WeatherDashboard.Server.Infrastructure.Redis;

/// <summary>
/// Decorator that adds distributed caching in front of any <see cref="IWeatherProvider"/>.
/// Falls back transparently to the inner provider when the cache is unavailable.
/// </summary>
public sealed class CachedWeatherProvider(
    IWeatherProvider inner,
    IDistributedCache cache,
    IOptions<WeatherCacheOptions> options,
    ILogger<CachedWeatherProvider> logger) : IWeatherProvider
{
    public async Task<WeatherInfo?> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"weather:{city.ToLowerInvariant().Trim()}";

        try
        {
            string? cached = await cache.GetStringAsync(cacheKey, cancellationToken);
            if (cached is not null)
            {
                logger.LogDebug("Cache hit for city {City}", city);
                return JsonSerializer.Deserialize<WeatherInfo>(cached);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache read failed for {City}. Falling through to provider.", city);
        }

        logger.LogDebug("Cache miss for city {City}", city);

        WeatherInfo? result = await inner.GetWeatherAsync(city, cancellationToken);

        if (result is not null)
        {
            DistributedCacheEntryOptions cacheEntryOptions = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.ExpirationMinutes)
            };

            try
            {
                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheEntryOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cache write failed for {City}.", city);
            }
        }

        return result;
    }
}

