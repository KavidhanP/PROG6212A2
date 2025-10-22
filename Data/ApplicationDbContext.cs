namespace PROGA22025.Data
{
    using Microsoft.EntityFrameworkCore;
    using PROGA22025.Models;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        
        public DbSet<Lecturers> Lecturers { get; set; }
        public DbSet<Claims> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Lecturer table
            modelBuilder.Entity<Lecturers>(entity =>
            {
                entity.HasKey(e => e.LecturerId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // Configure Claim table
            modelBuilder.Entity<Claims>(entity =>
            {
                entity.HasKey(e => e.ClaimId);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.SubmittedDate).HasDefaultValueSql("GETDATE()");

                // Relationship with Lecturer (Foreign Key)
                entity.HasOne<Lecturers>()
                    .WithMany()
                    .HasForeignKey(e => e.LecturerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship with SupportingDocuments
                entity.HasMany(e => e.SupportingDocuments)
                    .WithOne()
                    .HasForeignKey(d => d.ClaimId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SupportingDocument table
            modelBuilder.Entity<SupportingDocument>(entity =>
            {
                entity.HasKey(e => e.DocumentId);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileType).HasMaxLength(10);
                entity.Property(e => e.UploadedDate).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
