using HealthAidAPI.Data;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Implementations;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Services.MedicineRequest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAutoMapper(typeof(MappingProfile));


builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INgoMissionService, NgoMissionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPublicAlertService, PublicAlertService>();
builder.Services.AddScoped<INgoService, NgoService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISponsorshipService, SponsorshipService>();
builder.Services.AddScoped<IMedicineRequestService, MedicineRequestService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

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
    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole("Doctor", "Admin"));
    options.AddPolicy("AllUsers", policy => policy.RequireRole("Admin", "Doctor", "Patient", "Donor"));
});


builder.Services.AddMemoryCache();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


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

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
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



app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthAid API v1.0");
    c.RoutePrefix = "swagger"; 
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    try
    {
        await db.Database.EnsureCreatedAsync();

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
        else
        {
            Console.WriteLine("ℹ️ Admin user already exists");
        }

        Console.WriteLine("✅ Database seeding completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ℹ️ Note: {ex.Message}");
      
    }
}


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

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        message = "HealthAid API is running!",
        documentation = "/swagger",
        endpoints = new
        {
            swagger = "/swagger",
            emergency = "/api/emergency",
            medical = "/api/medicalfacility",
            analytics = "/api/analytics/dashboard"
        }
    });
});

Console.WriteLine("HealthAid API is running!");
Console.WriteLine("Swagger Documentation: /swagger");
Console.WriteLine("HealthAid - Palestinian Healthcare Platform");

app.Run();

public partial class Program { }