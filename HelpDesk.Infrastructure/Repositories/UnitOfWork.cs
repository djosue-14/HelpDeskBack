using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace HelpDesk.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly HelpDeskDbContext _context;
    private IDbContextTransaction? _transaction;

    public ITicketRepository Tickets { get; }
    public ITicketCommentRepository TicketComments { get; }
    public ITicketAttachmentRepository TicketAttachments { get; }
    public ITicketHistoryRepository TicketHistories { get; }
    public IDepartmentRepository Departments { get; }
    public ISupportTypeRepository SupportTypes { get; }
    public ISupportTypeAgentRepository SupportTypeAgents { get; }
    public ISlaConfigurationRepository SlaConfigurations { get; }
    public IScoreTransactionRepository ScoreTransactions { get; }
    public IUserScoreRepository UserScores { get; }

    public UnitOfWork(HelpDeskDbContext context)
    {
        _context = context;
        Tickets = new TicketRepository(context);
        TicketComments = new TicketCommentRepository(context);
        TicketAttachments = new TicketAttachmentRepository(context);
        TicketHistories = new TicketHistoryRepository(context);
        Departments = new DepartmentRepository(context);
        SupportTypes = new SupportTypeRepository(context);
        SupportTypeAgents = new SupportTypeAgentRepository(context);
        SlaConfigurations = new SlaConfigurationRepository(context);
        ScoreTransactions = new ScoreTransactionRepository(context);
        UserScores = new UserScoreRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
