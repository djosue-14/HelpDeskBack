using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.Dashboard;

namespace HelpDesk.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<RequesterDashboardDto>> GetRequesterDashboardAsync(string userId, CancellationToken cancellationToken);
    Task<Result<AgentDashboardDto>> GetAgentDashboardAsync(string userId, CancellationToken cancellationToken);
    Task<Result<CoordinatorDashboardDto>> GetCoordinatorDashboardAsync(string userId, CancellationToken cancellationToken);
    Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken);
    Task<Result<OperationalMetricsDto>> GetOperationalMetricsAsync(DateTime from, DateTime to, int? departmentId, CancellationToken cancellationToken);
    Task<Result<AgentPerformanceDto>> GetAgentPerformanceAsync(string agentUserId, DateTime from, DateTime to, CancellationToken cancellationToken);
    Task<Result<IEnumerable<AgentPerformanceDto>>> GetTeamPerformanceAsync(int departmentId, DateTime from, DateTime to, CancellationToken cancellationToken);
    Task<Result<HeatMapDto>> GetHeatMapAsync(DateTime from, DateTime to, IEnumerable<int>? departmentIds, CancellationToken cancellationToken);
    Task<Result<LeaderboardDto>> GetMonthlyLeaderboardAsync(int year, int month, CancellationToken cancellationToken);
}
