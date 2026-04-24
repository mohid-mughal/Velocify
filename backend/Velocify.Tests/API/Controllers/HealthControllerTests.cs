using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Velocify.API.Controllers;
using Velocify.Infrastructure.Data;
using Xunit;

namespace Velocify.Tests.API.Controllers;

public class HealthControllerTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<HealthController>> _loggerMock;

    public HealthControllerTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<HealthController>>();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenAllChecksPass()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VelocifyDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthCheckTestDb_AllPass")
            .Options;

        using var context = new VelocifyDbContext(options);

        // Configure OpenAI API key
        _configurationMock.Setup(c => c["OpenAI:ApiKey"]).Returns("test-api-key");

        var controller = new HealthController(context, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await controller.GetHealth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnServiceUnavailable_WhenDatabaseCheckFails()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VelocifyDbContext>()
            .UseSqlServer("Server=invalid;Database=invalid;") // Invalid connection string
            .Options;

        using var context = new VelocifyDbContext(options);

        // Configure OpenAI API key
        _configurationMock.Setup(c => c["OpenAI:ApiKey"]).Returns("test-api-key");

        var controller = new HealthController(context, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await controller.GetHealth();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task GetHealth_ShouldReturnServiceUnavailable_WhenLangChainCheckFails()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VelocifyDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthCheckTestDb_LangChainFail")
            .Options;

        using var context = new VelocifyDbContext(options);

        // Configure missing OpenAI API key
        _configurationMock.Setup(c => c["OpenAI:ApiKey"]).Returns((string?)null);

        var controller = new HealthController(context, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await controller.GetHealth();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task GetHealth_ShouldIncludeAllCheckResults()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VelocifyDbContext>()
            .UseInMemoryDatabase(databaseName: "HealthCheckTestDb_AllResults")
            .Options;

        using var context = new VelocifyDbContext(options);

        // Configure OpenAI API key
        _configurationMock.Setup(c => c["OpenAI:ApiKey"]).Returns("test-api-key");

        var controller = new HealthController(context, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await controller.GetHealth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        
        var response = okResult!.Value;
        response.Should().NotBeNull();

        // Verify response has the expected structure
        var responseType = response!.GetType();
        responseType.GetProperty("status").Should().NotBeNull();
        responseType.GetProperty("checks").Should().NotBeNull();
        responseType.GetProperty("timestamp").Should().NotBeNull();
    }
}
