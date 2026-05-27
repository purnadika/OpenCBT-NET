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

    public async Task<IEnumerable<Exam>> GetActiveExamsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(e => e.IsActive && e.StartTime <= now && e.EndTime >= now)
            .ToListAsync();
    }
}
