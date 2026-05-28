using Microsoft.EntityFrameworkCore;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Infrastructure.Services;

public class ClassRoomService : IClassRoomService
{
    private readonly ApplicationDbContext _context;

    public ClassRoomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassRoom>> GetAllAsync()
    {
        return await _context.ClassRooms.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<ClassRoom?> GetByIdAsync(Guid id)
    {
        return await _context.ClassRooms.FindAsync(id);
    }

    public async Task<ClassRoom> CreateAsync(ClassRoom classRoom)
    {
        _context.ClassRooms.Add(classRoom);
        await _context.SaveChangesAsync();
        return classRoom;
    }

    public async Task UpdateAsync(Guid id, ClassRoom classRoom)
    {
        var existing = await _context.ClassRooms.FindAsync(id);
        if (existing == null) throw new Exception("ClassRoom not found");

        existing.Name = classRoom.Name;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var classRoom = await _context.ClassRooms.FindAsync(id);
        if (classRoom != null)
        {
            _context.ClassRooms.Remove(classRoom);
            await _context.SaveChangesAsync();
        }
    }
}
