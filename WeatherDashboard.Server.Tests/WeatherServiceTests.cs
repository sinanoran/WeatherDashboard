using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherDashboard.Server.Application.Contracts;
using WeatherDashboard.Server.Application.Services;
using WeatherDashboard.Server.Domain.Interfaces;
using WeatherDashboard.Server.Domain.Models;
using Xunit;

namespace WeatherDashboard.Server.Tests;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherProvider> _provider = new Mock<IWeatherProvider>();
    private readonly WeatherService _sut;

    public WeatherServiceTests()
    {
        _sut = new WeatherService(_provider.Object, NullLogger<WeatherService>.Instance);
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsNull_WhenProviderReturnsNull()
    {
        _provider
            .Setup(provider => provider.GetWeatherAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherInfo?)null);

        WeatherResponse? result = await _sut.GetWeatherAsync("Unknown");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsMappedResponse_WhenProviderReturnsData()
    {
        WeatherInfo info = new WeatherInfo("Berlin", 18.3, 55, 4.2, "overcast clouds", "04d");
        _provider
            .Setup(provider => provider.GetWeatherAsync("Berlin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(info);

        WeatherResponse? result = await _sut.GetWeatherAsync("Berlin");

        Assert.NotNull(result);
        Assert.Equal("Berlin", result.City);
        Assert.Equal(18.3, result.TemperatureC);
        Assert.Equal(55, result.Humidity);
        Assert.Equal(4.2, result.WindSpeedMps);
        Assert.Equal("overcast clouds", result.Description);
        Assert.Equal("https://openweathermap.org/img/wn/04d@2x.png", result.Icon);
    }

    [Fact]
    public async Task GetWeatherAsync_PassesCancellationToken()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        _provider
            .Setup(provider => provider.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherInfo?)null);

        await _sut.GetWeatherAsync("London", cts.Token);

        _provider.Verify(provider => provider.GetWeatherAsync("London", cts.Token), Times.Once);
    }
}
