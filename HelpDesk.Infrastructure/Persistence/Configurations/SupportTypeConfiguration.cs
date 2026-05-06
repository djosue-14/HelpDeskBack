using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class SupportTypeConfiguration : IEntityTypeConfiguration<SupportType>
{
    public void Configure(EntityTypeBuilder<SupportType> builder)
    {
        builder.HasKey(st => st.SupportTypeId);
        builder.Property(st => st.SupportTypeId).ValueGeneratedOnAdd();

        builder.Property(st => st.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(st => st.Description)
            .HasMaxLength(500);

        builder.Property(st => st.CreatedBy)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(st => st.LastModifiedBy)
            .HasMaxLength(25);

        builder.Property(st => st.DisabledBy)
            .HasMaxLength(25);

        builder.HasMany(st => st.SupportTypeAgents)
            .WithOne(a => a.SupportType)
            .HasForeignKey(a => a.SupportTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("SupportType");
    }
}
