using Alerts.Application;
using Alerts.Domain;
using Moq;

namespace Alerts.Tests.Application;

public sealed class BuildSavedEventProcessorTests
{
    private const string ValidPayload = """
        {
          "eventId": "4bb3c2e0-7a1e-4e6a-bc5b-1f86f8f7a7a2",
          "userId": "user-1",
          "buildId": "build-1",
          "buildName": "PC Gamer 1440p",
          "consumoEstimadoW": 504,
          "potenciaFuenteW": 500,
          "ocurridoEn": "2026-11-17T20:00:00Z"
        }
        """;

    [Fact]
    public async Task ProcessAsync_MapsContractAndDelegatesToAlertService()
    {
        var alertService = new Mock<IAlertService>();
        BuildSavedEvent? captured = null;
        alertService
            .Setup(s => s.ProcessAsync(It.IsAny<BuildSavedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<BuildSavedEvent, CancellationToken>((buildEvent, _) => captured = buildEvent)
            .ReturnsAsync(AlertProcessingResult.Created);

        var processor = new BuildSavedEventProcessor(alertService.Object);

        var result = await processor.ProcessAsync(ValidPayload, CancellationToken.None);

        Assert.Equal(AlertProcessingResult.Created, result);
        Assert.NotNull(captured);
        Assert.Equal(Guid.Parse("4bb3c2e0-7a1e-4e6a-bc5b-1f86f8f7a7a2"), captured.EventId);
        Assert.Equal("user-1", captured.UserId);
        Assert.Equal("build-1", captured.BuildId);
        Assert.Equal("PC Gamer 1440p", captured.BuildName);
        Assert.Equal(504, captured.EstimatedConsumptionW);
        Assert.Equal(500, captured.PsuPowerW);
        Assert.Equal(
            DateTimeOffset.Parse("2026-11-17T20:00:00Z"),
            captured.OccurredAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{not-json}")]
    [InlineData("null")]
    public async Task ProcessAsync_ReturnsInvalid_ForMalformedPayload(string payload)
    {
        var alertService = new Mock<IAlertService>();
        var processor = new BuildSavedEventProcessor(alertService.Object);

        var result = await processor.ProcessAsync(payload, CancellationToken.None);

        Assert.Equal(AlertProcessingResult.Invalid, result);
        alertService.Verify(
            s => s.ProcessAsync(It.IsAny<BuildSavedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsInvalid_WhenPayloadIsNull()
    {
        var alertService = new Mock<IAlertService>();
        var processor = new BuildSavedEventProcessor(alertService.Object);

        var result = await processor.ProcessAsync(null!, CancellationToken.None);

        Assert.Equal(AlertProcessingResult.Invalid, result);
        alertService.Verify(
            s => s.ProcessAsync(It.IsAny<BuildSavedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsNoAlert_WhenServiceDecidesNoAlert()
    {
        var alertService = new Mock<IAlertService>();
        alertService
            .Setup(s => s.ProcessAsync(It.IsAny<BuildSavedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AlertProcessingResult.NoAlert);

        var processor = new BuildSavedEventProcessor(alertService.Object);

        var result = await processor.ProcessAsync(ValidPayload, CancellationToken.None);

        Assert.Equal(AlertProcessingResult.NoAlert, result);
    }

    [Fact]
    public async Task ProcessAsync_PropagatesServiceFailure_ForConsumerRetryLogic()
    {
        var alertService = new Mock<IAlertService>();
        alertService
            .Setup(s => s.ProcessAsync(It.IsAny<BuildSavedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        var processor = new BuildSavedEventProcessor(alertService.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => processor.ProcessAsync(ValidPayload, CancellationToken.None));
    }
}
