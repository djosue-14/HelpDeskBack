using HelpDesk.API.Controllers.Base;
using HelpDesk.Application.DTOs.TicketComment;
using HelpDesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

/// <summary>
/// Gestión del hilo de comentarios y notas internas de un ticket.
/// </summary>
[Route("api/Tickets/{ticketId:int}/comments")]
public class TicketCommentsController : BaseApiController
{
    private readonly ITicketCommentService _commentService;

    public TicketCommentsController(ITicketCommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>Obtiene todos los comentarios de un ticket.</summary>
    /// <remarks>
    /// Los comentarios de visibilidad `Internal` solo son visibles para agentes y coordinadores.
    /// Los solicitantes solo ven comentarios `Public`.
    /// </remarks>
    /// <param name="ticketId">Identificador del ticket.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Lista de comentarios obtenida correctamente.</response>
    /// <response code="404">El ticket no existe o no está accesible para el usuario.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTicket(int ticketId, CancellationToken cancellationToken)
    {
        var result = await _commentService.GetByTicketIdAsync(ticketId, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Agrega un nuevo comentario o nota interna a un ticket.</summary>
    /// <remarks>
    /// El campo `visibility` acepta: `Public` (visible para todos) o `Internal` (solo agentes/coordinadores).
    ///
    /// El `authorId` se extrae del token JWT; nunca se acepta en el body.
    /// </remarks>
    /// <param name="ticketId">Identificador del ticket al que pertenece el comentario.</param>
    /// <param name="request">Contenido y visibilidad del comentario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Comentario agregado correctamente.</response>
    /// <response code="400">Error de validación (contenido vacío, visibilidad inválida, etc.).</response>
    [HttpPost]
    [ProducesResponseType(typeof(TicketCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddComment(int ticketId, [FromBody] AddCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _commentService.AddCommentAsync(ticketId, request, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }
}
