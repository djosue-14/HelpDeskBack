using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class Ticket
{
    public int TicketId { get; set; }
    public int TicketNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public int SupportTypeId { get; set; }
    public SupportType SupportType { get; set; } = null!;
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public ResolutionCategory? ResolutionCategory { get; set; }
    public string? AssignedUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? FirstOpenedAt { get; set; }
    public DateTime? WorkStartedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime? PausedAt { get; set; }
    public int TotalPausedMinutes { get; set; }
    public int? RelatedTicketId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
}
