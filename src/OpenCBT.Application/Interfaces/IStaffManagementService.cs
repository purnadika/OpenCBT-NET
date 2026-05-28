using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IStaffManagementService
{
    Task<IEnumerable<StaffDto>> GetAllStaffAsync();
    Task<StaffDto> CreateStaffAsync(CreateStaffDto dto);
    Task UpdateStaffAsync(Guid id, UpdateStaffDto dto);
    Task DeleteStaffAsync(Guid id);
    Task<string> ResetStaffPasswordAsync(Guid id);
}
