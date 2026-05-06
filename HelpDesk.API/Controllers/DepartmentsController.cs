using HelpDesk.API.Controllers.Base;
using HelpDesk.Application.DTOs.Department;
using HelpDesk.Application.DTOs.SupportType;
using HelpDesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

/// <summary>
/// Administración de departamentos y consulta de sus tipos de soporte asociados.
/// </summary>
public class DepartmentsController : BaseApiController
{
    private readonly IDepartmentService _departmentService;
    private readonly ISupportTypeService _supportTypeService;

    public DepartmentsController(IDepartmentService departmentService, ISupportTypeService supportTypeService)
    {
        _departmentService = departmentService;
        _supportTypeService = supportTypeService;
    }

    /// <summary>Obtiene todos los departamentos activos.</summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Lista de departamentos obtenida correctamente.</response>
    /// <response code="400">Error al recuperar los departamentos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Obtiene un departamento por su ID.</summary>
    /// <param name="id">Identificador único del departamento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Departamento encontrado.</response>
    /// <response code="404">No existe ningún departamento con el ID indicado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetByIdAsync(id, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Obtiene los tipos de soporte activos de un departamento específico.</summary>
    /// <param name="id">Identificador del departamento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Lista de tipos de soporte del departamento.</response>
    /// <response code="404">El departamento no existe.</response>
    [HttpGet("{id:int}/supporttypes")]
    [ProducesResponseType(typeof(IEnumerable<SupportTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupportTypes(int id, CancellationToken cancellationToken)
    {
        var result = await _supportTypeService.GetByDepartmentAsync(id, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Crea un nuevo departamento.</summary>
    /// <remarks>El nombre del departamento debe ser único y no superar 100 caracteres.</remarks>
    /// <param name="request">Nombre y descripción del departamento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="201">Departamento creado correctamente.</response>
    /// <response code="400">Error de validación (nombre vacío, nombre duplicado, etc.).</response>
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _departmentService.CreateAsync(request, GetCurrentUser(), cancellationToken);
        return HandleCreateResult(result, nameof(GetById), new { id = result.Value?.DepartmentId });
    }

    /// <summary>Realiza el soft delete de un departamento.</summary>
    /// <param name="id">Identificador del departamento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Departamento eliminado correctamente.</response>
    /// <response code="400">No se puede eliminar el departamento (tiene tickets activos u otro conflicto).</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.DeleteAsync(id, GetCurrentUser(), cancellationToken);
        return HandleDeleteResult(result);
    }
}
