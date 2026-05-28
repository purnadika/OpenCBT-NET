using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;

namespace OpenCBT.Application.Services;

public class StudentManagementService : IStudentManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public StudentManagementService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
    {
        var students = await _userManager.GetUsersInRoleAsync("Student");
        var studentIds = students.Select(s => s.Id).ToList();

        var usersWithNav = await _userManager.Users
            .Include(u => u.Grade)
            .Include(u => u.ClassRoom)
            .Where(u => studentIds.Contains(u.Id))
            .ToListAsync();

        return usersWithNav.Select(s => new StudentDto
        {
            Id = s.Id,
            FullName = s.FullName,
            Email = s.Email ?? string.Empty,
            IdentifierNumber = s.IdentifierNumber,
            IsActive = s.IsActive,
            GradeId = s.GradeId,
            GradeName = s.Grade?.Name,
            ClassRoomId = s.ClassRoomId,
            ClassRoomName = s.ClassRoom?.Name
        });
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
    {
        // 1. Validation: NISN must be unique
        var existingByIdentifier = await _userManager.Users.FirstOrDefaultAsync(u => u.IdentifierNumber == dto.IdentifierNumber);
        if (existingByIdentifier != null)
        {
            throw new OpenCBT.Application.Exceptions.ValidationException("Error_NisnRegistered", dto.IdentifierNumber);
        }

        // 2. Validation: Email must be unique
        var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingByEmail != null)
        {
            throw new OpenCBT.Application.Exceptions.ValidationException("Error_EmailRegistered", dto.Email);
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            IdentifierNumber = dto.IdentifierNumber,
            IsActive = true,
            GradeId = dto.GradeId,
            ClassRoomId = dto.ClassRoomId
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
            IsActive = user.IsActive,
            GradeId = user.GradeId,
            ClassRoomId = user.ClassRoomId
        };
    }

    public async Task<StudentDto> UpdateStudentAsync(Guid id, StudentDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new OpenCBT.Application.Exceptions.ValidationException("Error_StudentNotFound");

        // 1. Validation: NISN must be unique (exclude self)
        var existingByIdentifier = await _userManager.Users
            .FirstOrDefaultAsync(u => u.IdentifierNumber == dto.IdentifierNumber && u.Id != id);
        if (existingByIdentifier != null)
        {
            throw new OpenCBT.Application.Exceptions.ValidationException("Error_NisnRegistered", dto.IdentifierNumber);
        }

        // 2. Validation: Email must be unique (exclude self)
        var existingByEmail = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != id);
        if (existingByEmail != null)
        {
            throw new OpenCBT.Application.Exceptions.ValidationException("Error_EmailRegistered", dto.Email);
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.IdentifierNumber = dto.IdentifierNumber;
        user.IsActive = dto.IsActive;
        user.GradeId = dto.GradeId;
        user.ClassRoomId = dto.ClassRoomId;

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
            IsActive = user.IsActive,
            GradeId = user.GradeId,
            ClassRoomId = user.ClassRoomId
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
        if (user == null) throw new OpenCBT.Application.Exceptions.ValidationException("Error_StudentNotFound");

        // Securely generate a temporary random password
        // Requires: Length 10, contains digits, uppercase, lowercase
        var chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$";
        var randomBytes = new byte[12];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var tempPasswordBuilder = new System.Text.StringBuilder();
        tempPasswordBuilder.Append("Cbt");
        tempPasswordBuilder.Append(randomBytes[0] % 10);
        for (int i = 1; i < 8; i++)
        {
            tempPasswordBuilder.Append(chars[randomBytes[i] % chars.Length]);
        }
        return tempPasswordBuilder.ToString();
    }

    public async Task<ProfileUpdateRequestDto?> GetPendingProfileUpdateAsync(Guid studentId)
    {
        var requests = await _unitOfWork.ProfileUpdateRequests.FindAsync(r => r.StudentId == studentId && r.Status == "Pending");
        var request = requests.OrderByDescending(r => r.SubmittedAt).FirstOrDefault();

        if (request == null) return null;

        return new ProfileUpdateRequestDto
        {
            Id = request.Id,
            StudentId = request.StudentId,
            RequestedFullName = request.RequestedFullName,
            RequestedEmail = request.RequestedEmail,
            RequestedIdentifierNumber = request.RequestedIdentifierNumber,
            Status = request.Status,
            SubmittedAt = request.SubmittedAt
        };
    }

    public async Task SubmitProfileUpdateAsync(Guid studentId, SubmitProfileUpdateDto dto)
    {
        var user = await _userManager.FindByIdAsync(studentId.ToString());
        if (user == null) throw new Exception("User not found");

        var existingPendingList = await _unitOfWork.ProfileUpdateRequests.FindAsync(r => r.StudentId == studentId && r.Status == "Pending");
        var existingPending = existingPendingList.FirstOrDefault();

        if (existingPending != null)
        {
            existingPending.Status = "Superseded";
            _unitOfWork.ProfileUpdateRequests.Update(existingPending);
        }

        var request = new ProfileUpdateRequest
        {
            StudentId = studentId,
            RequestedFullName = dto.RequestedFullName,
            RequestedEmail = dto.RequestedEmail,
            RequestedIdentifierNumber = dto.RequestedIdentifierNumber,
            Status = "Pending",
            SubmittedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProfileUpdateRequests.AddAsync(request);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<ProfileUpdateRequestDto>> GetAllPendingProfileUpdatesAsync()
    {
        var requests = await _unitOfWork.ProfileUpdateRequests.FindAsync(r => r.Status == "Pending");
        var dtos = new List<ProfileUpdateRequestDto>();
        
        foreach (var r in requests.OrderBy(x => x.SubmittedAt))
        {
            var student = await _userManager.FindByIdAsync(r.StudentId.ToString());
            if (student != null)
            {
                dtos.Add(new ProfileUpdateRequestDto
                {
                    Id = r.Id,
                    StudentId = r.StudentId,
                    StudentName = student.FullName,
                    CurrentEmail = student.Email ?? string.Empty,
                    RequestedFullName = r.RequestedFullName,
                    RequestedEmail = r.RequestedEmail,
                    RequestedIdentifierNumber = r.RequestedIdentifierNumber,
                    Status = r.Status,
                    SubmittedAt = r.SubmittedAt
                });
            }
        }
        
        return dtos;
    }

    public async Task ApproveProfileUpdateAsync(Guid requestId, Guid adminId)
    {
        var request = await _unitOfWork.ProfileUpdateRequests.GetByIdAsync(requestId);
        if (request == null || request.Status != "Pending") throw new Exception("Invalid request");

        var student = await _userManager.FindByIdAsync(request.StudentId.ToString());
        if (student == null) throw new Exception("Student not found");

        if (student.Email != request.RequestedEmail)
        {
            var existing = await _userManager.FindByEmailAsync(request.RequestedEmail);
            if (existing != null && existing.Id != student.Id)
                throw new OpenCBT.Application.Exceptions.ValidationException("Error_EmailRegistered", request.RequestedEmail);
            
            student.Email = request.RequestedEmail;
            student.UserName = request.RequestedEmail;
        }

        student.FullName = request.RequestedFullName;
        student.IdentifierNumber = request.RequestedIdentifierNumber;

        await _userManager.UpdateAsync(student);

        request.Status = "Approved";
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedById = adminId;
        _unitOfWork.ProfileUpdateRequests.Update(request);
        await _unitOfWork.CompleteAsync();
    }

    public async Task RejectProfileUpdateAsync(Guid requestId, Guid adminId)
    {
        var request = await _unitOfWork.ProfileUpdateRequests.GetByIdAsync(requestId);
        if (request == null || request.Status != "Pending") throw new Exception("Invalid request");

        request.Status = "Rejected";
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedById = adminId;
        _unitOfWork.ProfileUpdateRequests.Update(request);
        await _unitOfWork.CompleteAsync();
    }

    public Task<(int SavedCount, IEnumerable<string> Errors)> BulkImportStudentsAsync(Stream excelFileStream)
    {
        // Stub to satisfy interface for TDD
        throw new NotImplementedException();
    }
}
