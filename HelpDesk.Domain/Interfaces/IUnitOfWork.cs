namespace HelpDesk.Domain.Interfaces;

public interface IUnitOfWork
{
    ITicketRepository Tickets { get; }
    ITicketCommentRepository TicketComments { get; }
    ITicketAttachmentRepository TicketAttachments { get; }
    ITicketHistoryRepository TicketHistories { get; }
    IDepartmentRepository Departments { get; }
    ISupportTypeRepository SupportTypes { get; }
    ISupportTypeAgentRepository SupportTypeAgents { get; }
    ISlaConfigurationRepository SlaConfigurations { get; }
    IScoreTransactionRepository ScoreTransactions { get; }
    IUserScoreRepository UserScores { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
