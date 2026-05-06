using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.TicketId);
        builder.Property(t => t.TicketId).ValueGeneratedOnAdd();

        builder.Property(t => t.TicketNumber)
            .IsRequired();

        builder.HasIndex(t => t.TicketNumber)
            .IsUnique();

        builder.Property(t => t.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired();

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.Property(t => t.Status)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.Property(t => t.ResolutionCategory)
            .HasColumnType("tinyint");

        builder.Property(t => t.AssignedUserId)
            .HasMaxLength(25);

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(t => t.TotalPausedMinutes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(t => new { t.Status, t.Priority });
        builder.HasIndex(t => t.Deadline);

        builder.HasOne(t => t.Department)
            .WithMany(d => d.Tickets)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.SupportType)
            .WithMany(st => st.Tickets)
            .HasForeignKey(t => t.SupportTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.History)
            .WithOne(h => h.Ticket)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Ticket");
    }
}
