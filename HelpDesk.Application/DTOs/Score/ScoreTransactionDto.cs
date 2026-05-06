namespace HelpDesk.Application.DTOs.Score;

public class ScoreTransactionDto
{
    public int ScoreTransactionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int TicketId { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
