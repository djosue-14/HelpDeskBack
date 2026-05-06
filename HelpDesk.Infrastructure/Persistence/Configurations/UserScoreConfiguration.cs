using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public class UserScoreConfiguration : IEntityTypeConfiguration<UserScore>
{
    public void Configure(EntityTypeBuilder<UserScore> builder)
    {
        builder.HasKey(us => us.UserScoreId);
        builder.Property(us => us.UserScoreId).ValueGeneratedOnAdd();

        builder.Property(us => us.UserId)
            .IsRequired()
            .HasMaxLength(25);

        builder.HasIndex(us => us.UserId)
            .IsUnique();

        builder.Property(us => us.Level)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.Property(us => us.CurrentPoints)
            .IsRequired();

        builder.HasMany(us => us.ScoreTransactions)
            .WithOne()
            .HasForeignKey(st => st.UserId)
            .HasPrincipalKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("UserScore");
    }
}
