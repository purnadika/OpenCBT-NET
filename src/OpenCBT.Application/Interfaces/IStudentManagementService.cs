using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IStudentManagementService
{
    Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
    Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto);
    Task<StudentDto> UpdateStudentAsync(Guid id, StudentDto studentDto);
    Task DeleteStudentAsync(Guid id);
    Task<string> ResetStudentPasswordAsync(Guid id);
    
    // Profile Review Workflow
    Task<ProfileUpdateRequestDto?> GetPendingProfileUpdateAsync(Guid studentId);
    Task SubmitProfileUpdateAsync(Guid studentId, SubmitProfileUpdateDto dto);
    Task<IEnumerable<ProfileUpdateRequestDto>> GetAllPendingProfileUpdatesAsync();
    Task ApproveProfileUpdateAsync(Guid requestId, Guid adminId);
    Task RejectProfileUpdateAsync(Guid requestId, Guid adminId);

    Task<(int SavedCount, IEnumerable<string> Errors)> BulkImportStudentsAsync(Stream excelFileStream);
}
