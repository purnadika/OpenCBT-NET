using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;
using System.Security.Cryptography;

namespace OpenCBT.Application.Services;

public class StaffManagementService : IStaffManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<StaffDto>> GetAllStaffAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var teachers = await _userManager.GetUsersInRoleAsync("Teacher");

        var allStaff = admins.Select(a => new { User = a, Role = "Admin" })
            .Concat(teachers.Select(t => new { User = t, Role = "Teacher" }))
            .ToList();

        return allStaff.Select(s => new StaffDto
        {
            Id = s.User.Id,
            FullName = s.User.FullName,
            Email = s.User.Email ?? string.Empty,
            IdentifierNumber = s.User.IdentifierNumber,
            Role = s.Role,
            IsActive = s.User.IsActive,
            MustChangePassword = s.User.MustChangePassword
        }).OrderBy(s => s.FullName);
    }

    public async Task<StaffDto> CreateStaffAsync(CreateStaffDto dto)
    {
        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail != null)
        {
            throw new OpenCBT.Application.Exceptions.ValidationException("Error_EmailRegistered", dto.Email);
        }

        var password = string.IsNullOrEmpty(dto.Password) ? GenerateSecurePassword() : dto.Password;

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            IdentifierNumber = dto.IdentifierNumber,
            MustChangePassword = dto.MustChangePassword,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create staff: {errors}");
        }

        await _userManager.AddToRoleAsync(user, dto.Role);

        // We return the raw password in the DTO only upon creation so the UI can display it
        return new StaffDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IdentifierNumber = user.IdentifierNumber,
            Role = dto.Role,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword,
            RawPassword = password
        };
    }

    public async Task UpdateStaffAsync(Guid id, UpdateStaffDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new Exception("User not found");

        if (user.Email != dto.Email)
        {
            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail != null && existingEmail.Id != id)
            {
                throw new OpenCBT.Application.Exceptions.ValidationException("Error_EmailRegistered", dto.Email);
            }
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        user.FullName = dto.FullName;
        user.IdentifierNumber = dto.IdentifierNumber;
        user.IsActive = dto.IsActive;

        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(dto.Role))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);
        }
    }

    public async Task DeleteStaffAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new Exception("User not found");

        await _userManager.DeleteAsync(user);
    }

    public async Task<string> ResetStaffPasswordAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new Exception("User not found");

        var newPassword = GenerateSecurePassword();
        
        // Remove password and add new one
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        
        if (!result.Succeeded)
        {
            throw new Exception("Failed to reset password");
        }

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);

        return newPassword;
    }

    private string GenerateSecurePassword()
    {
        // Generate an 8 character secure password: 1 Uppercase, 1 Lowercase, 1 Number, 5 random
        var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        var result = new char[8];
        using (var rng = RandomNumberGenerator.Create())
        {
            var data = new byte[8];
            rng.GetBytes(data);
            for (int i = 0; i < 8; i++)
            {
                result[i] = chars[data[i] % chars.Length];
            }
        }
        
        // Ensure at least 1 uppercase, 1 lowercase, 1 digit
        result[0] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[result[0] % 26];
        result[1] = "abcdefghijklmnopqrstuvwxyz"[result[1] % 26];
        result[2] = "1234567890"[result[2] % 10];
        
        return new string(result);
    }
}
