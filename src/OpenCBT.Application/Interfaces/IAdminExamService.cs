using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IAdminExamService
{
    Task<IEnumerable<ExamDto>> GetAllExamsAsync();
    Task<ExamDto?> GetExamByIdAsync(Guid id);
    Task<ExamDto> CreateExamAsync(CreateExamDto createExamDto);
    Task UpdateExamAsync(Guid id, ExamDto updateExamDto);
    Task DeleteExamAsync(Guid id);
    Task<string> GenerateTokenAsync(Guid examId);
    
    // Questions Management
    Task<IEnumerable<QuestionDto>> GetQuestionsByExamIdAsync(Guid examId);
    Task<QuestionDto> AddQuestionAsync(Guid examId, QuestionDto questionDto);
    Task UpdateQuestionAsync(Guid questionId, QuestionDto questionDto);
    Task DeleteQuestionAsync(Guid questionId);

    // Grading & Analytics
    Task<ExamSessionDetailsDto?> GetSessionDetailsAsync(Guid sessionId);
    Task GradeEssayResponseAsync(Guid studentResponseId, decimal pointsObtained, string teacherFeedback);
    Task<ExamAnalyticsDto> GetExamAnalyticsAsync(Guid examId);
}
