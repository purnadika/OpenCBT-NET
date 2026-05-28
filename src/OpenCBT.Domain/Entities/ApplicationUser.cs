using Microsoft.AspNetCore.Identity;

namespace OpenCBT.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string IdentifierNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? GradeId { get; set; }
    public Grade? Grade { get; set; }

    public Guid? ClassRoomId { get; set; }
    public ClassRoom? ClassRoom { get; set; }
}
