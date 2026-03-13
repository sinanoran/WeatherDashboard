using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherDashboard.Server.Application.Services;
using WeatherDashboard.Server.Domain.Interfaces;
using Xunit;

namespace WeatherDashboard.Server.Tests;

public class LocationServiceTests
{
    private readonly Mock<ILocationRepository> _repository = new Mock<ILocationRepository>();
    private readonly LocationService _sut;

    public LocationServiceTests()
    {
        _sut = new LocationService(_repository.Object, NullLogger<LocationService>.Instance);
    }

    [Fact]
    public async Task GetDefaultLocationAsync_ReturnsFallback_WhenRepositoryReturnsNull()
    {
        _repository
            .Setup(repository => repository.GetDefaultCityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        string result = await _sut.GetDefaultLocationAsync();

        Assert.Equal("London", result);
    }

    [Fact]
    public async Task GetDefaultLocationAsync_ReturnsStoredValue()
    {
        _repository
            .Setup(repository => repository.GetDefaultCityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Paris");

        string result = await _sut.GetDefaultLocationAsync();

        Assert.Equal("Paris", result);
    }

    [Fact]
    public async Task GetDefaultLocationAsync_ReturnsFallback_WhenRepositoryReturnsEmptyString()
    {
        _repository
            .Setup(repository => repository.GetDefaultCityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        string result = await _sut.GetDefaultLocationAsync();

        Assert.Equal("London", result);
    }

    [Fact]
    public async Task GetDefaultLocationAsync_ReturnsFallback_WhenRepositoryReturnsWhitespace()
    {
        _repository
            .Setup(repository => repository.GetDefaultCityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("   ");

        string result = await _sut.GetDefaultLocationAsync();

        Assert.Equal("London", result);
    }

    [Fact]
    public async Task SetDefaultLocationAsync_DelegatesToRepository()
    {
        await _sut.SetDefaultLocationAsync("Berlin");

        _repository.Verify(repository => repository.SetDefaultCityAsync("Berlin", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetDefaultLocationAsync_TrimsCityBeforeSaving()
    {
        await _sut.SetDefaultLocationAsync("  Tokyo  ");

        _repository.Verify(repository => repository.SetDefaultCityAsync("Tokyo", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetDefaultLocationAsync_PassesCancellationToken()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();

        await _sut.SetDefaultLocationAsync("Rome", cts.Token);

        _repository.Verify(repository => repository.SetDefaultCityAsync("Rome", cts.Token), Times.Once);
    }
}
