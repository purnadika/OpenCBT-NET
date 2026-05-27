namespace OpenCBT.Application.DTOs;

public class StartExamDto
{
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
}

public class SubmitAnswerDto
{
    public Guid SessionId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
    public string RequestId { get; set; } = string.Empty; // Idempotency key
}


