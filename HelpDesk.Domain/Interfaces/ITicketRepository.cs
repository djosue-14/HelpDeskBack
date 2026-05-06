using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<Ticket?> GetByTicketNumberAsync(int ticketNumber, CancellationToken cancellationToken = default);
    Task<Ticket?> GetWithFullDetailAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByAgentAsync(string agentUserId, TicketStatus? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetOverdueSlaAsync(CancellationToken cancellationToken = default);
}
