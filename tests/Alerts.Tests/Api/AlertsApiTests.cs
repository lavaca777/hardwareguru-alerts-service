using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Alerts.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Alerts.Tests.Api;

public sealed class AlertsApiTests
{
    [Fact]
    public async Task GetAlerts_ReturnsOnlyUserAlerts_NewestFirst()
    {
        using var factory = new AlertsApiFactory();
        var older = CreateAlert(
            "user-1",
            new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
        var newer = CreateAlert(
            "user-1",
            new DateTimeOffset(2026, 1, 2, 10, 0, 0, TimeSpan.Zero));
        var otherUser = CreateAlert(
            "user-2",
            new DateTimeOffset(2026, 1, 3, 10, 0, 0, TimeSpan.Zero));
        await factory.SeedAsync(older, newer, otherUser);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/alerts?userId=user-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = payload.EnumerateArray().ToArray();

        Assert.Equal(2, items.Length);
        Assert.Equal(new[] { newer.Id, older.Id }, items.Select(i => i.GetProperty("id").GetGuid()));
        Assert.Equal("PC Gamer", items[0].GetProperty("buildName").GetString());
        Assert.Equal(101, items[0].GetProperty("porcentajeUso").GetInt32());
        Assert.False(items[0].GetProperty("leida").GetBoolean());
        Assert.True(items[0].TryGetProperty("creadaEn", out _));
        Assert.True(items[0].TryGetProperty("mensaje", out _));
        Assert.True(items[0].TryGetProperty("sugerencia", out _));
    }

    [Fact]
    public async Task GetAlerts_WithoutUserId_ReturnsProblemDetails()
    {
        using var factory = new AlertsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/alerts");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Theory]
    [InlineData("/alerts?userId=")]
    [InlineData("/alerts?userId=%20")]
    public async Task GetAlerts_WithBlankUserId_ReturnsBadRequest(string url)
    {
        using var factory = new AlertsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchRead_MarksAlertAndIsIdempotent()
    {
        using var factory = new AlertsApiFactory();
        var alert = CreateAlert("user-1", DateTimeOffset.UtcNow);
        await factory.SeedAsync(alert);
        using var client = factory.CreateClient();

        var firstResponse = await client.PatchAsync($"/alerts/{alert.Id}/read", null);
        var secondResponse = await client.PatchAsync($"/alerts/{alert.Id}/read", null);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var firstPayload = await firstResponse.Content.ReadFromJsonAsync<JsonElement>();
        var secondPayload = await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(firstPayload.GetProperty("leida").GetBoolean());
        Assert.True(secondPayload.GetProperty("leida").GetBoolean());
        Assert.Equal(alert.Id, firstPayload.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task PatchRead_WithUnknownId_ReturnsNotFound()
    {
        using var factory = new AlertsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PatchAsync($"/alerts/{Guid.NewGuid()}/read", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var factory = new AlertsApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", await response.Content.ReadAsStringAsync());
    }

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
