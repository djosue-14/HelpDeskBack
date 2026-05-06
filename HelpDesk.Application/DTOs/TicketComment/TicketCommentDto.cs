using HelpDesk.Application.DTOs.TicketAttachment;

namespace HelpDesk.Application.DTOs.TicketComment;

public class TicketCommentDto
{
    public int TicketCommentId { get; set; }
    public int TicketId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<TicketAttachmentDto> Attachments { get; set; } = new();
}
