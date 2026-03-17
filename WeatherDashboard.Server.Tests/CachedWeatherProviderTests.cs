using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using System.Text.Json;
using WeatherDashboard.Server.Domain.Interfaces;
using WeatherDashboard.Server.Domain.Models;
using WeatherDashboard.Server.Infrastructure.Redis;
using Xunit;

namespace WeatherDashboard.Server.Tests;

public class CachedWeatherProviderTests
{
    private readonly Mock<IWeatherProvider> _inner = new Mock<IWeatherProvider>();
    private readonly Mock<IDistributedCache> _cache = new Mock<IDistributedCache>();
    private readonly Mock<IOptions<WeatherCacheOptions>> _options = new Mock<IOptions<WeatherCacheOptions>>();
    private readonly CachedWeatherProvider _sut;

    public CachedWeatherProviderTests()
    {
        _sut = new CachedWeatherProvider(_inner.Object, _cache.Object, _options.Object, NullLogger<CachedWeatherProvider>.Instance);
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsCachedData_WhenCacheHit()
    {
        WeatherInfo expected = new WeatherInfo("London", 15, 60, 3.5, "clear sky", "01d");
        string json = JsonSerializer.Serialize(expected);
        _cache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));

        WeatherInfo? result = await _sut.GetWeatherAsync("London");

        Assert.NotNull(result);
        Assert.Equal("London", result.City);
        Assert.Equal(15, result.TemperatureCelsius);
        _inner.Verify(provider => provider.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetWeatherAsync_CallsInner_WhenCacheMiss()
    {
        _cache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        WeatherInfo expected = new WeatherInfo("Paris", 20, 55, 4.0, "cloudy", "03d");
        _inner
            .Setup(provider => provider.GetWeatherAsync("Paris", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        WeatherInfo? result = await _sut.GetWeatherAsync("Paris");

        Assert.NotNull(result);
        Assert.Equal("Paris", result.City);
        _inner.Verify(provider => provider.GetWeatherAsync("Paris", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWeatherAsync_StoresResultInCache_WhenCacheMiss()
    {
        _cache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        WeatherInfo info = new WeatherInfo("Berlin", 10, 70, 2.0, "rain", "09d");
        _inner
            .Setup(provider => provider.GetWeatherAsync("Berlin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(info);

        await _sut.GetWeatherAsync("Berlin");

        _cache.Verify(
            cache => cache.SetAsync(
                "weather:berlin",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWeatherAsync_DoesNotCache_WhenInnerReturnsNull()
    {
        _cache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        _inner
            .Setup(provider => provider.GetWeatherAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherInfo?)null);

        WeatherInfo? result = await _sut.GetWeatherAsync("Unknown");

        Assert.Null(result);
        _cache.Verify(
            cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWeatherAsync_NormalizesCacheKey_ToLowerTrimmed()
    {
        _cache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        _inner
            .Setup(provider => provider.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherInfo?)null);

        await _sut.GetWeatherAsync("  LONDON  ");

        _cache.Verify(cache => cache.GetAsync("weather:london", It.IsAny<CancellationToken>()), Times.Once);
    }
}
