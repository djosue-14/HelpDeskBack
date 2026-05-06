namespace HelpDesk.Application.DTOs.Ticket;

public class UpdateTicketStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
}
