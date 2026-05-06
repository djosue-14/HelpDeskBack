using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.TicketComment;

namespace HelpDesk.Application.Interfaces;

public interface ITicketCommentService
{
    Task<Result<IEnumerable<TicketCommentDto>>> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken);
    Task<Result<TicketCommentDto>> AddCommentAsync(int ticketId, AddCommentRequest request, string authorId, CancellationToken cancellationToken);
}
