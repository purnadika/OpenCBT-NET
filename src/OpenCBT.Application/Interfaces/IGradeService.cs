using OpenCBT.Domain.Entities;

namespace OpenCBT.Application.Interfaces;

public interface IGradeService
{
    Task<IEnumerable<Grade>> GetAllAsync();
    Task<Grade?> GetByIdAsync(Guid id);
    Task<Grade> CreateAsync(Grade grade);
    Task UpdateAsync(Guid id, Grade grade);
    Task DeleteAsync(Guid id);
}
