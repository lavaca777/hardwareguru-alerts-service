using Alerts.Domain;

namespace Alerts.Tests.Domain;

public sealed class AlertPolicyTests
{
    [Theory]
    [InlineData(425, 500, false)]
    [InlineData(426, 500, true)]
    [InlineData(500, 500, true)]
    [InlineData(0, 500, false)]
    [InlineData(100, 0, false)]
    [InlineData(-1, 500, false)]
    public void ShouldAlert_UsesStrictThreshold(
        int estimatedConsumptionW,
        int psuPowerW,
        bool expected)
    {
        Assert.Equal(expected, AlertPolicy.ShouldAlert(estimatedConsumptionW, psuPowerW));
    }

    [Theory]
    [InlineData(425, 500, 85)]
    [InlineData(426, 500, 85)]
    [InlineData(429, 500, 86)]
    [InlineData(504, 500, 101)]
    [InlineData(0, 500, 0)]
    public void CalculateUsagePercentage_RoundsAwayFromZero(
        int estimatedConsumptionW,
        int psuPowerW,
        int expected)
    {
        Assert.Equal(expected, AlertPolicy.CalculateUsagePercentage(estimatedConsumptionW, psuPowerW));
    }

    [Fact]
    public void CalculateUsagePercentage_Throws_WhenPsuPowerIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AlertPolicy.CalculateUsagePercentage(500, 0));
    }

    [Fact]
    public void CalculateUsagePercentage_Throws_WhenConsumptionIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AlertPolicy.CalculateUsagePercentage(-1, 500));
    }

    [Fact]
    public void BuildMessage_ContainsPsuPowerPercentageAndBuildName()
    {
        var message = AlertPolicy.BuildMessage(500, 101, "PC Gamer 1440p");

        Assert.Equal(
            "Tu fuente de 500 W está al 101% de uso con la build \"PC Gamer 1440p\".",
            message);
    }

    [Fact]
    public void Suggestion_RecommendsUndervoltOrNewPsu()
    {
        Assert.Contains("undervolt", AlertPolicy.BuildSuggestion(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fuente", AlertPolicy.BuildSuggestion(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildSavedEvent_IsValid_WithCompleteData()
    {
        var buildEvent = ValidEvent();

        Assert.True(buildEvent.IsValid());
    }

    [Theory]
    [MemberData(nameof(InvalidEvents))]
    public void BuildSavedEvent_IsInvalid_WhenRequiredDataIsMissing(BuildSavedEvent buildEvent)
    {
        Assert.False(buildEvent.IsValid());
    }

    public static TheoryData<BuildSavedEvent> InvalidEvents => new()
    {
        ValidEvent() with { EventId = Guid.Empty },
        ValidEvent() with { UserId = " " },
        ValidEvent() with { BuildId = "" },
        ValidEvent() with { BuildName = "" },
        ValidEvent() with { EstimatedConsumptionW = -1 },
        ValidEvent() with { PsuPowerW = 0 },
        ValidEvent() with { OccurredAt = default }
    };

    private static BuildSavedEvent ValidEvent()
        => new(
            Guid.NewGuid(),
            "user-1",
            "build-1",
            "PC Gamer",
            504,
            500,
            DateTimeOffset.UtcNow);
}
