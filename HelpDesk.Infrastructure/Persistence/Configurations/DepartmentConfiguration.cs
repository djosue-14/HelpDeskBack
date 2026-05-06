using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.DepartmentId);
        builder.Property(d => d.DepartmentId).ValueGeneratedOnAdd();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.CoordinatorUserId)
            .HasMaxLength(25);

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(d => d.LastModifiedBy)
            .HasMaxLength(25);

        builder.Property(d => d.DisabledBy)
            .HasMaxLength(25);

        builder.HasMany(d => d.SupportTypes)
            .WithOne(st => st.Department)
            .HasForeignKey(st => st.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Department");
    }
}
