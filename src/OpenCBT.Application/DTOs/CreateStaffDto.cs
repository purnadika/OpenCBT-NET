using System.ComponentModel.DataAnnotations;

namespace OpenCBT.Application.DTOs;

public class CreateStaffDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Required]
    public string Role { get; set; } = "Teacher";

    public string IdentifierNumber { get; set; } = string.Empty;
    
    public bool MustChangePassword { get; set; } = true;
}
