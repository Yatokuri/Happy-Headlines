using ArticleService.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Data;

public class ArticleDbContext : DbContext
{
    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("articles");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id)
                .HasMaxLength(64);

            entity.Property(a => a.Title)
                .IsRequired();

            entity.Property(a => a.Content)
                .IsRequired();

            entity.Property(a => a.PublisherId)
                .IsRequired();

            entity.Property(a => a.ScopeType)
                .IsRequired();

            entity.Property(a => a.ScopeValue)
                .IsRequired();

            entity.Property(a => a.CreatedAtUtc)
                .IsRequired();

            entity.Property(a => a.UpdatedAtUtc)
                .IsRequired();
        });
    }
}