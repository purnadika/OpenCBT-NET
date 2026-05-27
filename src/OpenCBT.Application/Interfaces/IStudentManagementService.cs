using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IStudentManagementService
{
    Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
    Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto);
    Task<StudentDto> UpdateStudentAsync(Guid id, StudentDto studentDto);
    Task DeleteStudentAsync(Guid id);
    Task<string> ResetStudentPasswordAsync(Guid id);
    Task<(int SavedCount, IEnumerable<string> Errors)> BulkImportStudentsAsync(Stream excelFileStream);
}
