using BlogManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<BlogPost> Posts => Set<BlogPost>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Author> Authors => Set<Author>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // BlogPost configuration
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.ToTable("Posts");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Content)
                .IsRequired();

            entity.Property(e => e.PublishedDate)
                .HasDefaultValueSql("datetime('now')");

            entity.Property(e => e.CoverImage)
                .HasMaxLength(500);

            entity.HasOne(e => e.Author)
                .WithMany(a => a.Posts)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Tags)
                .WithOne(t => t.BlogPost)
                .HasForeignKey(t => t.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => new { e.BlogPostId, e.Name }).IsUnique();
        });

        // Author configuration
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("Authors");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EmailAddress)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(e => e.EmailAddress).IsUnique();
        });
    }
}
