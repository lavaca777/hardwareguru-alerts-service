using Alerts.Application;
using Alerts.Domain;
using Moq;

namespace Alerts.Tests.Application;

public sealed class AlertServiceTests
{
    [Fact]
    public async Task ProcessAsync_CreatesAlert_WhenUsageIsAboveThreshold()
    {
        var repository = new Mock<IAlertRepository>();
        Alert? captured = null;

        repository
            .Setup(r => r.TryAddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()))
            .Callback<Alert, CancellationToken>((alert, _) => captured = alert)
            .ReturnsAsync(true);

        var service = new AlertService(repository.Object);
        var buildEvent = CreateEvent(estimatedConsumptionW: 504, psuPowerW: 500);

        var result = await service.ProcessAsync(buildEvent, CancellationToken.None);

        Assert.Equal(AlertProcessingResult.Created, result);
        Assert.NotNull(captured);
        Assert.Equal(buildEvent.EventId, captured.EventId);
        Assert.Equal("user-1", captured.UserId);
        Assert.Equal("build-1", captured.BuildId);
        Assert.Equal("PC Gamer", captured.BuildName);
        Assert.Equal(101, captured.UsagePercentage);
        Assert.False(captured.IsRead);
        Assert.Contains("500 W", captured.Message);
        Assert.Contains("101%", captured.Message);

        repository.Verify(
            r => r.TryAddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_DoesNotCreateAlert_AtExactlyEightyFivePercent()
    {
        var repository = new Mock<IAlertRepository>();
        var service = new AlertService(repository.Object);
        var buildEvent = CreateEvent(estimatedConsumptionW: 425, psuPowerW: 500);

        var result = await service.ProcessAsync(buildEvent, CancellationToken.None);

        Assert.Equal(AlertProcessingResult.NoAlert, result);
        repository.Verify(
            r => r.TryAddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(-1, 500)]
    [InlineData(504, 0)]
    public async Task ProcessAsync_ReturnsInvalid_WhenEventDataIsInvalid(
        int estimatedConsumptionW,
        int psuPowerW)
    {
        var repository = new Mock<IAlertRepository>();
        var service = new AlertService(repository.Object);
        var buildEvent = CreateEvent(estimatedConsumptionW, psuPowerW);

        var result = await service.ProcessAsync(buildEvent, CancellationToken.None);

        Assert.Equal(AlertProcessingResult.Invalid, result);
        repository.Verify(
            r => r.TryAddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsDuplicate_WhenRepositoryRejectsEventId()
    {
        var repository = new Mock<IAlertRepository>();
        repository
            .Setup(r => r.TryAddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new AlertService(repository.Object);
        var buildEvent = CreateEvent(estimatedConsumptionW: 504, psuPowerW: 500);

        var result = await service.ProcessAsync(buildEvent, CancellationToken.None);

        Assert.Equal(AlertProcessingResult.Duplicate, result);
    }

    [Fact]
    public async Task ListAsync_DelegatesToRepository()
    {
        var userId = "user-1";
        var expected = new[] { CreateAlert(userId, DateTimeOffset.UtcNow) };
        var repository = new Mock<IAlertRepository>();
        repository
            .Setup(r => r.ListByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = new AlertService(repository.Object);

        var result = await service.ListAsync(userId, CancellationToken.None);

        Assert.Same(expected, result);
        repository.Verify(
            r => r.ListByUserAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ListAsync_Throws_WhenUserIdIsBlank(string userId)
    {
        var repository = new Mock<IAlertRepository>();
        var service = new AlertService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ListAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task MarkAsReadAsync_MarksAlertAndSaves()
    {
        var alert = CreateAlert("user-1", DateTimeOffset.UtcNow);
        var repository = new Mock<IAlertRepository>();
        repository
            .Setup(r => r.GetByIdAsync(alert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alert);

        var service = new AlertService(repository.Object);

        var result = await service.MarkAsReadAsync(alert.Id, CancellationToken.None);

        Assert.Same(alert, result);
        Assert.True(alert.IsRead);
        repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_DoesNotSave_WhenAlertWasAlreadyRead()
    {
        var alert = CreateAlert("user-1", DateTimeOffset.UtcNow);
        alert.MarkAsRead();
        var repository = new Mock<IAlertRepository>();
        repository
            .Setup(r => r.GetByIdAsync(alert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alert);

        var service = new AlertService(repository.Object);

        var result = await service.MarkAsReadAsync(alert.Id, CancellationToken.None);

        Assert.Same(alert, result);
        repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_ReturnsNull_WhenAlertDoesNotExist()
    {
        var repository = new Mock<IAlertRepository>();
        repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert?)null);

        var service = new AlertService(repository.Object);

        var result = await service.MarkAsReadAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
        repository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static BuildSavedEvent CreateEvent(
        int estimatedConsumptionW = 504,
        int psuPowerW = 500)
        => new(
            Guid.NewGuid(),
            "user-1",
            "build-1",
            "PC Gamer",
            estimatedConsumptionW,
            psuPowerW,
            DateTimeOffset.UtcNow);

    private static Alert CreateAlert(string userId, DateTimeOffset createdAt)
    {
        var buildEvent = new BuildSavedEvent(
            Guid.NewGuid(),
            userId,
            "build-1",
            "PC Gamer",
            504,
            500,
            createdAt);

        return Alert.Create(buildEvent, 101, createdAt);
    }
}
