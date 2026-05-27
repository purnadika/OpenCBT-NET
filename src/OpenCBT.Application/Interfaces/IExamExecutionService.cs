using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IExamExecutionService
{
    Task<ExamSessionDto> StartExamAsync(StartExamDto request);
    Task SubmitAnswerAsync(SubmitAnswerDto request);
    Task PingHeartbeatAsync(Guid sessionId);
    Task<ExamSessionDto> ForceSubmitExamAsync(Guid sessionId);
}
