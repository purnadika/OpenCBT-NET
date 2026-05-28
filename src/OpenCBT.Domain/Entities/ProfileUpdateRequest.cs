using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenCBT.Domain.Entities;

public class ProfileUpdateRequest : BaseEntity
{

    [Required]
    public Guid StudentId { get; set; }

    [ForeignKey("StudentId")]
    public virtual ApplicationUser Student { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string RequestedFullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string RequestedEmail { get; set; } = string.Empty;

    [MaxLength(256)]
    public string RequestedIdentifierNumber { get; set; } = string.Empty;

    // "Pending", "Approved", "Rejected"
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedById { get; set; }

    [ForeignKey("ReviewedById")]
    public virtual ApplicationUser? ReviewedBy { get; set; }
}
