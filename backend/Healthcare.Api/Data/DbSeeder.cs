using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        // Demo-friendly: auto-create schema. For serious production use, replace with EF migrations.
        await db.Database.EnsureCreatedAsync();

        var email = config["Seed:SuperAdminEmail"]?.Trim().ToLowerInvariant();
        var password = config["Seed:SuperAdminPassword"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password) &&
            !await db.Users.AnyAsync(x => x.Email == email))
        {
            db.Users.Add(new User
            {
                Name = config["Seed:SuperAdminName"] ?? "System Super Admin",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                EmailVerified = true,
                Role = UserRole.SuperAdmin,
                Status = UserStatus.Active
            });
            await db.SaveChangesAsync();
        }

        if (!bool.TryParse(config["Seed:DemoData"], out var demo) || !demo) return;

        var demoDoctors = new[]
 {
            new
            {
                Name = "Dr. Afsana Rahman",
                Email = "afsana.demo@healthflow.local",
                License = "DEMO-1001",
                Specialty = "Cardiology",
                Qualifications = "MBBS, MD (Cardiology)",
                Experience = 12,
                Fee = 1200m,
                Bio = "Cardiology consultant focused on hypertension, preventive heart care and long-term cardiovascular risk management.",
                Rating = 4.9
            },
            new
            {
                Name = "Dr. Mahin Chowdhury",
                Email = "mahin.demo@healthflow.local",
                License = "DEMO-1002",
                Specialty = "Dermatology",
                Qualifications = "MBBS, DDV",
                Experience = 9,
                Fee = 1000m,
                Bio = "Dermatology consultant experienced in acne, eczema, allergies, hair loss and common skin conditions.",
                Rating = 4.8
            },
            new
            {
                Name = "Dr. Nusrat Karim",
                Email = "nusrat.demo@healthflow.local",
                License = "DEMO-1003",
                Specialty = "Medicine",
                Qualifications = "MBBS, FCPS (Medicine)",
                Experience = 15,
                Fee = 900m,
                Bio = "Internal medicine specialist focused on adult illnesses, diabetes, hypertension and long-term primary care.",
                Rating = 4.9
            },
            new
            {
                Name = "Dr. Farzana Islam",
                Email = "farzana.demo@healthflow.local",
                License = "DEMO-1004",
                Specialty = "Gynecology",
                Qualifications = "MBBS, FCPS (Gynecology & Obstetrics)",
                Experience = 11,
                Fee = 1300m,
                Bio = "Gynecology specialist providing consultation for women's health, pregnancy care and reproductive health.",
                Rating = 4.8
            },
            new
            {
                Name = "Dr. Tanvir Hasan",
                Email = "tanvir.demo@healthflow.local",
                License = "DEMO-1005",
                Specialty = "Neurology",
                Qualifications = "MBBS, MD (Neurology)",
                Experience = 10,
                Fee = 1500m,
                Bio = "Neurology consultant treating headaches, migraine, epilepsy, stroke and other neurological conditions.",
                Rating = 4.7
            },
            new
            {
                Name = "Dr. Sadia Noor",
                Email = "sadia.demo@healthflow.local",
                License = "DEMO-1006",
                Specialty = "Pediatrics",
                Qualifications = "MBBS, FCPS (Pediatrics)",
                Experience = 8,
                Fee = 1000m,
                Bio = "Pediatric specialist providing child health consultation, vaccination guidance and growth monitoring.",
                Rating = 4.9
            },
            new
            {
                Name = "Dr. Rafiq Hossain",
                Email = "rafiq.demo@healthflow.local",
                License = "DEMO-1007",
                Specialty = "Orthopedics",
                Qualifications = "MBBS, MS (Orthopedics)",
                Experience = 14,
                Fee = 1400m,
                Bio = "Orthopedic surgeon experienced in bone, joint, spine, sports injuries and musculoskeletal conditions.",
                Rating = 4.8
            },
            new
            {
                Name = "Dr. Ishrat Jahan",
                Email = "ishrat.demo@healthflow.local",
                License = "DEMO-1008",
                Specialty = "ENT",
                Qualifications = "MBBS, FCPS (ENT)",
                Experience = 9,
                Fee = 1100m,
                Bio = "ENT specialist treating ear, nose and throat conditions including sinus, tonsil and hearing problems.",
                Rating = 4.7
            },
            new
            {
                Name = "Dr. Nabil Ahmed",
                Email = "nabil.demo@healthflow.local",
                License = "DEMO-1009",
                Specialty = "Gastroenterology",
                Qualifications = "MBBS, MD (Gastroenterology)",
                Experience = 13,
                Fee = 1500m,
                Bio = "Gastroenterology specialist treating digestive disorders, liver problems, gastritis and gastrointestinal conditions.",
                Rating = 4.9
            },
            new
            {
                Name = "Dr. Tanjina Sultana",
                Email = "tanjina.demo@healthflow.local",
                License = "DEMO-1010",
                Specialty = "Endocrinology",
                Qualifications = "MBBS, MD (Endocrinology)",
                Experience = 10,
                Fee = 1300m,
                Bio = "Endocrinology specialist focused on diabetes, thyroid disorders, hormonal problems and metabolic diseases.",
                Rating = 4.8
            },
            new
            {
                Name = "Dr. Mehedi Kabir",
                Email = "mehedi.demo@healthflow.local",
                License = "DEMO-1011",
                Specialty = "Psychiatry",
                Qualifications = "MBBS, MD (Psychiatry)",
                Experience = 9,
                Fee = 1200m,
                Bio = "Psychiatry consultant providing assessment and treatment for anxiety, depression, stress and sleep disorders.",
                Rating = 4.8
            },
              new
            {
                Name = "Dr. Riashat Azim",
                Email = "riashat.demo@healthflow.local",
                License = "DEMO-1012",
                Specialty = "Medical",
                Qualifications = "MBBS",
                Experience = 3,
                Fee = 1200m,
                Bio = "Psychiatry consultant providing assessment and treatment for anxiety, depression, stress and sleep disorders.",
                Rating = 5.0
            },
                new
            {
                Name = "Dr. Saheda Sultana Setu",
                Email = "setu.demo@healthflow.local",
                License = "DEMO-1013",
                Specialty = "Medical",
                Qualifications = "MBBS",
                Experience = 9,
                Fee = 1200m,
                Bio = "Psychiatry consultant providing assessment and treatment for anxiety, depression, stress and sleep disorders.",
                Rating = 4.9
            },
            new
            {
                Name = "Dr. Sharmeen Akter",
                Email = "sharmeen.demo@healthflow.local",
                License = "DEMO-1014",
                Specialty = "Ophthalmology",
                Qualifications = "MBBS, FCPS (Ophthalmology)",
                Experience = 12,
                Fee = 1200m,
                Bio = "Eye specialist providing consultation for vision problems, eye infections, cataracts and other ophthalmic conditions.",
                Rating = 4.9
            }
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Doctor123!");

        // Database-এ আগে থেকেই থাকা email + license load করবে
        var existingEmails = (await db.Users
                .Select(x => x.Email)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingLicenses = (await db.DoctorProfiles
                .Select(x => x.LicenseNumber)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var d in demoDoctors)
        {
            var doctorEmail = d.Email.Trim().ToLowerInvariant();
            var license = d.License.Trim();

            // Email অথবা License — যেকোনো একটা আগে থাকলে skip
            if (existingEmails.Contains(doctorEmail) ||
                existingLicenses.Contains(license))
            {
                continue;
            }

            var user = new User
            {
                Name = d.Name,
                Email = doctorEmail,
                PasswordHash = passwordHash,
                EmailVerified = true,
                Role = UserRole.Doctor,
                Status = UserStatus.Active,

                DoctorProfile = new DoctorProfile
                {
                    LicenseNumber = license,
                    Specialization = d.Specialty,
                    Qualifications = d.Qualifications,
                    ExperienceYears = d.Experience,
                    ConsultationFee = d.Fee,
                    Bio = d.Bio,
                    ApprovalStatus = DoctorApprovalStatus.Approved,
                    AverageRating = d.Rating,
                    ReviewCount = 24
                }
            };

            db.Users.Add(user);

            // একই seed run-এর মধ্যে duplicate ঢুকতেও দেবে না
            existingEmails.Add(doctorEmail);
            existingLicenses.Add(license);
        }

        await db.SaveChangesAsync();



        var doctors = await db.DoctorProfiles
            .Include(x => x.User)
            .Where(x => x.ApprovalStatus == DoctorApprovalStatus.Approved && x.User.Status == UserStatus.Active)
            .OrderBy(x => x.User.Name).ToListAsync();
        for (var day = 0; day < 10; day++)
        {
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(day));
            for (var i = 0; i < doctors.Count; i++)
            {
                var start = i switch { 0 => new TimeOnly(17, 0), 1 => new TimeOnly(15, 0), _ => new TimeOnly(18, 0) };
                if (day == 0 && start <= TimeOnly.FromDateTime(DateTime.Now.AddMinutes(30)))
                    start = TimeOnly.FromDateTime(DateTime.Now.AddHours(1));

                var end = start.AddHours(3);
                if (end <= start) continue;
                if (await db.DoctorSchedules.AnyAsync(x => x.DoctorProfileId == doctors[i].Id && x.Date == date)) continue;
                db.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorProfileId = doctors[i].Id,
                    Date = date,
                    StartTime = start,
                    EndTime = end,
                    MeetLink = $"https://meet.google.com/healthflow-demo-{i + 1}",
                    Status = ScheduleStatus.Published
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
