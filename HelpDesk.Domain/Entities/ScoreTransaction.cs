using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class ScoreTransaction
{
    public int ScoreTransactionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int TicketId { get; set; }
    public int Points { get; set; }
    public ScoreTransactionReason Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
