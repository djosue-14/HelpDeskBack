using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class ScoreTransactionConfiguration : IEntityTypeConfiguration<ScoreTransaction>
{
    public void Configure(EntityTypeBuilder<ScoreTransaction> builder)
    {
        builder.HasKey(st => st.ScoreTransactionId);
        builder.Property(st => st.ScoreTransactionId).ValueGeneratedOnAdd();

        builder.Property(st => st.UserId)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(st => st.Reason)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.Property(st => st.Points)
            .IsRequired();

        builder.Property(st => st.CreatedAt)
            .IsRequired();

        builder.ToTable("ScoreTransaction");
    }
}
