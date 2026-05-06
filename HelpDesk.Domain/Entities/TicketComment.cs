using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class TicketComment
{
    public int TicketCommentId { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public CommentVisibility Visibility { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}
