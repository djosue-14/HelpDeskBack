using HelpDesk.Application.DTOs.Ticket;

namespace HelpDesk.Application.DTOs.Dashboard;

public class AgentDashboardDto
{
    public IEnumerable<TicketSummaryDto> Queue { get; set; } = Enumerable.Empty<TicketSummaryDto>();
    public int TotalAssigned { get; set; }
    public int OverdueCount { get; set; }
    public int StaleCount { get; set; }
    public double WorkloadPct { get; set; }
}

public class AgentWorkloadDto
{
    public string UserId { get; set; } = string.Empty;
    public int AssignedCount { get; set; }
    public int OverdueCount { get; set; }
}
