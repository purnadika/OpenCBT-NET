using Mapster;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;

namespace OpenCBT.Application.Services;

public class AdminExamService : IAdminExamService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminExamService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ExamDto>> GetAllExamsAsync()
    {
        var exams = await _unitOfWork.Exams.GetAllAsync();
        return exams.Adapt<IEnumerable<ExamDto>>();
    }

    public async Task<ExamDto?> GetExamByIdAsync(Guid id)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(id);
        return exam?.Adapt<ExamDto>();
    }

    public async Task<ExamDto> CreateExamAsync(CreateExamDto createExamDto)
    {
        var exam = createExamDto.Adapt<Exam>();
        exam.IsActive = true;
        exam.GradeId = createExamDto.GradeId;
        exam.TokenRequired = createExamDto.TokenRequired;
        exam.RandomizeQuestions = createExamDto.RandomizeQuestions;
        
        if (exam.TokenRequired)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            exam.CurrentToken = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        await _unitOfWork.Exams.AddAsync(exam);
        await _unitOfWork.CompleteAsync();
        
        return exam.Adapt<ExamDto>();
    }

    public async Task UpdateExamAsync(Guid id, ExamDto updateExamDto)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(id);
        if (exam == null) throw new KeyNotFoundException("Exam not found");

        exam.Title = updateExamDto.Title;
        exam.Description = updateExamDto.Description;
        exam.StartTime = updateExamDto.StartTime;
        exam.EndTime = updateExamDto.EndTime;
        exam.DurationMinutes = updateExamDto.DurationMinutes;
        exam.IsActive = updateExamDto.IsActive;
        exam.DisplayMode = updateExamDto.DisplayMode;
        exam.TokenRequired = updateExamDto.TokenRequired;
        exam.CurrentToken = updateExamDto.CurrentToken;

        _unitOfWork.Exams.Update(exam);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<string> GenerateTokenAsync(Guid examId)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
        if (exam == null) throw new KeyNotFoundException("Exam not found");

        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var token = new string(Enumerable.Repeat(chars, 5)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        exam.CurrentToken = token;
        _unitOfWork.Exams.Update(exam);
        await _unitOfWork.CompleteAsync();
        return token;
    }

    public async Task DeleteExamAsync(Guid id)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(id);
        if (exam != null)
        {
            _unitOfWork.Exams.Remove(exam);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task<IEnumerable<QuestionDto>> GetQuestionsByExamIdAsync(Guid examId)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
        if (exam == null) return new List<QuestionDto>();
        
        return exam.Questions.OrderBy(q => q.OrderIndex).Adapt<IEnumerable<QuestionDto>>();
    }

    public async Task<QuestionDto> AddQuestionAsync(Guid examId, QuestionDto questionDto)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
        if (exam == null) throw new KeyNotFoundException("Exam not found");

        var question = questionDto.Adapt<Question>();
        question.ExamId = examId;
        question.ImageUrl = questionDto.ImageUrl;
        
        await _unitOfWork.Questions.AddAsync(question);
        await _unitOfWork.CompleteAsync();

        return question.Adapt<QuestionDto>();
    }

    public async Task UpdateQuestionAsync(Guid questionId, QuestionDto questionDto)
    {
        var questions = await _unitOfWork.Questions.FindAsync(q => q.Id == questionId);
        var question = questions.FirstOrDefault();
        if (question == null) throw new KeyNotFoundException("Question not found");

        question.Text = questionDto.Text;
        question.ImageUrl = questionDto.ImageUrl;
        question.OrderIndex = questionDto.OrderIndex;
        question.Points = questionDto.Points;

        // Simplified for this phase: remove old options and add new ones
        var existingOptions = await _unitOfWork.AnswerOptions.FindAsync(o => o.QuestionId == questionId);
        foreach(var opt in existingOptions) {
             _unitOfWork.AnswerOptions.Remove(opt);
        }

        foreach (var optDto in questionDto.Options)
        {
            var newOpt = optDto.Adapt<AnswerOption>();
            newOpt.QuestionId = questionId;
            await _unitOfWork.AnswerOptions.AddAsync(newOpt);
        }

        _unitOfWork.Questions.Update(question);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteQuestionAsync(Guid questionId)
    {
        var questions = await _unitOfWork.Questions.FindAsync(q => q.Id == questionId);
        var question = questions.FirstOrDefault();
        if (question != null)
        {
            _unitOfWork.Questions.Remove(question);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task<ExamSessionDetailsDto?> GetSessionDetailsAsync(Guid sessionId)
    {
        var session = await _unitOfWork.ExamSessions.GetSessionWithDetailsAsync(sessionId);
        if (session == null) return null;

        var dto = new ExamSessionDetailsDto
        {
            SessionId = session.Id,
            StudentName = session.User?.FullName ?? "Unknown Student",
            StudentIdentifier = session.User?.IdentifierNumber ?? string.Empty,
            ExamTitle = session.Exam?.Title ?? "Unknown Exam",
            TotalScore = session.TotalScore
        };

        foreach (var response in session.Responses)
        {
            var question = response.Question;
            if (question == null) continue;

            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
            var isCorrect = false;
            if (correctOption != null && response.SelectedAnswerOptionId == correctOption.Id)
            {
                isCorrect = true;
            }

            dto.Responses.Add(new StudentResponseDetailsDto
            {
                ResponseId = response.Id,
                QuestionId = question.Id,
                QuestionText = question.Text,
                QuestionType = question.Options.Any() ? "MULTIPLE_CHOICE" : "ESSAY",
                MaxPoints = question.Points,
                SelectedOptionText = response.SelectedAnswerOption?.Text,
                CorrectOptionText = correctOption?.Text,
                IsMultipleChoiceCorrect = isCorrect,
                EssayAnswer = response.EssayAnswer,
                PointsObtained = response.PointsObtained,
                TeacherFeedback = response.TeacherFeedback
            });
        }

        return dto;
    }

    public async Task GradeEssayResponseAsync(Guid studentResponseId, decimal pointsObtained, string teacherFeedback)
    {
        // Find the session containing the response
        var sessions = await _unitOfWork.ExamSessions.FindAsync(s => s.Responses.Any(r => r.Id == studentResponseId));
        var session = sessions.FirstOrDefault();
        if (session == null) throw new KeyNotFoundException("Session not found");

        var detailedSession = await _unitOfWork.ExamSessions.GetSessionWithDetailsAsync(session.Id);
        if (detailedSession == null) throw new KeyNotFoundException("Session details not found");

        var response = detailedSession.Responses.First(r => r.Id == studentResponseId);
        response.PointsObtained = pointsObtained;
        response.TeacherFeedback = teacherFeedback;

        // Recalculate session total score:
        decimal newTotalScore = 0;
        foreach (var resp in detailedSession.Responses)
        {
            var question = resp.Question;
            if (question == null) continue;

            if (question.Options.Any())
            {
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                if (correctOption != null && resp.SelectedAnswerOptionId == correctOption.Id)
                {
                    newTotalScore += question.Points;
                }
            }
            else
            {
                newTotalScore += resp.PointsObtained ?? 0;
            }
        }

        detailedSession.TotalScore = newTotalScore;
        _unitOfWork.ExamSessions.Update(detailedSession);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<ExamAnalyticsDto> GetExamAnalyticsAsync(Guid examId)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
        if (exam == null) throw new KeyNotFoundException("Exam not found");

        var sessions = await _unitOfWork.ExamSessions.GetSessionsWithDetailsByExamIdAsync(examId);
        var completedSessions = sessions.Where(s => s.CompletedAt != null).ToList();

        var dto = new ExamAnalyticsDto
        {
            ExamId = examId,
            ExamTitle = exam.Title,
            TotalParticipants = sessions.Count(),
            CompletedCount = completedSessions.Count(),
            CompletionRate = sessions.Any() ? (double)completedSessions.Count() / sessions.Count() * 100 : 0
        };

        if (completedSessions.Any())
        {
            var scores = completedSessions.Select(s => s.TotalScore ?? 0).ToList();
            dto.AverageScore = scores.Average();
            dto.HighestScore = scores.Max();
            dto.LowestScore = scores.Min();
        }

        foreach (var session in sessions)
        {
            // Check if student has essays that haven't been graded yet
            bool hasPendingEssays = session.CompletedAt != null && session.Responses.Any(r => 
                !r.Question.Options.Any() && r.PointsObtained == null
            );

            dto.StudentPerformances.Add(new StudentPerformanceDto
            {
                SessionId = session.Id,
                FullName = session.User?.FullName ?? "Unknown Student",
                IdentifierNumber = session.User?.IdentifierNumber ?? string.Empty,
                StartedAt = session.StartedAt,
                CompletedAt = session.CompletedAt,
                Score = session.TotalScore,
                HasPendingEssays = hasPendingEssays
            });
        }

        // Item Analysis (calculate success rate for each question in this exam)
        var allQuestions = exam.Questions.OrderBy(q => q.OrderIndex).ToList();
        foreach (var question in allQuestions)
        {
            decimal successCount = 0;
            int totalResponsesForQuestion = 0;

            foreach (var session in completedSessions)
            {
                var response = session.Responses.FirstOrDefault(r => r.QuestionId == question.Id);
                if (response != null)
                {
                    totalResponsesForQuestion++;
                    if (question.Options.Any())
                    {
                        var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                        if (correctOption != null && response.SelectedAnswerOptionId == correctOption.Id)
                        {
                            successCount++;
                        }
                    }
                    else
                    {
                        // For essay, treat it as successful if they got >= 50% of maximum points
                        if (response.PointsObtained >= (question.Points / 2))
                        {
                            successCount++;
                        }
                    }
                }
            }

            dto.ItemAnalysis.Add(new ItemAnalysisDto
            {
                QuestionId = question.Id,
                QuestionText = question.Text,
                QuestionType = question.Options.Any() ? "Multiple Choice" : "Essay",
                Points = question.Points,
                SuccessRatePercent = totalResponsesForQuestion > 0 ? (successCount / totalResponsesForQuestion) * 100 : 0
            });
        }

        return dto;
    }
}
