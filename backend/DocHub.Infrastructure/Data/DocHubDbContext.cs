using Microsoft.EntityFrameworkCore;
using DocHub.Core.Entities;

namespace DocHub.Infrastructure.Data;

public class DocHubDbContext : DbContext
{
    public DocHubDbContext(DbContextOptions<DocHubDbContext> options) : base(options)
    {
    }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<DigitalSignature> DigitalSignatures { get; set; }
    public DbSet<LetterTemplate> LetterTemplates { get; set; }
    public DbSet<LetterTemplateField> LetterTemplateFields { get; set; }
    public DbSet<GeneratedLetter> GeneratedLetters { get; set; }
    public DbSet<LetterPreview> LetterPreviews { get; set; }
    public DbSet<LetterAttachment> LetterAttachments { get; set; }
    public DbSet<EmailHistory> EmailHistories { get; set; }
    public DbSet<EmailAttachment> EmailAttachments { get; set; }
    public DbSet<FileUpload> FileUploads { get; set; }
    public DbSet<DynamicTab> DynamicTabs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<NotificationPreferences> NotificationPreferences { get; set; }
    public DbSet<LetterStatusHistory> LetterStatusHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Admin entity
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(100);
            entity.Property(e => e.Permissions).HasMaxLength(1000);
            entity.Property(e => e.LastLoginIp).HasMaxLength(45);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Employee entity
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure DigitalSignature entity
        modelBuilder.Entity<DigitalSignature>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SignatureName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AuthorityName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AuthorityDesignation).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SignatureImagePath).HasMaxLength(500);
            entity.Property(e => e.SignatureData).HasMaxLength(4000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        // Configure LetterTemplate entity
        modelBuilder.Entity<LetterTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LetterType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TemplateContent).IsRequired();
            entity.Property(e => e.TemplateFilePath).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.DataSource).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DatabaseQuery).HasMaxLength(4000);
        });

        // Configure LetterTemplateField entity
        modelBuilder.Entity<LetterTemplateField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FieldName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DefaultValue).HasMaxLength(500);
            entity.Property(e => e.ValidationRules).HasMaxLength(1000);
        });

        // Configure GeneratedLetter entity
        modelBuilder.Entity<GeneratedLetter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LetterNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LetterType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LetterFilePath).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EmailId).HasMaxLength(200);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        });

        // Configure LetterPreview entity
        modelBuilder.Entity<LetterPreview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LetterType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PreviewContent).IsRequired();
            entity.Property(e => e.PreviewFilePath).HasMaxLength(500);
            entity.Property(e => e.PreviewImagePath).HasMaxLength(500);
        });

        // Configure LetterStatusHistory entity
        modelBuilder.Entity<LetterStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LetterId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.OldStatus).HasMaxLength(100).IsRequired();
            entity.Property(e => e.NewStatus).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ChangedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasOne(e => e.Letter)
                .WithMany()
                .HasForeignKey(e => e.LetterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // Configure LetterAttachment entity
        modelBuilder.Entity<LetterAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(100);
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
        });

        // Configure EmailHistory entity
        modelBuilder.Entity<EmailHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ToEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CcEmail).HasMaxLength(255);
            entity.Property(e => e.BccEmail).HasMaxLength(255);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EmailProvider).HasMaxLength(100);
            entity.Property(e => e.EmailId).HasMaxLength(100);
        });

        // Configure EmailAttachment entity
        modelBuilder.Entity<EmailAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(100);
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileSize).IsRequired();
        });

        // Configure FileUpload entity
        modelBuilder.Entity<FileUpload>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocumentType).HasMaxLength(100);
            entity.Property(e => e.AuthorityName).HasMaxLength(100);
            entity.Property(e => e.AuthorityDesignation).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.LetterType).HasMaxLength(100);
            entity.Property(e => e.Version).HasMaxLength(100);
            entity.Property(e => e.ProcessedBy).HasMaxLength(450);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        // Configure PasswordReset entity
        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Token).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsUsed).IsRequired();
            entity.Property(e => e.UsedByIp).HasMaxLength(45);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Token).IsUnique();
        });

        // Configure DynamicTab entity
        modelBuilder.Entity<DynamicTab>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DataSource).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DatabaseQuery).HasMaxLength(4000);
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.RequiredPermission).HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure relationships
        modelBuilder.Entity<LetterTemplate>()
            .HasMany(lt => lt.Fields)
            .WithOne(f => f.LetterTemplate)
            .HasForeignKey(f => f.LetterTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LetterTemplate>()
            .HasMany(lt => lt.GeneratedLetters)
            .WithOne(gl => gl.LetterTemplate)
            .HasForeignKey(gl => gl.LetterTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasMany(e => e.GeneratedLetters)
            .WithOne(gl => gl.Employee)
            .HasForeignKey(gl => gl.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GeneratedLetter>()
            .HasOne(gl => gl.DigitalSignature)
            .WithMany(ds => ds.GeneratedLetters)
            .HasForeignKey(gl => gl.DigitalSignatureId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GeneratedLetter>()
            .HasMany(gl => gl.Attachments)
            .WithOne(la => la.GeneratedLetter)
            .HasForeignKey(la => la.GeneratedLetterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmailHistory>()
            .HasMany(eh => eh.Attachments)
            .WithOne(ea => ea.EmailHistory)
            .HasForeignKey(ea => ea.EmailHistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DynamicTab>()
            .HasMany(dt => dt.LetterTemplates)
            .WithOne(lt => lt.DynamicTab)
            .HasForeignKey(lt => lt.DynamicTabId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure LetterTemplate relationship with DynamicTab
        modelBuilder.Entity<LetterTemplate>()
            .HasOne(lt => lt.DynamicTab)
            .WithMany(dt => dt.LetterTemplates)
            .HasForeignKey(lt => lt.DynamicTabId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure indexes for better performance
        modelBuilder.Entity<EmailHistory>()
            .HasIndex(e => e.SentAt);

        modelBuilder.Entity<GeneratedLetter>()
            .HasIndex(e => e.GeneratedAt);

        modelBuilder.Entity<FileUpload>()
            .HasIndex(e => e.CreatedAt);

        modelBuilder.Entity<DigitalSignature>()
            .HasIndex(e => e.CreatedAt);

        // Configure Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Data).HasMaxLength(4000);
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.SenderId).HasMaxLength(100);
            entity.Property(e => e.GroupName).HasMaxLength(100);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsDelivered);
        });

        // Configure EmailTemplate entity
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TemplateType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.HasIndex(e => e.TemplateName).IsUnique();
            entity.HasIndex(e => e.TemplateType);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure NotificationPreferences entity
        modelBuilder.Entity<NotificationPreferences>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EnabledTypes).HasMaxLength(1000);
            entity.Property(e => e.DisabledTypes).HasMaxLength(1000);
            entity.HasIndex(e => e.UserId).IsUnique();
        });
    }
}
