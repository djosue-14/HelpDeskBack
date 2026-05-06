namespace HelpDesk.Application.DTOs.TicketAttachment;

public class TicketAttachmentDto
{
    public int TicketAttachmentId { get; set; }
    public int TicketId { get; set; }
    public int? CommentId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
