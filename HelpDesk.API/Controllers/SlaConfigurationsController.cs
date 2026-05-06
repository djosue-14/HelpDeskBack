using HelpDesk.API.Controllers.Base;
using HelpDesk.Application.DTOs.SlaConfiguration;
using HelpDesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

/// <summary>
/// Configuración de los tiempos límite SLA (Service Level Agreement) por prioridad de ticket.
/// </summary>
public class SlaConfigurationsController : BaseApiController
{
    private readonly ISlaConfigurationService _slaService;

    public SlaConfigurationsController(ISlaConfigurationService slaService)
    {
        _slaService = slaService;
    }

    /// <summary>Obtiene la configuración SLA de todas las prioridades.</summary>
    /// <remarks>
    /// Devuelve las configuraciones para los 4 niveles de prioridad:
    /// `Critical`, `High`, `Medium`, `Low`.
    /// El `hoursLimit` indica cuántas horas tiene el agente para resolver el ticket.
    /// </remarks>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Configuraciones SLA obtenidas correctamente.</response>
    /// <response code="400">Error al recuperar las configuraciones.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SlaConfigurationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _slaService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Actualiza el tiempo límite SLA para una prioridad específica.</summary>
    /// <remarks>
    /// El parámetro `priority` en la ruta acepta los valores: `Critical`, `High`, `Medium`, `Low`.
    ///
    /// Ejemplo: `PUT /api/SlaConfigurations/High` con body `{ "hoursLimit": 6 }`.
    ///
    /// El `hoursLimit` debe ser un entero positivo mayor que cero.
    /// </remarks>
    /// <param name="priority">Nivel de prioridad a actualizar (`Critical`, `High`, `Medium`, `Low`).</param>
    /// <param name="request">Nuevo valor de horas límite.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Configuración SLA actualizada correctamente.</response>
    /// <response code="400">Prioridad inválida o `hoursLimit` menor o igual a cero.</response>
    [HttpPut("{priority}")]
    [ProducesResponseType(typeof(SlaConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string priority, [FromBody] UpdateSlaConfigurationRequest request, CancellationToken cancellationToken)
    {
        var result = await _slaService.UpdateAsync(priority, request, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }
}
