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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetDefaultLocationAsync_ReturnsFallback_WhenRepositoryReturnsMissingValue(string? storedCity)
    {
        _repository
            .Setup(repository => repository.GetDefaultCityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedCity);

        string result = await _sut.GetDefaultLocationAsync();

        Assert.Equal("London", result);
    }

    [Fact]
    public async Task GetDefaultLocationAsync_ReturnsStoredValue_WhenRepositoryHasCity()
    {
        _repository
            .Setup(repository => repository.GetDefaultCityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Paris");

        string result = await _sut.GetDefaultLocationAsync();

        Assert.Equal("Paris", result);
    }

    [Fact]
    public async Task SetDefaultLocationAsync_SavesTrimmedCity_WithProvidedCancellationToken()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();

        await _sut.SetDefaultLocationAsync("  Tokyo  ", cts.Token);

        _repository.Verify(repository => repository.SetDefaultCityAsync("Tokyo", cts.Token), Times.Once);
    }
}
