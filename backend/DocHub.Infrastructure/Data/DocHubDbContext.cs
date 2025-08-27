using Microsoft.EntityFrameworkCore;
using DocHub.Core.Entities;

namespace DocHub.Infrastructure.Data;

public class DocHubDbContext : DbContext
{
    public DocHubDbContext(DbContextOptions<DocHubDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<LetterTemplate> LetterTemplates { get; set; }
    public DbSet<LetterTemplateField> LetterTemplateFields { get; set; }
    public DbSet<GeneratedLetter> GeneratedLetters { get; set; }
    public DbSet<DigitalSignature> DigitalSignatures { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<EmailHistory> EmailHistories { get; set; }
    public DbSet<EmailAttachment> EmailAttachments { get; set; }
    public DbSet<DocumentInfo> Files { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationPreferences> NotificationPreferences { get; set; }
    public DbSet<PasswordReset> PasswordResets { get; set; }
    public DbSet<LetterStatusHistory> LetterStatusHistories { get; set; }

    // Dynamic tab entities
    public DbSet<DynamicTab> DynamicTabs { get; set; }
    public DbSet<DynamicTabField> DynamicTabFields { get; set; }
    public DbSet<DynamicTabData> DynamicTabData { get; set; }
    public DbSet<LetterPreview> LetterPreviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Admin entity
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Employee entity
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });



        // Configure LetterTemplate entity
        modelBuilder.Entity<LetterTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LetterType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TemplateContent).IsRequired();
            entity.Property(e => e.TemplateFilePath);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.DataSource).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DatabaseQuery);
            entity.HasIndex(e => e.LetterType);
        });

        // Configure GeneratedLetter entity
        modelBuilder.Entity<GeneratedLetter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.LetterNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LetterType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LetterTemplateId).IsRequired();
            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.DigitalSignatureId);
            entity.Property(e => e.LetterFilePath);
            entity.Property(e => e.Status).IsRequired();
            entity.HasOne<LetterTemplate>().WithMany().HasForeignKey(e => e.LetterTemplateId);
            entity.HasOne<Employee>().WithMany().HasForeignKey(e => e.EmployeeId);
            entity.HasOne<DigitalSignature>().WithMany(d => d.GeneratedLetters).HasForeignKey(e => e.DigitalSignatureId);
        });

        // Configure DigitalSignature entity
        modelBuilder.Entity<DigitalSignature>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SignatureName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AuthorityName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AuthorityDesignation).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.SignatureType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SignatureHash).IsRequired();
            entity.Property(e => e.SignaturePurpose).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SignatureImagePath);
            entity.Property(e => e.SignatureData);
            entity.Property(e => e.ExpiresAt);
            entity.Property(e => e.IsActive);
            entity.Property(e => e.SortOrder);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Metadata);
        });

        // Configure EmailTemplate entity
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TemplateName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TemplateType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
        });

        // Configure EmailHistory entity
        modelBuilder.Entity<EmailHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ToEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CcEmail).HasMaxLength(255);
            entity.Property(e => e.BccEmail).HasMaxLength(255);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
            entity.HasOne(e => e.GeneratedLetter).WithMany().HasForeignKey(e => e.GeneratedLetterId);
        });

        // Configure EmailAttachment entity
        modelBuilder.Entity<EmailAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileType).HasMaxLength(100);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.EmailHistory).WithMany(e => e.Attachments).HasForeignKey(e => e.EmailHistoryId);
        });

        // Configure DocumentInfo entity
        modelBuilder.Entity<DocumentInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.UploadedBy).IsRequired().HasMaxLength(100);
        });

        // Configure Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Data).HasMaxLength(4000);
            entity.Property(e => e.GroupName).HasMaxLength(100);
            entity.Property(e => e.RecipientId).HasMaxLength(100);
        });

        // Configure NotificationPreferences entity
        modelBuilder.Entity<NotificationPreferences>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EnabledTypes).HasMaxLength(500);
            entity.Property(e => e.DisabledTypes).HasMaxLength(500);
        });

        // Configure PasswordReset entity
        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UsedByIp).HasMaxLength(45);
        });



        // Configure DynamicTab entity
        modelBuilder.Entity<DynamicTab>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TabType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DataSource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DatabaseQuery).HasMaxLength(4000);
            entity.Property(e => e.ExcelMapping).HasMaxLength(4000);
            entity.Property(e => e.TemplateId).HasMaxLength(100);
            entity.Property(e => e.FieldMappings).HasMaxLength(4000);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.Permissions).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId);
        });

        // Configure DynamicTabField entity
        modelBuilder.Entity<DynamicTabField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.DynamicTabId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DataType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ValidationRules).HasMaxLength(1000);
            entity.Property(e => e.DefaultValue).HasMaxLength(500);
            entity.Property(e => e.ExcelColumnName).HasMaxLength(100);
            entity.Property(e => e.DatabaseColumnName).HasMaxLength(100);
            entity.HasOne(e => e.DynamicTab).WithMany(e => e.Fields).HasForeignKey(e => e.DynamicTabId);
        });

        // Configure DynamicTabData entity
        modelBuilder.Entity<DynamicTabData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.DynamicTabId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DataSource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            entity.Property(e => e.DataContent).HasMaxLength(4000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProcessedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.HasOne(e => e.DynamicTab).WithMany(e => e.DataRecords).HasForeignKey(e => e.DynamicTabId);
        });
    }
}
