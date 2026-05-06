namespace HelpDesk.Domain.Entities;

public class TicketHistory
{
    public int TicketHistoryId { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public string ActionType { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public string ExecutedBy { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
}
