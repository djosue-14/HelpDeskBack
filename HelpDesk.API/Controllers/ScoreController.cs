using HelpDesk.API.Controllers.Base;
using HelpDesk.Application.DTOs.Score;
using HelpDesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

/// <summary>
/// Consulta de reputación de agentes y calificación de tickets resueltos.
/// </summary>
public class ScoreController : BaseApiController
{
    private readonly IScoreService _scoreService;

    public ScoreController(IScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    /// <summary>Obtiene el perfil de reputación (score) de un usuario.</summary>
    /// <param name="userId">Identificador del usuario (UserId del token).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Perfil de reputación obtenido correctamente.</response>
    /// <response code="404">No existe un perfil de reputación para el usuario indicado.</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserScoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserScore(string userId, CancellationToken cancellationToken)
    {
        var result = await _scoreService.GetByUserIdAsync(userId, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Califica un ticket cerrado y aplica puntos de reputación al agente.</summary>
    /// <remarks>
    /// Solo el solicitante del ticket puede calificarlo y únicamente una vez.
    /// El ticket debe estar en estado `Closed`.
    ///
    /// El `raterUserId` se extrae del token JWT.
    /// </remarks>
    /// <param name="ticketId">Identificador del ticket a calificar.</param>
    /// <param name="request">Indica si la calificación incluye comentario escrito.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Calificación registrada y puntos aplicados correctamente.</response>
    /// <response code="400">El ticket no está cerrado, ya fue calificado, o el usuario no es el solicitante.</response>
    [HttpPost("/api/Tickets/{ticketId:int}/rate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RateTicket(int ticketId, [FromBody] RateTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _scoreService.ApplyRatingPointsAsync(ticketId, GetCurrentUser(), request.HasComment, cancellationToken);
        return HandleResult(result);
    }
}
