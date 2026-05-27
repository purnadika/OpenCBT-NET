using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginRequestDto request);
    Task ResetPasswordAsync(ResetPasswordDto request);
    Task<string> GenerateJwtTokenAsync(string userId, IEnumerable<string> roles);
    Task<bool> HasAdminAccessAsync(string token);
    Task LogoutAsync(string token);
}
