namespace HelpDesk.Application.DTOs.Ticket;

public class CloseTicketRequest
{
    public string ResolutionCategory { get; set; } = string.Empty;
    public string? ClosingComment { get; set; }
}
