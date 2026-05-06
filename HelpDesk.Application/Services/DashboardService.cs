using AutoMapper;
using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.Dashboard;
using HelpDesk.Application.DTOs.Score;
using HelpDesk.Application.DTOs.Ticket;
using HelpDesk.Application.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Interfaces;

namespace HelpDesk.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DashboardService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<RequesterDashboardDto>> GetRequesterDashboardAsync(string userId, CancellationToken cancellationToken)
    {
        var allTickets = await _unitOfWork.Tickets
            .FindAsync(t => t.CreatedBy == userId && t.IsEnabled, cancellationToken);

        var tickets = allTickets.ToList();

        var activeTickets = tickets
            .Where(t => t.Status != TicketStatus.Closed)
            .ToList();

        var countByStatus = tickets
            .GroupBy(t => t.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var userScore = await _unitOfWork.UserScores
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        return Result<RequesterDashboardDto>.Success(new RequesterDashboardDto
        {
            ActiveTickets = _mapper.Map<IEnumerable<TicketSummaryDto>>(activeTickets),
            TicketCountByStatus = countByStatus,
            Score = userScore is not null ? _mapper.Map<UserScoreDto>(userScore) : new UserScoreDto { UserId = userId }
        });
    }

    public async Task<Result<AgentDashboardDto>> GetAgentDashboardAsync(string userId, CancellationToken cancellationToken)
    {
        var assignedTickets = await _unitOfWork.Tickets
            .FindAsync(t => t.AssignedUserId == userId && t.IsEnabled, cancellationToken);

        var all = assignedTickets.ToList();
        var active = all.Where(t => t.Status != TicketStatus.Closed).ToList();
        var now = DateTime.UtcNow;

        var overdue = active.Count(t => t.Deadline < now);
        var stale = active.Count(t =>
            t.Status == TicketStatus.WaitingForInfo &&
            t.PausedAt.HasValue &&
            (now - t.PausedAt.Value).TotalHours > 48);

        // Workload: percentage of agent's open tickets vs a reference cap of 20
        var workloadPct = active.Count > 0 ? Math.Min(100.0, active.Count / 20.0 * 100) : 0;

        return Result<AgentDashboardDto>.Success(new AgentDashboardDto
        {
            Queue = _mapper.Map<IEnumerable<TicketSummaryDto>>(active),
            TotalAssigned = active.Count,
            OverdueCount = overdue,
            StaleCount = stale,
            WorkloadPct = Math.Round(workloadPct, 1)
        });
    }

    public async Task<Result<CoordinatorDashboardDto>> GetCoordinatorDashboardAsync(string userId, CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.Departments
            .FirstOrDefaultAsync(d => d.CoordinatorUserId == userId && d.IsEnabled, cancellationToken);

        if (department is null)
            return Result<CoordinatorDashboardDto>.Failure("No department found for this coordinator");

        var deptTickets = await _unitOfWork.Tickets
            .FindAsync(t => t.DepartmentId == department.DepartmentId && t.IsEnabled, cancellationToken);

        var active = deptTickets.Where(t => t.Status != TicketStatus.Closed).ToList();
        var now = DateTime.UtcNow;

        var slaBreached = active.Count(t => t.Deadline < now);

        var teamWorkload = active
            .Where(t => t.AssignedUserId is not null)
            .GroupBy(t => t.AssignedUserId!)
            .Select(g => new AgentWorkloadDto
            {
                UserId = g.Key,
                AssignedCount = g.Count(),
                OverdueCount = g.Count(t => t.Deadline < now)
            });

        var escalated = active
            .Where(t => t.Deadline < now || t.Deadline < now.AddHours(4))
            .OrderBy(t => t.Deadline)
            .Take(10);

        return Result<CoordinatorDashboardDto>.Success(new CoordinatorDashboardDto
        {
            DepartmentName = department.Name,
            TotalActive = active.Count,
            SlaBreachedCount = slaBreached,
            TeamWorkload = teamWorkload,
            EscalatedTickets = _mapper.Map<IEnumerable<TicketSummaryDto>>(escalated)
        });
    }

    public async Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken)
    {
        var allTickets = await _unitOfWork.Tickets
            .FindAsync(t => t.IsEnabled, cancellationToken);

        var active = allTickets.Where(t => t.Status != TicketStatus.Closed).ToList();
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var closedToday = allTickets
            .Where(t => t.Status == TicketStatus.Closed &&
                        t.ClosedAt.HasValue &&
                        t.ClosedAt.Value >= today &&
                        t.ClosedAt.Value < tomorrow)
            .ToList();

        var departments = await _unitOfWork.Departments
            .FindAsync(d => d.IsEnabled, cancellationToken);
        var deptMap = departments.ToDictionary(d => d.DepartmentId, d => d.Name);

        var slaDailyPct = closedToday.Count > 0
            ? closedToday.Count(t => t.ClosedAt <= t.Deadline) * 100.0 / closedToday.Count
            : 100.0;

        var countByPriority = active
            .GroupBy(t => t.Priority.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var countByDepartment = active
            .GroupBy(t => deptMap.TryGetValue(t.DepartmentId, out var name) ? name : t.DepartmentId.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return Result<AdminDashboardDto>.Success(new AdminDashboardDto
        {
            TotalActiveAllDepartments = active.Count,
            SlaDailyCompliancePct = Math.Round(slaDailyPct, 1),
            CountByPriority = countByPriority,
            CountByDepartment = countByDepartment
        });
    }

    public async Task<Result<OperationalMetricsDto>> GetOperationalMetricsAsync(DateTime from, DateTime to, int? departmentId, CancellationToken cancellationToken)
    {
        if (from >= to)
            return Result<OperationalMetricsDto>.Failure("'from' must be earlier than 'to'");

        var allTickets = await _unitOfWork.Tickets
            .FindAsync(t => t.IsEnabled, cancellationToken);

        var tickets = allTickets
            .Where(t => departmentId is null || t.DepartmentId == departmentId)
            .ToList();

        var created = tickets.Where(t => t.CreatedAt >= from && t.CreatedAt < to).ToList();
        var closed = tickets.Where(t => t.Status == TicketStatus.Closed && t.ClosedAt >= from && t.ClosedAt < to).ToList();
        var active = tickets.Where(t => t.Status != TicketStatus.Closed).ToList();

        var allHistory = await _unitOfWork.TicketHistories
            .FindAsync(h => h.ExecutedAt >= from && h.ExecutedAt < to, cancellationToken);
        var historyList = allHistory.ToList();

        var reopenedTicketIds = historyList
            .Where(h => h.ActionType == "Reopened")
            .Select(h => h.TicketId)
            .Distinct()
            .ToHashSet();

        var redirectedTicketIds = historyList
            .Where(h => h.ActionType == "Redirected")
            .Select(h => h.TicketId)
            .Distinct()
            .ToHashSet();

        var closedInSla = closed.Count(t => t.ClosedAt <= t.Deadline);
        var slaCompliancePct = closed.Count > 0 ? closedInSla * 100.0 / closed.Count : 100.0;

        var avgResolutionHours = closed.Count > 0
            ? closed.Where(t => t.ClosedAt.HasValue)
                    .Average(t => (t.ClosedAt!.Value - t.RequestedAt).TotalHours)
            : 0;

        var firstResponsed = created.Where(t => t.FirstOpenedAt.HasValue).ToList();
        var avgFirstResponseHours = firstResponsed.Count > 0
            ? firstResponsed.Average(t => (t.FirstOpenedAt!.Value - t.RequestedAt).TotalHours)
            : 0;

        var reopenRate = created.Count > 0
            ? reopenedTicketIds.Intersect(created.Select(t => t.TicketId)).Count() * 100.0 / created.Count
            : 0;

        var redirectRate = created.Count > 0
            ? redirectedTicketIds.Intersect(created.Select(t => t.TicketId)).Count() * 100.0 / created.Count
            : 0;

        var dailyTrend = BuildDailyTrend(created, closed, from, to);

        var slaByPriority = closed
            .GroupBy(t => t.Priority.ToString())
            .ToDictionary(
                g => g.Key,
                g => g.Count() > 0 ? g.Count(t => t.ClosedAt <= t.Deadline) * 100.0 / g.Count() : 100.0);

        var periodLength = to - from;
        var prevFrom = from - periodLength;
        var prevTo = from;
        var prevCreated = tickets.Where(t => t.CreatedAt >= prevFrom && t.CreatedAt < prevTo).ToList();
        var prevClosed = tickets.Where(t => t.Status == TicketStatus.Closed && t.ClosedAt >= prevFrom && t.ClosedAt < prevTo).ToList();
        var prevSla = prevClosed.Count > 0 ? prevClosed.Count(t => t.ClosedAt <= t.Deadline) * 100.0 / prevClosed.Count : 100.0;
        var prevAvgRes = prevClosed.Count > 0 ? prevClosed.Where(t => t.ClosedAt.HasValue).Average(t => (t.ClosedAt!.Value - t.RequestedAt).TotalHours) : 0;

        return Result<OperationalMetricsDto>.Success(new OperationalMetricsDto
        {
            From = from,
            To = to,
            TotalCreated = created.Count,
            TotalClosed = closed.Count,
            TotalActive = active.Count,
            SlaCompliancePct = Math.Round(slaCompliancePct, 1),
            AvgResolutionHours = Math.Round(avgResolutionHours, 1),
            AvgFirstResponseHours = Math.Round(avgFirstResponseHours, 1),
            ReopenRatePct = Math.Round(reopenRate, 1),
            RedirectRatePct = Math.Round(redirectRate, 1),
            DailyTrend = dailyTrend,
            SlaByPriority = slaByPriority.ToDictionary(k => k.Key, v => Math.Round(v.Value, 1)),
            PreviousPeriod = new OperationalMetricsDto
            {
                From = prevFrom,
                To = prevTo,
                TotalCreated = prevCreated.Count,
                TotalClosed = prevClosed.Count,
                TotalActive = 0,
                SlaCompliancePct = Math.Round(prevSla, 1),
                AvgResolutionHours = Math.Round(prevAvgRes, 1)
            }
        });
    }

    public async Task<Result<AgentPerformanceDto>> GetAgentPerformanceAsync(string agentUserId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var agentTickets = await _unitOfWork.Tickets
            .FindAsync(t => t.AssignedUserId == agentUserId && t.IsEnabled, cancellationToken);

        var inPeriod = agentTickets
            .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
            .ToList();

        if (!inPeriod.Any())
            return Result<AgentPerformanceDto>.Failure($"No activity for agent {agentUserId} in the given period");

        var closed = inPeriod.Where(t => t.Status == TicketStatus.Closed && t.ClosedAt.HasValue).ToList();
        var active = agentTickets.Where(t => t.Status != TicketStatus.Closed).ToList();

        var slaCompliance = closed.Count > 0 ? closed.Count(t => t.ClosedAt <= t.Deadline) * 100.0 / closed.Count : 100.0;
        var avgRes = closed.Count > 0 ? closed.Average(t => (t.ClosedAt!.Value - t.RequestedAt).TotalHours) : 0;

        var firstResponsed = inPeriod.Where(t => t.FirstOpenedAt.HasValue).ToList();
        var avgFirstResponse = firstResponsed.Count > 0
            ? firstResponsed.Average(t => (t.FirstOpenedAt!.Value - t.RequestedAt).TotalHours)
            : 0;

        var paused = inPeriod.Where(t => t.TotalPausedMinutes > 0).ToList();
        var avgPause = paused.Count > 0 ? paused.Average(t => t.TotalPausedMinutes) : 0;

        var ratingTransactions = await _unitOfWork.ScoreTransactions
            .FindAsync(st => st.UserId == agentUserId &&
                             st.Reason == ScoreTransactionReason.RatingSubmitted &&
                             st.CreatedAt >= from && st.CreatedAt < to, cancellationToken);

        var ratings = ratingTransactions.ToList();
        var avgRating = ratings.Count > 0 ? ratings.Average(r => r.Points) : 0;

        var stale = active
            .Where(t => t.Status == TicketStatus.WaitingForInfo &&
                        t.PausedAt.HasValue &&
                        (DateTime.UtcNow - t.PausedAt.Value).TotalHours > 48)
            .ToList();

        var teamPerf = await ComputeTeamAvgAsync(inPeriod.FirstOrDefault()?.DepartmentId ?? 0, from, to, agentUserId, cancellationToken);

        return Result<AgentPerformanceDto>.Success(new AgentPerformanceDto
        {
            UserId = agentUserId,
            TotalAssigned = inPeriod.Count,
            TotalClosed = closed.Count,
            TotalActive = active.Count,
            SlaCompliancePct = Math.Round(slaCompliance, 1),
            AvgResolutionHours = Math.Round(avgRes, 1),
            AvgFirstResponseHours = Math.Round(avgFirstResponse, 1),
            PauseCount = paused.Count,
            AvgPauseMinutes = Math.Round(avgPause, 1),
            AvgRating = Math.Round(avgRating, 1),
            TeamAvgSlaCompliancePct = teamPerf.sla,
            TeamAvgResolutionHours = teamPerf.resolution,
            StaleTickets = _mapper.Map<IEnumerable<TicketSummaryDto>>(stale)
        });
    }

    public async Task<Result<IEnumerable<AgentPerformanceDto>>> GetTeamPerformanceAsync(int departmentId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken);
        if (department is null || !department.IsEnabled)
            return Result<IEnumerable<AgentPerformanceDto>>.Failure($"Department {departmentId} not found");

        var deptTickets = await _unitOfWork.Tickets
            .FindAsync(t => t.DepartmentId == departmentId && t.IsEnabled && t.CreatedAt >= from && t.CreatedAt < to, cancellationToken);

        var agentGroups = deptTickets
            .Where(t => t.AssignedUserId is not null)
            .GroupBy(t => t.AssignedUserId!)
            .ToList();

        var results = new List<AgentPerformanceDto>();

        foreach (var group in agentGroups)
        {
            var agentId = group.Key;
            var tickets = group.ToList();
            var closed = tickets.Where(t => t.Status == TicketStatus.Closed && t.ClosedAt.HasValue).ToList();
            var active = tickets.Where(t => t.Status != TicketStatus.Closed).ToList();
            var sla = closed.Count > 0 ? closed.Count(t => t.ClosedAt <= t.Deadline) * 100.0 / closed.Count : 100.0;
            var avgRes = closed.Count > 0 ? closed.Average(t => (t.ClosedAt!.Value - t.RequestedAt).TotalHours) : 0;
            var firstRes = tickets.Where(t => t.FirstOpenedAt.HasValue).ToList();
            var avgFirst = firstRes.Count > 0 ? firstRes.Average(t => (t.FirstOpenedAt!.Value - t.RequestedAt).TotalHours) : 0;
            var paused = tickets.Where(t => t.TotalPausedMinutes > 0).ToList();
            var avgPause = paused.Count > 0 ? paused.Average(t => t.TotalPausedMinutes) : 0;

            results.Add(new AgentPerformanceDto
            {
                UserId = agentId,
                TotalAssigned = tickets.Count,
                TotalClosed = closed.Count,
                TotalActive = active.Count,
                SlaCompliancePct = Math.Round(sla, 1),
                AvgResolutionHours = Math.Round(avgRes, 1),
                AvgFirstResponseHours = Math.Round(avgFirst, 1),
                PauseCount = paused.Count,
                AvgPauseMinutes = Math.Round(avgPause, 1)
            });
        }

        // Populate team averages now that we have all individual results
        var teamSla = results.Count > 0 ? results.Average(r => r.SlaCompliancePct) : 100.0;
        var teamRes = results.Count > 0 ? results.Average(r => r.AvgResolutionHours) : 0;
        foreach (var r in results)
        {
            r.TeamAvgSlaCompliancePct = Math.Round(teamSla, 1);
            r.TeamAvgResolutionHours = Math.Round(teamRes, 1);
        }

        return Result<IEnumerable<AgentPerformanceDto>>.Success(results.OrderByDescending(r => r.SlaCompliancePct));
    }

    public async Task<Result<HeatMapDto>> GetHeatMapAsync(DateTime from, DateTime to, IEnumerable<int>? departmentIds, CancellationToken cancellationToken)
    {
        if (from >= to)
            return Result<HeatMapDto>.Failure("'from' must be earlier than 'to'");

        var deptFilter = departmentIds?.ToHashSet();

        var departments = await _unitOfWork.Departments
            .FindAsync(d => d.IsEnabled && (deptFilter == null || deptFilter.Contains(d.DepartmentId)), cancellationToken);

        var supportTypes = await _unitOfWork.SupportTypes
            .FindAsync(s => s.IsEnabled, cancellationToken);

        var tickets = await _unitOfWork.Tickets
            .FindAsync(t => t.IsEnabled && t.CreatedAt >= from && t.CreatedAt < to, cancellationToken);

        var ticketList = tickets.ToList();
        var stMap = supportTypes.GroupBy(s => s.DepartmentId).ToDictionary(g => g.Key, g => g.ToList());

        var rows = departments.Select(dept =>
        {
            var deptTypes = stMap.TryGetValue(dept.DepartmentId, out var types) ? types : new List<SupportType>();
            var deptTickets = ticketList.Where(t => t.DepartmentId == dept.DepartmentId).ToList();

            var cells = deptTypes.Select(st =>
            {
                var stTickets = deptTickets.Where(t => t.SupportTypeId == st.SupportTypeId).ToList();
                var closed = stTickets.Where(t => t.Status == TicketStatus.Closed && t.ClosedAt.HasValue).ToList();
                var breached = closed.Count(t => t.ClosedAt > t.Deadline);
                var slaBreachPct = closed.Count > 0 ? breached * 100.0 / closed.Count : 0;

                return new HeatMapCellDto
                {
                    SupportTypeId = st.SupportTypeId,
                    SupportTypeName = st.Name,
                    Volume = stTickets.Count,
                    SlaBreachPct = Math.Round(slaBreachPct, 1)
                };
            });

            return new HeatMapRowDto
            {
                DepartmentId = dept.DepartmentId,
                DepartmentName = dept.Name,
                Cells = cells
            };
        });

        return Result<HeatMapDto>.Success(new HeatMapDto
        {
            From = from,
            To = to,
            Rows = rows
        });
    }

    public async Task<Result<LeaderboardDto>> GetMonthlyLeaderboardAsync(int year, int month, CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12)
            return Result<LeaderboardDto>.Failure("Month must be between 1 and 12");

        if (year < 2000 || year > DateTime.UtcNow.Year + 1)
            return Result<LeaderboardDto>.Failure($"Invalid year: {year}");

        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1);

        var transactions = await _unitOfWork.ScoreTransactions
            .FindAsync(t => t.CreatedAt >= from && t.CreatedAt < to, cancellationToken);

        var top10 = transactions
            .GroupBy(t => t.UserId)
            .Select(g =>
            {
                var positivePoints = g.Where(t => t.Points > 0).Sum(t => t.Points);
                var ratingCount = g.Count(t => t.Reason == ScoreTransactionReason.RatingSubmitted);
                var totalCount = g.Count();
                var ratingRate = totalCount > 0 ? ratingCount * 100.0 / totalCount : 0;

                return new { UserId = g.Key, Points = positivePoints, RatingRate = ratingRate };
            })
            .OrderByDescending(x => x.Points)
            .Take(10)
            .Select((x, index) => new LeaderboardEntryDto
            {
                Rank = index + 1,
                UserId = x.UserId,
                PointsEarned = x.Points,
                RatingRatePct = Math.Round(x.RatingRate, 1)
            });

        return Result<LeaderboardDto>.Success(new LeaderboardDto
        {
            Year = year,
            Month = month,
            Top10 = top10
        });
    }

    private static IEnumerable<DailyVolumeDto> BuildDailyTrend(List<Ticket> created, List<Ticket> closed, DateTime from, DateTime to)
    {
        var result = new List<DailyVolumeDto>();
        for (var day = from.Date; day < to.Date; day = day.AddDays(1))
        {
            var next = day.AddDays(1);
            result.Add(new DailyVolumeDto
            {
                Date = DateOnly.FromDateTime(day),
                Created = created.Count(t => t.CreatedAt >= day && t.CreatedAt < next),
                Closed = closed.Count(t => t.ClosedAt.HasValue && t.ClosedAt.Value >= day && t.ClosedAt.Value < next)
            });
        }
        return result;
    }

    private async Task<(double sla, double resolution)> ComputeTeamAvgAsync(int departmentId, DateTime from, DateTime to, string excludeAgentId, CancellationToken cancellationToken)
    {
        if (departmentId == 0)
            return (100.0, 0);

        var deptTickets = await _unitOfWork.Tickets
            .FindAsync(t => t.DepartmentId == departmentId &&
                            t.IsEnabled &&
                            t.AssignedUserId != null &&
                            t.AssignedUserId != excludeAgentId &&
                            t.CreatedAt >= from && t.CreatedAt < to, cancellationToken);

        var closed = deptTickets
            .Where(t => t.Status == TicketStatus.Closed && t.ClosedAt.HasValue)
            .ToList();

        if (!closed.Any())
            return (100.0, 0);

        var sla = closed.Count(t => t.ClosedAt <= t.Deadline) * 100.0 / closed.Count;
        var res = closed.Average(t => (t.ClosedAt!.Value - t.RequestedAt).TotalHours);

        return (Math.Round(sla, 1), Math.Round(res, 1));
    }
}
