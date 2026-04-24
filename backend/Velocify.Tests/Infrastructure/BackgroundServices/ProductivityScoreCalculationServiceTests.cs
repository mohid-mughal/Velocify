using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;
using Velocify.Infrastructure.Services.BackgroundServices;
using Xunit;

namespace Velocify.Tests.Infrastructure.BackgroundServices;

public class ProductivityScoreCalculationServiceTests
{
    private readonly Mock<ILogger<ProductivityScoreCalculationService>> _loggerMock;
    private readonly IServiceProvider _serviceProvider;
    private readonly VelocifyDbContext _context;

    public ProductivityScoreCalculationServiceTests()
    {
        _loggerMock = new Mock<ILogger<ProductivityScoreCalculationService>>();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<VelocifyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VelocifyDbContext(options);

        // Setup service provider
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task StartAsync_ShouldStartService_WithoutErrors()
    {
        // Arrange
        var service = new ProductivityScoreCalculationService(_serviceProvider, _loggerMock.Object);

        // Act
        var act = async () => await service.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_ShouldStopService_WithoutErrors()
    {
        // Arrange
        var service = new ProductivityScoreCalculationService(_serviceProvider, _loggerMock.Object);
        await service.StartAsync(CancellationToken.None);

        // Act
        var act = async () => await service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Dispose_ShouldDisposeService_WithoutErrors()
    {
        // Arrange
        var service = new ProductivityScoreCalculationService(_serviceProvider, _loggerMock.Object);

        // Act
        var act = () => service.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Service_ShouldBeRegistered_AsHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var options = new DbContextOptionsBuilder<VelocifyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        services.AddScoped(_ => new VelocifyDbContext(options));
        
        services.AddHostedService<ProductivityScoreCalculationService>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

        // Assert
        hostedServices.Should().NotBeEmpty();
        hostedServices.Should().ContainSingle(s => s is ProductivityScoreCalculationService);
    }
}
