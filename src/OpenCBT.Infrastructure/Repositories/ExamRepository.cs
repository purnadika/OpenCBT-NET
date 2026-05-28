using Microsoft.EntityFrameworkCore;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Infrastructure.Repositories;

public class ExamRepository : GenericRepository<Exam>, IExamRepository
{
    public ExamRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Exam?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Exam>> GetActiveExamsAsync(Guid? gradeId = null)
    {
        var now = DateTime.UtcNow;
        var query = _dbSet.Where(e => e.IsActive && e.StartTime <= now && e.EndTime >= now);

        if (gradeId.HasValue)
        {
            query = query.Where(e => e.GradeId == gradeId.Value);
        }

        return await query.ToListAsync();
    }
}
