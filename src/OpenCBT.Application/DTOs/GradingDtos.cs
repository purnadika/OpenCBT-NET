namespace OpenCBT.Application.DTOs;

public class GradingResultDto
{
    public Guid SessionId { get; set; }
    public decimal Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public int UnansweredQuestions { get; set; }
}


