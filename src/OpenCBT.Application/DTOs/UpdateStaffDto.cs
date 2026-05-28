using System.ComponentModel.DataAnnotations;

namespace OpenCBT.Application.DTOs;

public class UpdateStaffDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Teacher";

    public string IdentifierNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
