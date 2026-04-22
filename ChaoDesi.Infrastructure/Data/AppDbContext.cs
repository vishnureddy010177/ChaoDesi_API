using ChaoDesi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChaoDesi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserType> UserTypes => Set<UserType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<OtpVerification>(entity =>
        {
            entity.ToTable("OtpVerifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LoginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.OtpCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Purpose).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Token).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.ToTable("UserTypes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
        });
    }
}
