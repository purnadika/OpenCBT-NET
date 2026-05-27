using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;
using System.Linq.Expressions;

namespace OpenCBT.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IExamRepository Exams { get; }
    IGenericRepository<Question> Questions { get; }
    IGenericRepository<AnswerOption> AnswerOptions { get; }
    IExamSessionRepository ExamSessions { get; }
    Task<int> CompleteAsync();
}

public interface IExamSessionRepository : IGenericRepository<ExamSession>
{
    Task<ExamSession?> GetSessionWithDetailsAsync(Guid sessionId);
    Task<IEnumerable<ExamSession>> GetSessionsWithDetailsByExamIdAsync(Guid examId);
}
