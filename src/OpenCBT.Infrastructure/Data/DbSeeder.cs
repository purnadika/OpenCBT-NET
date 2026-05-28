using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Enums;
using System.Security.Claims;

namespace OpenCBT.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<ApplicationRole> roleManager)
    {
        // 1. Seed Roles
        var roles = new[] { "Admin", "Teacher", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var newRole = new ApplicationRole(role);
                await roleManager.CreateAsync(newRole);

                // Add permissions (Claims) to Admin role for future dynamic checking
                if (role == "Admin")
                {
                    await roleManager.AddClaimAsync(newRole, new Claim("Permission", "ManageExams"));
                    await roleManager.AddClaimAsync(newRole, new Claim("Permission", "ManageUsers"));
                }
                else if (role == "Teacher")
                {
                    await roleManager.AddClaimAsync(newRole, new Claim("Permission", "ManageExams"));
                }
            }
        }

        // 2. Seed Admin User
        var adminEmail = "admin@opencbt.local";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                IdentifierNumber = "ADMIN-001",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                if (context.Database.IsRelational())
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    var adminRole = await roleManager.FindByNameAsync("Admin");
                    context.UserRoles.Add(new IdentityUserRole<Guid> { UserId = admin.Id, RoleId = adminRole.Id });
                    await context.SaveChangesAsync();
                }
            }
        }
        
        // 3. Seed Student User
        var studentEmail = "student@opencbt.local";
        var student = await userManager.FindByEmailAsync(studentEmail);
        if (student == null)
        {
            student = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FullName = "Test Student",
                IdentifierNumber = "NISN-12345",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(student, "Student123!");
            if (result.Succeeded)
            {
                if (context.Database.IsRelational())
                {
                    await userManager.AddToRoleAsync(student, "Student");
                }
                else
                {
                    var studentRole = await roleManager.FindByNameAsync("Student");
                    context.UserRoles.Add(new IdentityUserRole<Guid> { UserId = student.Id, RoleId = studentRole.Id });
                    await context.SaveChangesAsync();
                }
            }
        }

        // 4. Seed Demo Exam
        if (!context.Exams.Any())
        {
            var exam = new Exam
            {
                Title = "Demo CBT Exam",
                Description = "A sample exam to test the wizard UI and scoring system.",
                StartTime = DateTime.UtcNow.AddMinutes(-10),
                EndTime = DateTime.UtcNow.AddYears(1),
                DurationMinutes = 60,
                IsActive = true,
                DisplayMode = DisplayMode.Wizard
            };

            var q1 = new Question
            {
                Text = "What is the primary benefit of using ASP.NET Core?",
                OrderIndex = 1,
                Points = 10,
                Options = new List<AnswerOption>
                {
                    new AnswerOption { Text = "Cross-platform and high performance", IsCorrect = true, OrderIndex = 1 },
                    new AnswerOption { Text = "It only runs on Windows", IsCorrect = false, OrderIndex = 2 },
                    new AnswerOption { Text = "It requires PHP", IsCorrect = false, OrderIndex = 3 }
                }
            };

            var q2 = new Question
            {
                Text = "Which library is used for building UI components in our architecture?",
                OrderIndex = 2,
                Points = 10,
                Options = new List<AnswerOption>
                {
                    new AnswerOption { Text = "React", IsCorrect = false, OrderIndex = 1 },
                    new AnswerOption { Text = "Alpine.js and Tailwind CSS", IsCorrect = true, OrderIndex = 2 },
                    new AnswerOption { Text = "Angular", IsCorrect = false, OrderIndex = 3 }
                }
            };

            exam.Questions.Add(q1);
            exam.Questions.Add(q2);

            await context.Exams.AddAsync(exam);
            await context.SaveChangesAsync();
        }
    }
}
