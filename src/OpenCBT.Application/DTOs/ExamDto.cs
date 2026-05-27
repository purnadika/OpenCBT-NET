using OpenCBT.Domain.Enums;

namespace OpenCBT.Application.DTOs;

public class ExamDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public DisplayMode DisplayMode { get; set; }
    public bool TokenRequired { get; set; }
    public string? CurrentToken { get; set; }
    public bool RandomizeQuestions { get; set; }
    
    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int OrderIndex { get; set; }
    public decimal Points { get; set; }
    public List<AnswerOptionDto> Options { get; set; } = new();
}

public class AnswerOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsCorrect { get; set; }
}

public class CreateExamDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public DisplayMode DisplayMode { get; set; }
    public bool TokenRequired { get; set; }
    public bool RandomizeQuestions { get; set; }
}

public class ExamSessionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? TotalScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
}

public class ExamAnalyticsDto
{
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int TotalParticipants { get; set; }
    public int CompletedCount { get; set; }
    public double CompletionRate { get; set; }
    public decimal AverageScore { get; set; }
    public decimal HighestScore { get; set; }
    public decimal LowestScore { get; set; }
    public List<StudentPerformanceDto> StudentPerformances { get; set; } = new();
    public List<ItemAnalysisDto> ItemAnalysis { get; set; } = new();
}

public class StudentPerformanceDto
{
    public Guid SessionId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdentifierNumber { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? Score { get; set; }
    public bool HasPendingEssays { get; set; }
}

public class ItemAnalysisDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public decimal Points { get; set; }
    public decimal SuccessRatePercent { get; set; }
}

public class ExamSessionDetailsDto
{
    public Guid SessionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentIdentifier { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public List<StudentResponseDetailsDto> Responses { get; set; } = new();
}

public class StudentResponseDetailsDto
{
    public Guid ResponseId { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public decimal MaxPoints { get; set; }
    public string? SelectedOptionText { get; set; }
    public string? CorrectOptionText { get; set; }
    public bool IsMultipleChoiceCorrect { get; set; }
    public string? EssayAnswer { get; set; }
    public decimal? PointsObtained { get; set; }
    public string? TeacherFeedback { get; set; }
}
