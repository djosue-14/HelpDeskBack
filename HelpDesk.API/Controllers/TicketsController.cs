using HelpDesk.API.Controllers.Base;
using HelpDesk.Application.DTOs.Ticket;
using HelpDesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

/// <summary>
/// Gestión del ciclo de vida completo de los tickets de soporte.
/// </summary>
public class TicketsController : BaseApiController
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>Obtiene todos los tickets del sistema.</summary>
    /// <remarks>Devuelve un listado resumido. Los agentes solo ven sus tickets asignados; administradores ven todos.</remarks>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Listado de tickets obtenido correctamente.</response>
    /// <response code="400">Error al recuperar los tickets.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _ticketService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Obtiene el detalle completo de un ticket por su ID.</summary>
    /// <param name="id">Identificador único del ticket.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Ticket encontrado con todos sus detalles, comentarios y adjuntos.</response>
    /// <response code="404">No existe ningún ticket con el ID indicado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _ticketService.GetByIdAsync(id, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Crea un nuevo ticket de soporte.</summary>
    /// <remarks>
    /// El sistema asigna automáticamente:
    /// - **TicketNumber** secuencial
    /// - **Deadline** según la configuración SLA de la prioridad indicada
    /// - **AssignedAgent** según el agente activo del tipo de soporte
    ///
    /// El campo `createdBy` se extrae del token JWT.
    /// </remarks>
    /// <param name="request">Datos del ticket a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Ticket creado correctamente. Devuelve el ticket con todos sus campos calculados.</response>
    /// <response code="400">Error de validación o referencia inválida (departamento/tipo de soporte no encontrado).</response>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _ticketService.CreateAsync(request, GetCurrentUser(), cancellationToken);
        return HandleCreateResult(result, nameof(GetById), new { id = result.Value?.TicketId });
    }

    /// <summary>Cambia el estado de un ticket.</summary>
    /// <param name="id">Identificador del ticket.</param>
    /// <param name="request">Nuevo estado a aplicar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Estado actualizado correctamente.</response>
    /// <response code="400">Transición de estado inválida o estado desconocido.</response>
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _ticketService.ChangeStatusAsync(id, request, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Cierra un ticket indicando la categoría de resolución.</summary>
    /// <param name="id">Identificador del ticket.</param>
    /// <param name="request">Categoría de resolución y comentario de cierre.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Ticket cerrado correctamente.</response>
    /// <response code="400">Error de validación o el ticket ya está cerrado.</response>
    [HttpPut("{id:int}/close")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Close(int id, [FromBody] CloseTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _ticketService.CloseAsync(id, request, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Reabre un ticket cerrado dentro del período de gracia.</summary>
    /// <param name="id">Identificador del ticket.</param>
    /// <param name="request">Motivo de reapertura.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Ticket reabierto correctamente.</response>
    /// <response code="400">El ticket no puede reabrirse (estado incorrecto o período de gracia expirado).</response>
    [HttpPut("{id:int}/reopen")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reopen(int id, [FromBody] ReopenTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _ticketService.ReopenAsync(id, request.Reason, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Redirige un ticket a un tipo de soporte diferente.</summary>
    /// <param name="id">Identificador del ticket.</param>
    /// <param name="request">Nuevo tipo de soporte de destino.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Ticket redirigido correctamente.</response>
    /// <response code="400">El tipo de soporte destino no existe o es el mismo que el actual.</response>
    [HttpPut("{id:int}/redirect")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Redirect(int id, [FromBody] RedirectTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _ticketService.RedirectAsync(id, request.NewSupportTypeId, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Realiza el soft delete de un ticket (deshabilita sin eliminar de la BD).</summary>
    /// <param name="id">Identificador del ticket.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Ticket eliminado (deshabilitado) correctamente.</response>
    /// <response code="400">No se pudo eliminar el ticket.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _ticketService.DeleteAsync(id, GetCurrentUser(), cancellationToken);
        return HandleDeleteResult(result);
    }
}
