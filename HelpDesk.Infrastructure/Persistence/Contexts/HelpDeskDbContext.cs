using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Contexts;

public class HelpDeskDbContext : DbContext
{
    public HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<SupportType> SupportTypes => Set<SupportType>();
    public DbSet<SupportTypeAgent> SupportTypeAgents => Set<SupportTypeAgent>();
    public DbSet<SlaConfiguration> SlaConfigurations => Set<SlaConfiguration>();
    public DbSet<ScoreTransaction> ScoreTransactions => Set<ScoreTransaction>();
    public DbSet<UserScore> UserScores => Set<UserScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TicketConfiguration());
        modelBuilder.ApplyConfiguration(new TicketCommentConfiguration());
        modelBuilder.ApplyConfiguration(new TicketAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new TicketHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new SupportTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SupportTypeAgentConfiguration());
        modelBuilder.ApplyConfiguration(new SlaConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new ScoreTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new UserScoreConfiguration());
    }
}
