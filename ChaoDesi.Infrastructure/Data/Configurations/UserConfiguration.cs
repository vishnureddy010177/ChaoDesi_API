using ChaoDesi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChaoDesi.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.MobileNumber)
            .HasMaxLength(20);

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();
    }
}
