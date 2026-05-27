using Microsoft.EntityFrameworkCore;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public IExamRepository Exams { get; private set; }
    public IGenericRepository<Question> Questions { get; private set; }
    public IGenericRepository<AnswerOption> AnswerOptions { get; private set; }
    public IExamSessionRepository ExamSessions { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Exams = new ExamRepository(_context);
        Questions = new GenericRepository<Question>(_context);
        AnswerOptions = new GenericRepository<AnswerOption>(_context);
        ExamSessions = new ExamSessionRepository(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

public class ExamSessionRepository : GenericRepository<ExamSession>, IExamSessionRepository
{
    public ExamSessionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ExamSession?> GetSessionWithDetailsAsync(Guid sessionId)
    {
        return await _dbSet
            .Include(s => s.Exam)
            .Include(s => s.User)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Question)
                    .ThenInclude(q => q.Options)
            .Include(s => s.Responses)
                .ThenInclude(r => r.SelectedAnswerOption)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<IEnumerable<ExamSession>> GetSessionsWithDetailsByExamIdAsync(Guid examId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Question)
                    .ThenInclude(q => q.Options)
            .Include(s => s.Responses)
                .ThenInclude(r => r.SelectedAnswerOption)
            .Where(s => s.ExamId == examId)
            .ToListAsync();
    }
}
