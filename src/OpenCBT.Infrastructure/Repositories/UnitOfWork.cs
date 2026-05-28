using Microsoft.EntityFrameworkCore;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public IExamRepository Exams { get; private set; }
    private IGenericRepository<Question>? _questions;
    private IGenericRepository<AnswerOption>? _answerOptions;
    private IExamSessionRepository? _examSessions;
    private IGenericRepository<ProfileUpdateRequest>? _profileUpdateRequests;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Exams = new ExamRepository(_context);
    }

    public IGenericRepository<Question> Questions => _questions ??= new GenericRepository<Question>(_context);
    public IGenericRepository<AnswerOption> AnswerOptions => _answerOptions ??= new GenericRepository<AnswerOption>(_context);

    public IGenericRepository<ProfileUpdateRequest> ProfileUpdateRequests => _profileUpdateRequests ??= new GenericRepository<ProfileUpdateRequest>(_context);

    public IExamSessionRepository ExamSessions => _examSessions ??= new ExamSessionRepository(_context);

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
