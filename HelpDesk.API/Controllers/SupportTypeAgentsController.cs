using HelpDesk.API.Controllers.Base;
using HelpDesk.Application.DTOs.SupportTypeAgent;
using HelpDesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

/// <summary>
/// Asignación y seguimiento de agentes a tipos de soporte.
/// Solo puede existir un agente activo por tipo de soporte en cualquier momento.
/// </summary>
public class SupportTypeAgentsController : BaseApiController
{
    private readonly ISupportTypeAgentService _agentService;

    public SupportTypeAgentsController(ISupportTypeAgentService agentService)
    {
        _agentService = agentService;
    }

    /// <summary>Obtiene la asignación activa de un tipo de soporte.</summary>
    /// <param name="supportTypeId">Identificador del tipo de soporte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Asignación activa encontrada.</response>
    /// <response code="404">El tipo de soporte no tiene agente activo asignado.</response>
    [HttpGet("active/{supportTypeId:int}")]
    [ProducesResponseType(typeof(SupportTypeAgentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActive(int supportTypeId, CancellationToken cancellationToken)
    {
        var result = await _agentService.GetActiveAgentAsync(supportTypeId, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Obtiene el historial completo de asignaciones de un tipo de soporte.</summary>
    /// <param name="supportTypeId">Identificador del tipo de soporte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Historial de asignaciones obtenido correctamente.</response>
    /// <response code="400">Error al recuperar el historial.</response>
    [HttpGet("history/{supportTypeId:int}")]
    [ProducesResponseType(typeof(IEnumerable<SupportTypeAgentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(int supportTypeId, CancellationToken cancellationToken)
    {
        var result = await _agentService.GetHistoryAsync(supportTypeId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Asigna el usuario autenticado como agente activo de un tipo de soporte.</summary>
    /// <remarks>
    /// El `userId` del agente **siempre** se extrae del token JWT — nunca del body.
    /// Si ya existe un agente activo para el tipo de soporte, se realiza su soft-delete automáticamente
    /// antes de crear la nueva asignación (operación atómica en transacción).
    ///
    /// El body solo requiere el `supportTypeId`.
    /// </remarks>
    /// <param name="request">Identificador del tipo de soporte a asignar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Agente asignado correctamente. Devuelve la nueva asignación activa.</response>
    /// <response code="400">El tipo de soporte no existe o error de validación.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SupportTypeAgentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign([FromBody] AssignAgentRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        request.UserId = currentUser;
        var result = await _agentService.AssignAsync(request, currentUser, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Desasigna el agente activo de un tipo de soporte (soft delete).</summary>
    /// <param name="supportTypeId">Identificador del tipo de soporte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Agente desasignado correctamente.</response>
    /// <response code="400">No hay ningún agente activo asignado a este tipo de soporte.</response>
    [HttpDelete("{supportTypeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unassign(int supportTypeId, CancellationToken cancellationToken)
    {
        var result = await _agentService.UnassignAsync(supportTypeId, GetCurrentUser(), cancellationToken);
        return HandleDeleteResult(result);
    }
}
