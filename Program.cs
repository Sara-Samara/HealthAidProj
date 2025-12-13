using HealthAidAPI.Data;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services;
using HealthAidAPI.Services.Implementations;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Services.MedicineRequest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================================================
// 1. Configuration & Core Services
// ====================================================

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Settings Injection
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Infrastructure
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddHttpContextAccessor();

// CORS - محددة بشكل آمن
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:5000",
                "https://localhost:5000",
                "http://localhost:7000",
                "https://localhost:7000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ====================================================
// 2. Dependency Injection
// ====================================================

// Auth & User
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDonorService, DonorService>();
builder.Services.AddScoped<INgoService, NgoService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();


// Communication

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPublicAlertService, PublicAlertService>();

builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
// Medical
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IMedicalFacilityService, MedicalFacilityService>();
builder.Services.AddScoped<IHealthGuideService, HealthGuideService>();
builder.Services.AddScoped<IMentalSupportSessionService, MentalSupportSessionService>();
builder.Services.AddScoped<IMedicineRequestService, MedicineRequestService>();

// Emergency & Logistics
builder.Services.AddScoped<IEmergencyService, EmergencyService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<INgoMissionService, NgoMissionService>();

// Financial
builder.Services.AddScoped<ISponsorshipService, SponsorshipService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Utilities
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAiService, AiService>();

// Medical Services
builder.Services.AddScoped<IBloodBankService, BloodBankService>();
builder.Services.AddScoped<IPharmacyService, PharmacyService>();
builder.Services.AddScoped<IVitalsService, VitalsService>();

// ====================================================
// 3. Authentication & Authorization
// ====================================================

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
    options.AddPolicy("DonorOnly", policy => policy.RequireRole("Donor"));
    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole("Doctor", "Admin"));
    options.AddPolicy("AllUsers", policy => policy.RequireRole("Admin", "Doctor", "Patient", "Donor"));
});

// ====================================================
// 4. Swagger Configuration
// ====================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HealthAid API - Palestinian Healthcare Platform",
        Version = "v1.0",
        Description = "Comprehensive Digital Healthcare Platform for Palestinians",
        Contact = new OpenApiContact
        {
            Name = "HealthAid Team",
            Email = "support@healthaid.ps"
        }
    });

    // Accept-Language header
    c.OperationFilter<AcceptLanguageHeaderFilter>();

    c.CustomSchemaIds(type => type.ToString());

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token (format: Bearer {token})"
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ====================================================
// 5. Middleware Pipeline
// ====================================================

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthAid API v1.0");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
});

// Localization
var supportedCultures = new[] { "en-US", "ar-PS" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// 🚨 الترتيب الصحيح للميدلوير 🚨
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");      // CORS أولاً
app.UseAuthentication();      // ثم التوثيق
app.UseAuthorization();       // ثم الصلاحيات

// ====================================================
// 6. Database Seeding
// ====================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        await db.Database.EnsureCreatedAsync();

        // إنشاء Admin إذا لم يكن موجوداً
        if (!db.Users.Any(u => u.Email == "admin@healthaid.ps"))
        {
            var adminUser = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@healthaid.ps",
                PasswordHash = passwordHasher.HashPassword("Admin123!"),
                Phone = await GenerateUniquePhoneNumberAsync(db),
                Country = "Palestine",
                City = "Gaza",
                Street = "Al-Rasheed Street",
                Role = "Admin",
                Status = "Active",
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(adminUser);
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Admin user created successfully!");
        }

        // Seed البيانات
        await SeedData.SeedAsync(db, passwordHasher);
        Console.WriteLine("✅ Database fully seeded with demo data!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ℹ️ Seeding Error: {ex.Message}");
    }
}

// ====================================================
// 7. Endpoints
// ====================================================

app.MapControllers();

// SignalR Hub
app.MapHub<HealthAidAPI.Hubs.HealthAidHub>("/hubs/healthaid");

// Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "HealthAid API",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow
}));

// Database Test Endpoint
app.MapGet("/api/test/db", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var usersCount = await db.Users.CountAsync();

        return Results.Ok(new
        {
            Database = canConnect ? "Connected" : "Not Connected",
            UsersCount = usersCount,
            Timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});

// Root Redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

Console.WriteLine("========================================");
Console.WriteLine("🚀 HealthAid API is running!");
Console.WriteLine("📚 Swagger: https://localhost:7001/swagger");
Console.WriteLine("🏥 Health Check: /health");
Console.WriteLine("========================================");

app.Run();

// Helper Function
async Task<string> GenerateUniquePhoneNumberAsync(ApplicationDbContext dbContext)
{
    var random = new Random();
    string phoneNumber;
    int attempts = 0;

    do
    {
        phoneNumber = $"+97059{random.Next(1000000, 9999999)}";
        attempts++;

        var exists = await dbContext.Users.AnyAsync(u => u.Phone == phoneNumber);
        if (!exists) break;

        if (attempts > 10)
        {
            phoneNumber = $"+97059{DateTime.Now.Ticks % 10000000}";
            break;
        }
    } while (true);

    return phoneNumber;
}

public partial class Program { }