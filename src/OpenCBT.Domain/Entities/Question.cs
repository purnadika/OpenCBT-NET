namespace OpenCBT.Domain.Entities;

public class Question : BaseEntity
{
    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int OrderIndex { get; set; }
    public decimal Points { get; set; } = 1.0m;

    public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
}
