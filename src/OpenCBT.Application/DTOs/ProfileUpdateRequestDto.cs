namespace OpenCBT.Application.DTOs;

public class ProfileUpdateRequestDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CurrentEmail { get; set; } = string.Empty;
    public string RequestedFullName { get; set; } = string.Empty;
    public string RequestedEmail { get; set; } = string.Empty;
    public string RequestedIdentifierNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class SubmitProfileUpdateDto
{
    public string RequestedFullName { get; set; } = string.Empty;
    public string RequestedEmail { get; set; } = string.Empty;
    public string RequestedIdentifierNumber { get; set; } = string.Empty;
}
