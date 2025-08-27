using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DocHub.Infrastructure.Data;
using DocHub.Application.Interfaces;
using DocHub.Infrastructure.Services;
using DocHub.Infrastructure.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DocHub.API.Middleware;
using DocHub.API.Hubs;
using DocHub.Infrastructure.Services.Document;
using DocHub.Infrastructure.Services.PROXKey;
using DocHub.Infrastructure.Services.Signature;
using DocHub.Application.MappingProfiles;
using FluentValidation.AspNetCore;
using DocHub.Application.Validation;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DocHub API", Version = "1.0.0" });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"))
            )
        };
    });

// Add DbContext with SQLite
builder.Services.AddDbContext<DocHubDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    options.UseSqlite(connectionString, b => b.MigrationsAssembly("DocHub.API"));
    options.EnableSensitiveDataLogging();  // Enable detailed logging for development
});

// Register repositories
builder.Services.AddScoped<IDynamicTabRepository, DynamicTabRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(DynamicTabProfile).Assembly);

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDynamicTabDtoValidator>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocHub.Infrastructure.Services.Document.SyncfusionDocumentService>();
builder.Services.AddScoped<IDigitalSignatureService, DigitalSignatureService>();
builder.Services.AddScoped<IDynamicTabService, DynamicTabService>();
builder.Services.AddScoped<IEmailService, DocHub.Infrastructure.Services.Email.EnhancedEmailService>();
builder.Services.AddScoped<IExcelService, DocHub.Infrastructure.Services.Excel.EPPlusExcelService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILetterTemplateService, LetterTemplateService>();
builder.Services.AddScoped<IGeneratedLetterService, GeneratedLetterService>();
builder.Services.AddScoped<ILetterPreviewService, LetterPreviewService>();
builder.Services.AddScoped<IEmailHistoryService, EmailHistoryService>();
builder.Services.AddScoped<IPROXKeyService, DocHub.Infrastructure.Services.PROXKey.PROXKeyService>();
builder.Services.AddScoped<ISignatureService, DocHub.Infrastructure.Services.Signature.SignatureService>();
builder.Services.AddScoped<IExcelDataProcessingService, DocHub.Infrastructure.Services.ExcelDataProcessing.ExcelDataProcessingService>();

// Register new services for backend completion
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IFileCompressionService, FileCompressionService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();

// Register Memory Cache for rate limiting
builder.Services.AddMemoryCache();

// Add configuration
builder.Services.AddSingleton<DocHub.Application.Configuration.AppConfiguration>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply database initialization and migrations
if (builder.Configuration.GetValue<bool>("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DocHubDbContext>();

    try
    {
        // Apply any pending migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            app.Logger.LogInformation("Database migrations applied successfully");
        }

        // Seed initial data if enabled
        if (builder.Configuration.GetValue<bool>("Database:AutoSeed", true))
        {
            await SeedDataAsync(context);
            app.Logger.LogInformation("Database seeded successfully");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error during database initialization");
        throw; // Fail fast if database setup fails
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add custom middleware
app.UseRequestValidation();
app.UseRateLimiting(new RateLimitOptions { MaxRequestsPerWindow = 100, WindowMinutes = 1 });
app.UseGlobalExceptionHandler();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<EmailStatusHub>("/emailStatusHub");

app.Run();

// Seed data method
static async Task SeedDataAsync(DocHubDbContext context)
{
    if (!context.DynamicTabs.Any())
    {
        var defaultTabs = new[]
        {
            new DocHub.Core.Entities.DynamicTab
            {
                Name = "transfer-letter",
                DisplayName = "Transfer Letter",
                Description = "Generate transfer letters for employees",
                DataSource = "Upload",
                Icon = "swap_horiz",
                Color = "#2196F3",
                SortOrder = 1,
                IsActive = true
            },
            new DocHub.Core.Entities.DynamicTab
            {
                Name = "experience-letter",
                DisplayName = "Experience Letter",
                Description = "Generate experience letters for employees",
                DataSource = "Upload",
                Icon = "work",
                Color = "#4CAF50",
                SortOrder = 2,
                IsActive = true
            },
            new DocHub.Core.Entities.DynamicTab
            {
                Name = "confirmation-letter",
                DisplayName = "Confirmation Letter",
                Description = "Generate confirmation letters for employees",
                DataSource = "Upload",
                Icon = "verified",
                Color = "#FF9800",
                SortOrder = 3,
                IsActive = true
            },
            new DocHub.Core.Entities.DynamicTab
            {
                Name = "mutual-cessation",
                DisplayName = "Mutual Cessation",
                Description = "Generate mutual cessation letters",
                DataSource = "Upload",
                Icon = "handshake",
                Color = "#9C27B0",
                SortOrder = 4,
                IsActive = true
            }
        };

        context.DynamicTabs.AddRange(defaultTabs);
        await context.SaveChangesAsync();
    }

    if (!context.Admins.Any())
    {
        var admin = new DocHub.Core.Entities.Admin
        {
            Username = "admin",
            Email = "admin@dochub.com",
            FullName = "System Administrator",
            PasswordHash = "hashed_password_here", // This should be properly hashed in production
            Role = "SuperAdmin",
            IsSuperAdmin = true,
            IsActive = true
        };

        context.Admins.Add(admin);
        await context.SaveChangesAsync();
    }
}
