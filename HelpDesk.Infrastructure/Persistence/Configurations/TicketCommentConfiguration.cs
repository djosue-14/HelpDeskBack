using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.HasKey(c => c.TicketCommentId);
        builder.Property(c => c.TicketCommentId).ValueGeneratedOnAdd();

        builder.Property(c => c.Content)
            .IsRequired();

        builder.Property(c => c.AuthorId)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(c => c.Visibility)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.HasMany(c => c.Attachments)
            .WithOne(a => a.Comment)
            .HasForeignKey(a => a.CommentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("TicketComment");
    }
}
