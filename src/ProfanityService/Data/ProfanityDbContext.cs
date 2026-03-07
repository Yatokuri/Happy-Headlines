using Microsoft.EntityFrameworkCore;
using ProfanityService.Models;

namespace ProfanityService.Data;

public class ProfanityDbContext(DbContextOptions<ProfanityDbContext> options) : DbContext(options)
{
    public DbSet<ProfanityWord> ProfanityWords => Set<ProfanityWord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProfanityWord>(entity =>
        {
            entity.ToTable("profanity_words");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Word)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.Word)
                .IsUnique();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
    }
}