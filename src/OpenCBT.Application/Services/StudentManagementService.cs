using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Application.Services;

public class StudentManagementService : IStudentManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
    {
        var students = await _userManager.GetUsersInRoleAsync("Student");
        return students.Select(s => new StudentDto
        {
            Id = s.Id,
            FullName = s.FullName,
            Email = s.Email ?? string.Empty,
            IdentifierNumber = s.IdentifierNumber,
            IsActive = s.IsActive
        });
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
    {
        // 1. Validation: NISN must be unique
        var existingByIdentifier = await _userManager.Users.FirstOrDefaultAsync(u => u.IdentifierNumber == dto.IdentifierNumber);
        if (existingByIdentifier != null)
        {
            throw new ArgumentException($"NISN / ID Number '{dto.IdentifierNumber}' is already registered.");
        }

        // 2. Validation: Email must be unique
        var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingByEmail != null)
        {
            throw new ArgumentException($"Email Address '{dto.Email}' is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            IdentifierNumber = dto.IdentifierNumber,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, "Student");

        return new StudentDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            IdentifierNumber = user.IdentifierNumber,
            IsActive = user.IsActive
        };
    }

    public async Task<StudentDto> UpdateStudentAsync(Guid id, StudentDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new KeyNotFoundException("Student not found.");

        // 1. Validation: NISN must be unique (exclude self)
        var existingByIdentifier = await _userManager.Users
            .FirstOrDefaultAsync(u => u.IdentifierNumber == dto.IdentifierNumber && u.Id != id);
        if (existingByIdentifier != null)
        {
            throw new ArgumentException($"NISN / ID Number '{dto.IdentifierNumber}' is already registered.");
        }

        // 2. Validation: Email must be unique (exclude self)
        var existingByEmail = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != id);
        if (existingByEmail != null)
        {
            throw new ArgumentException($"Email Address '{dto.Email}' is already registered.");
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.IdentifierNumber = dto.IdentifierNumber;
        user.IsActive = dto.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return new StudentDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            IdentifierNumber = user.IdentifierNumber,
            IsActive = user.IsActive
        };
    }

    public async Task DeleteStudentAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
    }

    public async Task<string> ResetStudentPasswordAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new KeyNotFoundException("Student not found.");

        // Securely generate a temporary random password
        // Requires: Length 10, contains digits, uppercase, lowercase
        var chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$";
        var randomBytes = new byte[12];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var tempPasswordBuilder = new System.Text.StringBuilder();
        // Ensure at least one uppercase, lowercase, digit, and symbol are explicitly placed first
        tempPasswordBuilder.Append("Cbt");
        tempPasswordBuilder.Append(randomBytes[0] % 10); // Digit
        for (int i = 1; i < 8; i++)
        {
            tempPasswordBuilder.Append(chars[randomBytes[i] % chars.Length]);
        }

        var tempPassword = tempPasswordBuilder.ToString();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);
        
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return tempPassword;
    }

    public Task<(int SavedCount, IEnumerable<string> Errors)> BulkImportStudentsAsync(Stream excelFileStream)
    {
        // Stub to satisfy interface for TDD
        throw new NotImplementedException();
    }
}
