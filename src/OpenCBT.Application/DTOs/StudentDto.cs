namespace OpenCBT.Application.DTOs;

public class StudentDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentifierNumber { get; set; } = string.Empty; // e.g. NISN for students
    public bool IsActive { get; set; }
}

public class CreateStudentDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentifierNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
