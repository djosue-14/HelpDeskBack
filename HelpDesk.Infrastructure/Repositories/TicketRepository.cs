using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(HelpDeskDbContext context) : base(context) { }

    public async Task<Ticket?> GetByTicketNumberAsync(int ticketNumber, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);

    public async Task<Ticket?> GetWithFullDetailAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Department)
            .Include(t => t.SupportType)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.History)
            .FirstOrDefaultAsync(t => t.TicketId == id, cancellationToken);

    public async Task<IEnumerable<Ticket>> GetByAgentAsync(string agentUserId, TicketStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.AssignedUserId == agentUserId && t.IsEnabled);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetOverdueSlaAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(t => t.IsEnabled && t.Deadline < now && t.Status != TicketStatus.Closed)
            .ToListAsync(cancellationToken);
    }
}
