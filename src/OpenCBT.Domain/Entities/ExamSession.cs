namespace OpenCBT.Domain.Entities;

public class ExamSession : BaseEntity
{
    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public decimal? TotalScore { get; set; }
    
    public ICollection<StudentResponse> Responses { get; set; } = new List<StudentResponse>();
}
