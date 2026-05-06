using HelpDesk.Application.DTOs.Ticket;

namespace HelpDesk.Application.DTOs.Dashboard;

public class AgentPerformanceDto
{
    public string UserId { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int TotalClosed { get; set; }
    public int TotalActive { get; set; }
    public double SlaCompliancePct { get; set; }
    public double AvgResolutionHours { get; set; }
    public double AvgFirstResponseHours { get; set; }
    public int PauseCount { get; set; }
    public double AvgPauseMinutes { get; set; }
    public double AvgRating { get; set; }
    public double TeamAvgSlaCompliancePct { get; set; }
    public double TeamAvgResolutionHours { get; set; }
    public IEnumerable<TicketSummaryDto> StaleTickets { get; set; } = Enumerable.Empty<TicketSummaryDto>();
}
