using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IGradingService
{
    Task<GradingResultDto> CalculateScoreAsync(Guid sessionId);
    Task<GradingResultDto> RecalculateScoreAsync(Guid sessionId, bool applyBonus);
    Task<ExamAnalyticsDto> GetExamAnalyticsAsync(Guid examId);
}
