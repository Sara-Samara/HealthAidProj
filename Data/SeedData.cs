using HealthAidAPI.Models;
using HealthAidAPI.Models.Emergency;
using HealthAidAPI.Models.Extras;
using HealthAidAPI.Models.Location;
using HealthAidAPI.Models.MedicalFacilities;
using HealthAidAPI.Models.Recommendations;
using HealthAidAPI.Services.Interfaces;

namespace HealthAidAPI.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            // 1. Check if seeded
            if (context.Users.Any(u => u.Role == "Doctor")) return;

            // كلمات مرور موحدة لتسهيل الاختبار
            var adminPass = passwordHasher.HashPassword("Admin@1234");
            var docPass = passwordHasher.HashPassword("Doctor@1234");
            var patPass = passwordHasher.HashPassword("Patient@1234");
            var donorPass = passwordHasher.HashPassword("Donor@1234");

            // ==========================================
            // 2. Users & Profiles
            // ==========================================
            var users = new List<User>
            {
                // --- Doctors ---
                // د. سمير (قلب - غزة)
                new User { FirstName = "Samir", LastName = "Al-Khatib", Email = "dr.samir@healthaid.ps", PasswordHash = docPass, Phone = "0599100100", Country = "Palestine", City = "Gaza", Street = "Omar Al-Mukhtar", Role = "Doctor", Status = "Active", EmailVerified = true },
                // د. منى (أطفال - رفح)
                new User { FirstName = "Mona", LastName = "Masri", Email = "dr.mona@healthaid.ps", PasswordHash = docPass, Phone = "0599100200", Country = "Palestine", City = "Rafah", Street = "Al-Najjar", Role = "Doctor", Status = "Active", EmailVerified = true },
                // د. يوسف (نفسي - نابلس)
                new User { FirstName = "Yousef", LastName = "Jabari", Email = "dr.yousef@healthaid.ps", PasswordHash = docPass, Phone = "0599100300", Country = "Palestine", City = "Nablus", Street = "Rafidia", Role = "Doctor", Status = "Active", EmailVerified = true },

                // --- Patients ---
                // أحمد (حالة طارئة - غزة)
                new User { FirstName = "Ahmed", LastName = "Odeh", Email = "ahmed.odeh@gmail.com", PasswordHash = patPass, Phone = "0599500500", Country = "Palestine", City = "Gaza", Street = "Shujaiya", Role = "Patient", Status = "Active", EmailVerified = true },
                // ليلى (مرض مزمن - خانيونس)
                new User { FirstName = "Laila", LastName = "Kareem", Email = "laila.k@gmail.com", PasswordHash = patPass, Phone = "0599500600", Country = "Palestine", City = "Khan Yunis", Street = "Al-Amal", Role = "Patient", Status = "Active", EmailVerified = true },
                // خالد (يحتاج كفالة - مخيم جنين)
                new User { FirstName = "Khaled", LastName = "Zaid", Email = "khaled.z@gmail.com", PasswordHash = patPass, Phone = "0599500700", Country = "Palestine", City = "Jenin", Street = "Camp Area", Role = "Patient", Status = "Active", EmailVerified = true },

                // --- Donors ---
                new User { FirstName = "Global", LastName = "Relief", Email = "support@globalrelief.org", PasswordHash = donorPass, Phone = "0012345678", Country = "USA", City = "New York", Street = "UN Plaza", Role = "Donor", Status = "Active", EmailVerified = true },
                new User { FirstName = "Karim", LastName = "Abbas", Email = "karim.abbas@hotmail.com", PasswordHash = donorPass, Phone = "0599800800", Country = "Palestine", City = "Ramallah", Street = "Masyoun", Role = "Donor", Status = "Active", EmailVerified = true },

                // --- NGO Admin ---
                new User { FirstName = "Red", LastName = "Crescent", Email = "admin@prcs.ps", PasswordHash = adminPass, Phone = "101", Country = "Palestine", City = "Gaza", Street = "Tal Al-Hawa", Role = "Admin", Status = "Active", EmailVerified = true }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // --- Link Profiles ---

            // Doctors
            context.Doctors.AddRange(
                new Doctor { UserId = users.First(u => u.Email == "dr.samir@healthaid.ps").Id, Specialization = "Cardiology", YearsExperience = 20, LicenseNumber = "PS-MED-1122", Bio = "Consultant Cardiologist at Al-Shifa Hospital.", AvailableHours = "Sun-Thu: 8AM-2PM" },
                new Doctor { UserId = users.First(u => u.Email == "dr.mona@healthaid.ps").Id, Specialization = "Pediatrics", YearsExperience = 12, LicenseNumber = "PS-MED-3344", Bio = "Specialist in child nutrition and vaccination.", AvailableHours = "Sat-Thu: 10AM-4PM" },
                new Doctor { UserId = users.First(u => u.Email == "dr.yousef@healthaid.ps").Id, Specialization = "Psychiatry", YearsExperience = 15, LicenseNumber = "PS-MED-5566", Bio = "Trauma support and mental health counseling.", AvailableHours = "Online Only: 4PM-9PM" }
            );

            // Patients
            context.Patients.AddRange(
                new Patient { UserId = users.First(u => u.Email == "ahmed.odeh@gmail.com").Id, PatientName = "Ahmed Odeh", BloodType = "O+", DateOfBirth = new DateTime(1980, 5, 10), Gender = "Male", MedicalHistory = "Hypertension, Previous Heart Attack" },
                new Patient { UserId = users.First(u => u.Email == "laila.k@gmail.com").Id, PatientName = "Laila Kareem", BloodType = "A-", DateOfBirth = new DateTime(2015, 8, 20), Gender = "Female", MedicalHistory = "Severe Asthma, Insulin Dependent Diabetes" },
                new Patient { UserId = users.First(u => u.Email == "khaled.z@gmail.com").Id, PatientName = "Khaled Zaid", BloodType = "B+", DateOfBirth = new DateTime(1995, 12, 1), Gender = "Male", MedicalHistory = "War Injury - Leg Amputation" }
            );

            // Donors
            context.Donors.AddRange(
                new Donor { UserId = users.First(u => u.Email == "support@globalrelief.org").Id, Organization = "Global Relief Foundation", TotalDonated = 50000 },
                new Donor { UserId = users.First(u => u.Email == "karim.abbas@hotmail.com").Id, Organization = "Individual", TotalDonated = 200 }
            );

            // NGOs
            context.NGOs.Add(new NGO { OrganizationName = "Palestine Red Crescent", AreaOfWork = "Emergency Response", VerifiedStatus = "Verified", ContactedPerson = "Dr. Sameer", Email = "info@prcs.ps", Phone = "101", Address = "Gaza HQ" });

            await context.SaveChangesAsync();

            // ==========================================
            // 4. Facilities & Logistics
            // ==========================================
            var facilities = new List<MedicalFacility>
            {
                new MedicalFacility { Name = "Al-Shifa Medical Complex", Type = "Hospital", Address = "Gaza City", Latitude = 31.5222m, Longitude = 34.4480m, IsActive = true, Verified = true, OperatingHours = "24/7", ContactNumber = "0599000001", Services = "Emergency, Surgery, ICU" },
                new MedicalFacility { Name = "Nasser Hospital", Type = "Hospital", Address = "Khan Yunis", Latitude = 31.3456m, Longitude = 34.2955m, IsActive = true, Verified = true, OperatingHours = "24/7", ContactNumber = "0599000002", Services = "General, Maternity" },
                new MedicalFacility { Name = "Central Care Pharmacy", Type = "Pharmacy", Address = "Gaza - Remal", Latitude = 31.5150m, Longitude = 34.4550m, IsActive = true, Verified = true, OperatingHours = "8AM - 11PM", ContactNumber = "0599111222", Services = "Medicines, Supplies" }
            };
            context.MedicalFacilities.AddRange(facilities);
            await context.SaveChangesAsync();

            // Inventory
            var pharmacy = context.MedicalFacilities.First(f => f.Type == "Pharmacy");
            context.PharmacyInventories.AddRange(
                new PharmacyInventory { MedicalFacilityId = pharmacy.Id, MedicineName = "Insulin Lantus", StockQuantity = 5, Price = 30.00m },
                new PharmacyInventory { MedicalFacilityId = pharmacy.Id, MedicineName = "Panadol Extra", StockQuantity = 200, Price = 5.00m },
                new PharmacyInventory { MedicalFacilityId = pharmacy.Id, MedicineName = "Amoxicillin 500mg", StockQuantity = 0, Price = 15.00m } // Out of Stock
            );

            // ==========================================
            // 5. Operational Data
            // ==========================================

            var drSamir = context.Doctors.First(d => d.User.Email == "dr.samir@healthaid.ps");
            var drMona = context.Doctors.First(d => d.User.Email == "dr.mona@healthaid.ps");
            var patAhmed = context.Patients.First(p => p.User.Email == "ahmed.odeh@gmail.com");
            var patLaila = context.Patients.First(p => p.User.Email == "laila.k@gmail.com");

            // Appointments
            context.Appointments.AddRange(
                new Appointment { DoctorId = drSamir.DoctorId, PatientId = patAhmed.PatientId, AppointmentDate = DateTime.UtcNow.AddDays(1), Status = "Confirmed", Note = "Chest pain follow-up" },
                new Appointment { DoctorId = drMona.DoctorId, PatientId = patLaila.PatientId, AppointmentDate = DateTime.UtcNow.AddDays(2), Status = "Pending", Note = "Asthma checkup" },
                new Appointment { DoctorId = drSamir.DoctorId, PatientId = patAhmed.PatientId, AppointmentDate = DateTime.UtcNow.AddDays(-5), Status = "Completed", Note = "Initial Consultation" }
            );

            // Sponsorships (Fundraising)
            context.Sponsorships.AddRange(
                new Sponsorship { PatientId = patLaila.PatientId, Category = "Medical", GoalAmount = 1000, AmountRaised = 250, GoalDescription = "Urgent Insulin Supply for Child", Status = "Active", Deadline = DateTime.UtcNow.AddMonths(1) },
                new Sponsorship { PatientId = users.First(u => u.Email == "khaled.z@gmail.com").Patient!.PatientId, Category = "Surgery", GoalAmount = 5000, AmountRaised = 5000, GoalDescription = "Prosthetic Leg Surgery", Status = "Completed", Deadline = DateTime.UtcNow.AddDays(10) }
            );

            // Emergency
            context.EmergencyCases.Add(new EmergencyCase
            {
                PatientId = patAhmed.PatientId,
                EmergencyType = "Heart Attack",
                Priority = "Critical",
                Location = "Gaza - Shujaiya, Near Market",
                Latitude = 31.5100m,
                Longitude = 34.4600m,
                Status = "Active",
                Description = "Patient collapsed, severe chest pain."
            });

            // Alerts
            context.PublicAlerts.Add(new PublicAlert
            {
                Title = "Weather Warning",
                Description = "Heavy rainfall expected in Gaza Strip. Please secure homes.",
                Region = "Gaza Strip",
                AlertType = "Safety",
                Severity = "High",
                IsActive = true,
                UserId = users.First(u => u.Email == "admin@prcs.ps").Id
            });

            await context.SaveChangesAsync();
        }
    }
}