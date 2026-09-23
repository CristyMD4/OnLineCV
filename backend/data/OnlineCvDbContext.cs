using Microsoft.EntityFrameworkCore;
using OnlineCV.Api.Models;

namespace OnlineCV.Api.Data;

public sealed class OnlineCvDbContext(DbContextOptions<OnlineCvDbContext> options) : DbContext(options)
{
    public DbSet<CvDocumentRecord> CvDocuments => Set<CvDocumentRecord>();
    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CvDocumentRecord>(entity =>
        {
            entity.ToTable("CvDocuments", table =>
                table.HasCheckConstraint("CK_CvDocuments_ContentJson_IsJson", "ISJSON([ContentJson]) = 1"));
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ContentJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(item => item.UpdatedAt).HasPrecision(0);
        });

        modelBuilder.Entity<ContactSubmission>(entity =>
        {
            entity.ToTable("ContactSubmissions");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Email).HasMaxLength(254).IsRequired();
            entity.Property(item => item.Message).HasMaxLength(5000).IsRequired();
            entity.Property(item => item.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(item => item.CreatedAt).HasPrecision(0);
            entity.Property(item => item.UpdatedAt).HasPrecision(0);
            entity.HasIndex(item => item.CreatedAt);
            entity.HasIndex(item => item.Status);
        });
    }
}

public sealed class CvDocumentRecord
{
    public int Id { get; set; }
    public string ContentJson { get; set; } = "";
    public DateTimeOffset UpdatedAt { get; set; }
}
