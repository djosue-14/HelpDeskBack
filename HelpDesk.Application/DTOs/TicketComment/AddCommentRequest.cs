namespace HelpDesk.Application.DTOs.TicketComment;

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
}
