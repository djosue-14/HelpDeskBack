namespace HelpDesk.Application.DTOs.Ticket;

public class CreateTicketRequest
{
    public int DepartmentId { get; set; }
    public int SupportTypeId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
