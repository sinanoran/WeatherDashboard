using Microsoft.Extensions.Caching.Distributed;
using WeatherDashboard.Server.Domain.Interfaces;

namespace WeatherDashboard.Server.Infrastructure.Redis;

public sealed class RedisLocationRepository(
    IDistributedCache cache,
    ILogger<RedisLocationRepository> logger) : ILocationRepository
{
    private const string CacheKey = "default-location";

    public async Task<string?> GetDefaultCityAsync(CancellationToken cancellationToken = default)
    {
        return await cache.GetStringAsync(CacheKey, cancellationToken);
    }

    public async Task SetDefaultCityAsync(string city, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Persisting default location: {City}", city);

        await cache.SetStringAsync(CacheKey, city, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
        }, cancellationToken);
    }
}
