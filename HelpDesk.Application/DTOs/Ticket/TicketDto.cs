using HelpDesk.Application.DTOs.TicketAttachment;
using HelpDesk.Application.DTOs.TicketComment;

namespace HelpDesk.Application.DTOs.Ticket;

public class TicketDto
{
    public int TicketId { get; set; }
    public int TicketNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string SupportTypeName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ResolutionCategory { get; set; }
    public string? AssignedAgentUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? FirstOpenedAt { get; set; }
    public DateTime? WorkStartedAt { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int TotalPausedMinutes { get; set; }
    public List<TicketCommentDto> Comments { get; set; } = new();
    public List<TicketAttachmentDto> Attachments { get; set; } = new();
}
