namespace HelpDesk.Application.DTOs.Ticket;

public class TicketSummaryDto
{
    public int TicketId { get; set; }
    public int TicketNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string SupportTypeName { get; set; } = string.Empty;
    public string? AssignedAgentUsername { get; set; }
    public DateTime Deadline { get; set; }
    public double RemainingSlaPct { get; set; }
}
