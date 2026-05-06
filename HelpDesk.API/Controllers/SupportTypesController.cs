using HelpDesk.API.Controllers.Base;
using HelpDesk.Application.DTOs.SupportType;
using HelpDesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

/// <summary>
/// Administración de tipos de soporte disponibles en el sistema.
/// </summary>
public class SupportTypesController : BaseApiController
{
    private readonly ISupportTypeService _supportTypeService;

    public SupportTypesController(ISupportTypeService supportTypeService)
    {
        _supportTypeService = supportTypeService;
    }

    /// <summary>Obtiene todos los tipos de soporte activos.</summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Lista de tipos de soporte obtenida correctamente.</response>
    /// <response code="400">Error al recuperar los tipos de soporte.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SupportTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _supportTypeService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Obtiene un tipo de soporte por su ID.</summary>
    /// <param name="id">Identificador único del tipo de soporte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Tipo de soporte encontrado.</response>
    /// <response code="404">No existe ningún tipo de soporte con el ID indicado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SupportTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _supportTypeService.GetByIdAsync(id, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Crea un nuevo tipo de soporte asociado a un departamento.</summary>
    /// <param name="request">Nombre, descripción y departamento del tipo de soporte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Tipo de soporte creado correctamente.</response>
    /// <response code="400">Error de validación (departamento no existe, nombre vacío, etc.).</response>
    [HttpPost]
    [ProducesResponseType(typeof(SupportTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSupportTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _supportTypeService.CreateAsync(request, GetCurrentUser(), cancellationToken);
        return HandleCreateResult(result, nameof(GetById), new { id = result.Value?.SupportTypeId });
    }

    /// <summary>Realiza el soft delete de un tipo de soporte.</summary>
    /// <param name="id">Identificador del tipo de soporte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Tipo de soporte eliminado correctamente.</response>
    /// <response code="400">No se puede eliminar (tiene tickets activos u otro conflicto).</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _supportTypeService.DeleteAsync(id, GetCurrentUser(), cancellationToken);
        return HandleDeleteResult(result);
    }
}
