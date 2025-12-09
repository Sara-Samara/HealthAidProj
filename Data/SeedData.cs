// Data/SeedData.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;

namespace HealthAidAPI.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger>();

            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if database already has data
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already has data. Skipping seeding.");
                return; // DB has been seeded
            }

            var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();

            // Seed initial admin user
            var adminUser = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@healthpal.ps",
                PasswordHash =  passwordHasher.HashPassword("Admin123!"),
                Phone = "+970599999999",
                Country = "Palestine",
                City = "Gaza",
                Street = "Al-Rasheed Street",
                Role = "Admin",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Seed sample doctor
            var doctorUser = new User
            {
                FirstName = "Ahmed",
                LastName = "Mohammed",
                Email = "ahmed.mohammed@healthpal.ps",
                PasswordHash = passwordHasher.HashPassword("Doctor123!"),
                Phone = "+970598888888",
                Country = "Palestine",
                City = "Ramallah",
                Street = "Al-Manara Square",
                Role = "Doctor",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(doctorUser);
            await context.SaveChangesAsync();

            var doctor = new Doctor
            {
                Specialization = "General Practice",
                YearsExperience = 8,
                Bio = "Experienced general practitioner with focus on family medicine and preventive care.",
                LicenseNumber = "PAL-MED-00123",
                AvailableHours = "Mon-Fri: 9:00 AM - 5:00 PM",
                UserId = doctorUser.Id
            };

            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();

            // Seed sample patient
            var patientUser = new User
            {
                FirstName = "Fatima",
                LastName = "Hassan",
                Email = "fatima.hassan@example.ps",
                PasswordHash = passwordHasher.HashPassword("Patient123!"),
                Phone = "+970597777777",
                Country = "Palestine",
                City = "Hebron",
                Street = "Bab Al-Zawiya",
                Role = "Patient",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(patientUser);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded successfully with initial data.");
        }
    }
}