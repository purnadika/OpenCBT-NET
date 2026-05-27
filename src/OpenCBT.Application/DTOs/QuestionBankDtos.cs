using System.ComponentModel.DataAnnotations;

namespace OpenCBT.Application.DTOs;

public class CreateQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public decimal WeightPoint { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class UpdateQuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public decimal WeightPoint { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}


