using Microsoft.EntityFrameworkCore;

namespace ChaoDesi.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureUserProfileSchemaAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF OBJECT_ID(N'dbo.UserProfiles', N'U') IS NOT NULL
               AND COL_LENGTH(N'dbo.UserProfiles', N'CoverImageUrl') IS NULL
            BEGIN
                ALTER TABLE [dbo].[UserProfiles]
                ADD [CoverImageUrl] NVARCHAR(500) NULL;
            END
            """;

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
