using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommentService.Data;

public class CommentDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CommentDbContext>
{
    public CommentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommentDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=comment_db;Username=postgres;Password=postgres");

        return new CommentDbContext(optionsBuilder.Options);
    }
}