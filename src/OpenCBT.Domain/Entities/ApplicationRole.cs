using Microsoft.AspNetCore.Identity;

namespace OpenCBT.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    
    public ApplicationRole(string roleName) : base(roleName)
    {
    }
}
