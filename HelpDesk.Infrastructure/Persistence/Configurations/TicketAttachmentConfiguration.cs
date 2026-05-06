using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.HasKey(a => a.TicketAttachmentId);
        builder.Property(a => a.TicketAttachmentId).ValueGeneratedOnAdd();

        builder.Property(a => a.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(a => a.FileExtension)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.UploadedBy)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(a => a.FileSizeBytes)
            .IsRequired();

        builder.Property(a => a.UploadedAt)
            .IsRequired();

        builder.ToTable("TicketAttachment");
    }
}
