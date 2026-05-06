using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.Ticket;

namespace HelpDesk.Application.Interfaces;

public interface ITicketService
{
    Task<Result<TicketDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<IEnumerable<TicketSummaryDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<IEnumerable<TicketSummaryDto>>> GetByAgentAsync(string agentUserId, CancellationToken cancellationToken);
    Task<Result<TicketDto>> CreateAsync(CreateTicketRequest request, string createdBy, CancellationToken cancellationToken);
    Task<Result<TicketDto>> ChangeStatusAsync(int id, UpdateTicketStatusRequest request, string changedBy, CancellationToken cancellationToken);
    Task<Result<TicketDto>> CloseAsync(int id, CloseTicketRequest request, string closedBy, CancellationToken cancellationToken);
    Task<Result<TicketDto>> ReopenAsync(int id, string reason, string reopenedBy, CancellationToken cancellationToken);
    Task<Result<TicketDto>> RedirectAsync(int id, int newSupportTypeId, string redirectedBy, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken);
}
