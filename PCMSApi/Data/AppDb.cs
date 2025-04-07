using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PCMSApi.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PCMSApi.Data
{
    /// <summary>
    /// Represents the application's database context.
    /// </summary>
    public class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> options) : base(options) { }

        /// <summary>
        /// Gets or sets the Patients table.
        /// </summary>
        public DbSet<Patient> Patients => Set<Patient>();

        /// <summary>
        /// Gets or sets the Attachments table.
        /// </summary>
        public DbSet<Attachment> Attachments => Set<Attachment>();

        /// <summary>
        /// Gets or sets the MedicalConditions table.
        /// </summary>
        public DbSet<MedicalCondition> Conditions => Set<MedicalCondition>();

        /// <summary>
        /// Configures the model relationships and properties.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("patients");

                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).HasColumnName("id");

                entity.Property(p => p.PatientUid).HasColumnName("patient_uid");
                entity.HasIndex(p => p.PatientUid).IsUnique();

                entity.Property(p => p.Name).HasColumnName("name");
                entity.Property(p => p.Age).HasColumnName("age");
                entity.Property(p => p.ContactPhone).HasColumnName("contact_phone");
                entity.Property(p => p.ContactEmail).HasColumnName("contact_email");
                entity.Property(p => p.ContactAddress).HasColumnName("contact_address");
            });

            // Configure Attachment entity
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("attachments");

                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).HasColumnName("id");

                entity.Property(a => a.AttachmentId).HasColumnName("attachment_uid");
                entity.HasIndex(a => a.AttachmentId).IsUnique();

                entity.Property(a => a.PatientId).HasColumnName("patient_id");
                entity.Property(a => a.FileName).HasColumnName("filename");
                entity.Property(a => a.S3Key).HasColumnName("s3_key");
                entity.Property(a => a.DocumentType).HasColumnName("document_type");
                entity.Property(a => a.UploadedAt).HasColumnName("uploaded_at");

                entity.Property(a => a.Metadata)
                    .HasColumnName("metadata")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new()
                    )
                    .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                        (d1, d2) => JsonSerializer.Serialize(d1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(d2, (JsonSerializerOptions?)null),
                        d => JsonSerializer.Serialize(d, (JsonSerializerOptions?)null).GetHashCode(),
                        d => JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(d, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!
                    ));

                entity.HasOne(a => a.Patient)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(a => a.PatientId)
                    .HasPrincipalKey(p => p.Id) // FK to internal `id`
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure MedicalCondition entity
            modelBuilder.Entity<MedicalCondition>(entity =>
            {
                entity.ToTable("conditions");

                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id");

                entity.Property(c => c.PatientId).HasColumnName("patient_id");
                entity.Property(c => c.Condition).HasColumnName("condition");
                entity.Property(c => c.Notes).HasColumnName("notes");
                entity.Property(c => c.Since).HasColumnName("since");
                entity.Property(c => c.Frequency).HasColumnName("frequency");
                entity.Property(c => c.History).HasColumnName("history");
                entity.Property(c => c.Status).HasColumnName("status");

                entity.HasIndex(c => c.PatientId);
                entity.HasIndex(c => c.Condition);

                entity.HasOne(c => c.Patient)
                    .WithMany(p => p.MedicalHistory)
                    .HasForeignKey(c => c.PatientId)
                    .HasPrincipalKey(p => p.Id) // FK to internal `id`
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}


