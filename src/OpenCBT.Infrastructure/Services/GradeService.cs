using Microsoft.EntityFrameworkCore;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Infrastructure.Services;

public class GradeService : IGradeService
{
    private readonly ApplicationDbContext _context;

    public GradeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Grade>> GetAllAsync()
    {
        return await _context.Grades.OrderBy(g => g.Name).ToListAsync();
    }

    public async Task<Grade?> GetByIdAsync(Guid id)
    {
        return await _context.Grades.FindAsync(id);
    }

    public async Task<Grade> CreateAsync(Grade grade)
    {
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();
        return grade;
    }

    public async Task UpdateAsync(Guid id, Grade grade)
    {
        var existing = await _context.Grades.FindAsync(id);
        if (existing == null) throw new Exception("Grade not found");

        existing.Name = grade.Name;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade != null)
        {
            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
        }
    }
}
