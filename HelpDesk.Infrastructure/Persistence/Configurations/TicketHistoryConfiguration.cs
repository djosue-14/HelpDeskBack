using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.HasKey(h => h.TicketHistoryId);
        builder.Property(h => h.TicketHistoryId).ValueGeneratedOnAdd();

        builder.Property(h => h.ExecutedBy)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(h => h.ActionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.ExecutedAt)
            .IsRequired();

        builder.ToTable("TicketHistory");
    }
}
