using HelpDesk.API.Controllers.Base;
using HelpDesk.Application.DTOs.Dashboard;
using HelpDesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

/// <summary>
/// Métricas operativas, análisis de rendimiento y vistas personalizadas por rol de usuario.
/// </summary>
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Vista principal del dashboard personalizada según el rol del usuario autenticado.</summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Dashboard obtenido correctamente (estructura varía según el rol).</response>
    /// <response code="400">Error al generar el dashboard.</response>
    /// <response code="403">El rol del usuario no tiene dashboard asignado.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUser();
        var role = GetCurrentRole();

        return role switch
        {
            "Requester" => await GetRequesterDashboard(userId, cancellationToken),
            "SupportAgent" => await GetAgentDashboard(userId, cancellationToken),
            "Coordinator" => await GetCoordinatorDashboard(userId, cancellationToken),
            "Administrator" => await GetAdminDashboard(cancellationToken),
            _ => Forbid()
        };
    }

    /// <summary>KPIs operativos del sistema para un período y departamento específicos.</summary>
    /// <param name="from">Fecha de inicio del período (UTC).</param>
    /// <param name="to">Fecha de fin del período (UTC).</param>
    /// <param name="departmentId">Filtro opcional por departamento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Métricas operativas calculadas correctamente.</response>
    /// <response code="400">Rango de fechas inválido.</response>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(OperationalMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? departmentId,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetOperationalMetricsAsync(from, to, departmentId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Indicadores de rendimiento individual de un agente en un período.</summary>
    /// <param name="agentId">Identificador del agente (UserId).</param>
    /// <param name="from">Fecha de inicio del período (UTC).</param>
    /// <param name="to">Fecha de fin del período (UTC).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Rendimiento del agente calculado correctamente.</response>
    /// <response code="404">El agente no existe o no tiene actividad en el período.</response>
    [HttpGet("agents/{agentId}/performance")]
    [ProducesResponseType(typeof(AgentPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAgentPerformance(
        string agentId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAgentPerformanceAsync(agentId, from, to, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Tabla comparativa de rendimiento de todos los agentes de un departamento.</summary>
    /// <param name="departmentId">Identificador del departamento.</param>
    /// <param name="from">Fecha de inicio del período (UTC).</param>
    /// <param name="to">Fecha de fin del período (UTC).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Rendimiento del equipo calculado correctamente.</response>
    /// <response code="404">El departamento no existe.</response>
    [HttpGet("departments/{departmentId:int}/team-performance")]
    [ProducesResponseType(typeof(IEnumerable<AgentPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamPerformance(
        int departmentId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetTeamPerformanceAsync(departmentId, from, to, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Mapa de calor de volumen de tickets por departamento y tipo de soporte.</summary>
    /// <param name="from">Fecha de inicio del período (UTC).</param>
    /// <param name="to">Fecha de fin del período (UTC).</param>
    /// <param name="departmentIds">Filtro opcional de departamentos específicos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Mapa de calor calculado correctamente.</response>
    /// <response code="400">Rango de fechas inválido.</response>
    [HttpGet("heatmap")]
    [ProducesResponseType(typeof(HeatMapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHeatMap(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] IEnumerable<int>? departmentIds,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetHeatMapAsync(from, to, departmentIds, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Ranking mensual de reputación — top 10 agentes por puntos.</summary>
    /// <param name="year">Año del ranking (ej. 2026).</param>
    /// <param name="month">Mes del ranking (1–12).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="200">Ranking mensual calculado correctamente.</response>
    /// <response code="400">Año o mes fuera de rango.</response>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(LeaderboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetMonthlyLeaderboardAsync(year, month, cancellationToken);
        return HandleResult(result);
    }

    private async Task<IActionResult> GetRequesterDashboard(string userId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetRequesterDashboardAsync(userId, cancellationToken);
        return HandleResult(result);
    }

    private async Task<IActionResult> GetAgentDashboard(string userId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAgentDashboardAsync(userId, cancellationToken);
        return HandleResult(result);
    }

    private async Task<IActionResult> GetCoordinatorDashboard(string userId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetCoordinatorDashboardAsync(userId, cancellationToken);
        return HandleResult(result);
    }

    private async Task<IActionResult> GetAdminDashboard(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAdminDashboardAsync(cancellationToken);
        return HandleResult(result);
    }
}
