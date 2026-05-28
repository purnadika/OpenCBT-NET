using OpenCBT.Domain.Entities;

namespace OpenCBT.Application.Interfaces;

public interface IClassRoomService
{
    Task<IEnumerable<ClassRoom>> GetAllAsync();
    Task<ClassRoom?> GetByIdAsync(Guid id);
    Task<ClassRoom> CreateAsync(ClassRoom classRoom);
    Task UpdateAsync(Guid id, ClassRoom classRoom);
    Task DeleteAsync(Guid id);
}
