using ChaoDesi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChaoDesi.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.Country)
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.AddressLine1)
            .HasMaxLength(250);

        builder.Property(x => x.AddressLine2)
            .HasMaxLength(250);

        builder.Property(x => x.ZipCode)
            .HasMaxLength(20);

        builder.Property(x => x.FacebookUrl)
            .HasMaxLength(500);

        builder.Property(x => x.TwitterUrl)
            .HasMaxLength(500);

        builder.Property(x => x.YoutubeUrl)
            .HasMaxLength(500);

        builder.Property(x => x.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(x => x.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.CoverImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.PhotoIdProofUrl)
            .HasMaxLength(500);

        builder.HasOne(x => x.User)
            .WithOne(x => x.UserProfile)
            .HasForeignKey<UserProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
