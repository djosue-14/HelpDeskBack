using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class SlaConfigurationConfiguration : IEntityTypeConfiguration<SlaConfiguration>
{
    public void Configure(EntityTypeBuilder<SlaConfiguration> builder)
    {
        builder.HasKey(s => s.SlaConfigurationId);
        builder.Property(s => s.SlaConfigurationId).ValueGeneratedOnAdd();

        builder.Property(s => s.Priority)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.HasIndex(s => s.Priority)
            .IsUnique();

        builder.Property(s => s.HoursLimit)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(s => s.LastModifiedBy)
            .HasMaxLength(25);

        builder.Property(s => s.DisabledBy)
            .HasMaxLength(25);

        builder.ToTable("SlaConfiguration");
    }
}
