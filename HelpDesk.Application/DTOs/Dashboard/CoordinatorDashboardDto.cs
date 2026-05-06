using HelpDesk.Application.DTOs.Ticket;

namespace HelpDesk.Application.DTOs.Dashboard;

public class CoordinatorDashboardDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalActive { get; set; }
    public int SlaBreachedCount { get; set; }
    public IEnumerable<AgentWorkloadDto> TeamWorkload { get; set; } = Enumerable.Empty<AgentWorkloadDto>();
    public IEnumerable<TicketSummaryDto> EscalatedTickets { get; set; } = Enumerable.Empty<TicketSummaryDto>();
}
