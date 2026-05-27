namespace OpenCBT.Domain.Entities;

public class StudentResponse : BaseEntity
{
    public Guid ExamSessionId { get; set; }
    public ExamSession ExamSession { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public Guid? SelectedAnswerOptionId { get; set; }
    public AnswerOption? SelectedAnswerOption { get; set; }

    public string? EssayAnswer { get; set; }
    public decimal? PointsObtained { get; set; }
    public string? TeacherFeedback { get; set; }
}
