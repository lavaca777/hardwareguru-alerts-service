using Alerts.Domain;
using Alerts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Tests.Infrastructure;

public sealed class AlertRepositoryTests
{
    [Fact]
    public async Task TryAddAsync_PersistsAlert()
    {
        var databaseName = NewDatabaseName();
        await using var context = CreateContext(databaseName);
        var repository = new AlertRepository(context);
        var alert = CreateAlert();

        var added = await repository.TryAddAsync(alert, CancellationToken.None);

        Assert.True(added);
        Assert.Equal(1, await context.Alerts.CountAsync());
    }

    [Fact]
    public async Task TryAddAsync_ReturnsFalse_ForDuplicateEventId()
    {
        var databaseName = NewDatabaseName();
        await using var context = CreateContext(databaseName);
        var repository = new AlertRepository(context);
        var eventId = Guid.NewGuid();
        var first = CreateAlert(eventId);
        var duplicate = CreateAlert(eventId);

        var firstAdded = await repository.TryAddAsync(first, CancellationToken.None);
        var duplicateAdded = await repository.TryAddAsync(duplicate, CancellationToken.None);

        Assert.True(firstAdded);
        Assert.False(duplicateAdded);
        Assert.Equal(1, await context.Alerts.CountAsync());
    }

    [Fact]
    public async Task ListByUserAsync_ReturnsOnlyUserAlerts_NewestFirst()
    {
        var databaseName = NewDatabaseName();
        await using var context = CreateContext(databaseName);
        var older = CreateAlert(createdAt: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
        var newer = CreateAlert(createdAt: new DateTimeOffset(2026, 1, 2, 10, 0, 0, TimeSpan.Zero));
        var otherUser = CreateAlert(userId: "user-2", createdAt: new DateTimeOffset(2026, 1, 3, 10, 0, 0, TimeSpan.Zero));

        context.Alerts.AddRange(older, newer, otherUser);
        await context.SaveChangesAsync();

        var repository = new AlertRepository(context);

        var result = await repository.ListByUserAsync("user-1", CancellationToken.None);

        Assert.Equal(new[] { newer.Id, older.Id }, result.Select(a => a.Id));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAlert()
    {
        var databaseName = NewDatabaseName();
        await using var context = CreateContext(databaseName);
        var alert = CreateAlert();
        context.Alerts.Add(alert);
        await context.SaveChangesAsync();

        var repository = new AlertRepository(context);

        var result = await repository.GetByIdAsync(alert.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(alert.Id, result.Id);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsReadState()
    {
        var databaseName = NewDatabaseName();
        var alert = CreateAlert();

        await using (var context = CreateContext(databaseName))
        {
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext(databaseName))
        {
            var repository = new AlertRepository(context);
            var tracked = await repository.GetByIdAsync(alert.Id, CancellationToken.None);
            Assert.NotNull(tracked);

            tracked.MarkAsRead();
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var verificationContext = CreateContext(databaseName);
        var saved = await verificationContext.Alerts.SingleAsync(a => a.Id == alert.Id);
        Assert.True(saved.IsRead);
    }

    private static AlertsDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AlertsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AlertsDbContext(options);
    }

    private static Alert CreateAlert(
        Guid? eventId = null,
        string userId = "user-1",
        DateTimeOffset? createdAt = null)
    {
        var occurredAt = createdAt ?? DateTimeOffset.UtcNow;
        var buildEvent = new BuildSavedEvent(
            eventId ?? Guid.NewGuid(),
            userId,
            "build-1",
            "PC Gamer",
            504,
            500,
            occurredAt);

        return Alert.Create(buildEvent, 101, occurredAt);
    }

    private static string NewDatabaseName() => $"alerts-repository-{Guid.NewGuid()}";
}
