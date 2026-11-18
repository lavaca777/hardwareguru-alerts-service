namespace Alerts.Domain;

public sealed class Alert
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid EventId { get; init; }

    public string UserId { get; init; } = string.Empty;

    public string BuildId { get; init; } = string.Empty;

    public string BuildName { get; init; } = string.Empty;

    public int UsagePercentage { get; init; }

    public string Message { get; init; } = string.Empty;

    public string Suggestion { get; init; } = string.Empty;

    public bool IsRead { get; private set; }

    public DateTimeOffset CreatedAt { get; init; }

    public static Alert Create(
        BuildSavedEvent buildEvent,
        int usagePercentage,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(buildEvent);

        return new Alert
        {
            EventId = buildEvent.EventId,
            UserId = buildEvent.UserId,
            BuildId = buildEvent.BuildId,
            BuildName = buildEvent.BuildName,
            UsagePercentage = usagePercentage,
            Message = AlertPolicy.BuildMessage(
                buildEvent.PsuPowerW,
                usagePercentage,
                buildEvent.BuildName),
            Suggestion = AlertPolicy.BuildSuggestion(),
            IsRead = false,
            CreatedAt = createdAt
        };
    }

    public void MarkAsRead() => IsRead = true;
}
