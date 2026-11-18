using Alerts.Api.Contracts;
using Alerts.Application;
using Microsoft.AspNetCore.Mvc;

namespace Alerts.Api.Controllers;

[ApiController]
[Route("alerts")]
public sealed class AlertsController(IAlertService alertService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AlertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> List(
        [FromQuery] string? userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "userId es requerido",
                Detail = "El gateway debe enviar el userId del usuario autenticado."
            });
        }

        var alerts = await alertService.ListAsync(userId.Trim(), cancellationToken);

        return Ok(alerts.Select(AlertResponse.From).ToArray());
    }

    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResponse>> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var alert = await alertService.MarkAsReadAsync(id, cancellationToken);

        if (alert is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Alerta no encontrada",
                Detail = $"No existe una alerta con id {id}."
            });
        }

        return Ok(AlertResponse.From(alert));
    }
}
