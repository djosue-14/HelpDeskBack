using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class SupportTypeAgentConfiguration : IEntityTypeConfiguration<SupportTypeAgent>
{
    public void Configure(EntityTypeBuilder<SupportTypeAgent> builder)
    {
        builder.HasKey(a => a.SupportTypeAgentId);
        builder.Property(a => a.SupportTypeAgentId).ValueGeneratedOnAdd();

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(a => a.LastModifiedBy)
            .HasMaxLength(25);

        builder.Property(a => a.DisabledBy)
            .HasMaxLength(25);

        // Solo un agente activo por tipo de soporte (índice filtrado)
        builder.HasIndex(a => a.SupportTypeId)
            .IsUnique()
            .HasDatabaseName("UX_SupportTypeAgent_SupportTypeId_Active")
            .HasFilter("([IsEnabled]=(1))");

        builder.ToTable("SupportTypeAgent");
    }
}
