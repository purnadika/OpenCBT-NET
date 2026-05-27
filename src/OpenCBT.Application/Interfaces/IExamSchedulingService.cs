using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IExamSchedulingService
{
    Task<ExamScheduleDto> CreateExamAsync(CreateExamScheduleDto createExamDto);
    Task<string> GenerateTokenAsync(Guid examId);
    Task<string> RefreshExamTokenAsync(Guid examId);
    Task DeleteExamAsync(Guid examId);
}
