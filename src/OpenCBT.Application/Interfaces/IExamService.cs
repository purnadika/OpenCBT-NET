using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IExamService
{
    Task<IEnumerable<ExamDto>> GetActiveExamsAsync(Guid userId);
    Task<ExamDto?> GetExamByIdAsync(Guid id);
    Task<ExamDto> CreateExamAsync(CreateExamDto createExamDto);
    
    // Exam Taking Logic
    Task<ExamSessionDto> StartExamAsync(StartExamRequestDto request);
    Task SubmitAnswerAsync(Guid examSessionId, Guid questionId, Guid answerOptionId);
    Task SubmitEssayAnswerAsync(Guid examSessionId, Guid questionId, string essayAnswer);
    Task<ExamSessionDto> CompleteExamAsync(Guid examSessionId);
    
    // Student Dashboard & History
    Task<IEnumerable<ExamSessionDto>> GetStudentExamHistoryAsync(Guid userId);
    Task<ExamSessionDetailsDto?> GetStudentSessionReviewAsync(Guid sessionId, Guid userId);
}
