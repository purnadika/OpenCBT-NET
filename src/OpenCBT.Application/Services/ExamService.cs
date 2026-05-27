using Mapster;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Exceptions;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;

namespace OpenCBT.Application.Services;

public class ExamService : IExamService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExamService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ExamDto>> GetActiveExamsAsync()
    {
        var exams = await _unitOfWork.Exams.GetActiveExamsAsync();
        return exams.Adapt<IEnumerable<ExamDto>>();
    }

    public async Task<ExamDto?> GetExamByIdAsync(Guid id)
    {
        // Using Generic Repository FindAsync and loading navigation properties in UI or modifying Repo
        var exam = await _unitOfWork.Exams.GetByIdAsync(id);
        return exam?.Adapt<ExamDto>();
    }

    public async Task<ExamDto> CreateExamAsync(CreateExamDto createExamDto)
    {
        var exam = createExamDto.Adapt<Exam>();
        exam.IsActive = true;
        
        await _unitOfWork.Exams.AddAsync(exam);
        await _unitOfWork.CompleteAsync();
        
        return exam.Adapt<ExamDto>();
    }

    public async Task<ExamSessionDto> StartExamAsync(StartExamRequestDto request)
    {
        var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId);
        
        if (exam == null)
            throw new ValidationException($"Exam with id {request.ExamId} not found.");

        if (!exam.IsActive)
            throw new ValidationException("Cannot start an inactive exam.");

        if (exam.TokenRequired)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                throw new ValidationException("This exam requires a token to start.");
                
            if (exam.CurrentToken != request.Token.Trim().ToUpperInvariant())
                throw new ValidationException("Invalid token.");
        }

        var now = DateTime.UtcNow;
        if (now < exam.StartTime || now > exam.EndTime)
            throw new ValidationException("Exam is outside the allowed time window.");

        var existingSessions = await _unitOfWork.ExamSessions.FindAsync(s => s.ExamId == request.ExamId && s.UserId == request.UserId && s.CompletedAt == null);
        var existingSession = existingSessions.FirstOrDefault();

        if (existingSession != null)
        {
            var existingDto = existingSession.Adapt<ExamSessionDto>();
            existingDto.Status = "InProgress";
            return existingDto;
        }

        var session = new ExamSession
        {
            ExamId = request.ExamId,
            UserId = request.UserId,
            StartedAt = now
        };

        await _unitOfWork.ExamSessions.AddAsync(session);
        await _unitOfWork.CompleteAsync();

        var dto = session.Adapt<ExamSessionDto>();
        dto.Status = "InProgress";
        return dto;
    }

    public async Task SubmitAnswerAsync(Guid examSessionId, Guid questionId, Guid answerOptionId)
    {
        var session = await _unitOfWork.ExamSessions.GetSessionWithDetailsAsync(examSessionId);

        if (session == null || session.CompletedAt != null)
            throw new ValidationException("Exam session is invalid or already completed.");
            
        // Enforce time limit logic here
        var timeTaken = DateTime.UtcNow - session.StartedAt;
        if (timeTaken.TotalMinutes > session.Exam.DurationMinutes)
        {
             await CompleteExamAsync(examSessionId);
             throw new ValidationException("Exam time has expired.");
        }

        var existingResponse = session.Responses.FirstOrDefault(r => r.QuestionId == questionId);
        if (existingResponse != null)
        {
            existingResponse.SelectedAnswerOptionId = answerOptionId;
            existingResponse.EssayAnswer = null; // Clear if somehow was essay
        }
        else
        {
            session.Responses.Add(new StudentResponse
            {
                QuestionId = questionId,
                SelectedAnswerOptionId = answerOptionId
            });
        }

        await _unitOfWork.CompleteAsync();
    }

    public async Task SubmitEssayAnswerAsync(Guid examSessionId, Guid questionId, string essayAnswer)
    {
        var session = await _unitOfWork.ExamSessions.GetSessionWithDetailsAsync(examSessionId);

        if (session == null || session.CompletedAt != null)
            throw new ValidationException("Exam session is invalid or already completed.");

        var timeTaken = DateTime.UtcNow - session.StartedAt;
        if (timeTaken.TotalMinutes > session.Exam.DurationMinutes)
        {
             await CompleteExamAsync(examSessionId);
             throw new ValidationException("Exam time has expired.");
        }

        var existingResponse = session.Responses.FirstOrDefault(r => r.QuestionId == questionId);
        if (existingResponse != null)
        {
            existingResponse.EssayAnswer = essayAnswer;
            existingResponse.SelectedAnswerOptionId = null; // Clear if somehow was option
        }
        else
        {
            session.Responses.Add(new StudentResponse
            {
                QuestionId = questionId,
                EssayAnswer = essayAnswer
            });
        }

        await _unitOfWork.CompleteAsync();
    }

    public async Task<ExamSessionDto> CompleteExamAsync(Guid examSessionId)
    {
        var session = await _unitOfWork.ExamSessions.GetSessionWithDetailsAsync(examSessionId);

        if (session == null)
            throw new KeyNotFoundException("Session not found.");

        if (session.CompletedAt != null)
            return session.Adapt<ExamSessionDto>();

        var questionIds = session.Responses.Select(r => r.QuestionId).ToList();
        var allOptions = await _unitOfWork.AnswerOptions.FindAsync(o => questionIds.Contains(o.QuestionId) && o.IsCorrect);
        var correctOptions = allOptions.ToDictionary(o => o.QuestionId, o => o.Id);

        decimal totalScore = 0;
        
        var questionsEntities = await _unitOfWork.Questions.FindAsync(q => questionIds.Contains(q.Id));
        var questions = questionsEntities.ToDictionary(q => q.Id, q => q.Points);

        foreach (var response in session.Responses)
        {
            if (correctOptions.TryGetValue(response.QuestionId, out var correctId))
            {
                if (response.SelectedAnswerOptionId == correctId)
                {
                    totalScore += questions.GetValueOrDefault(response.QuestionId, 1.0m);
                }
            }
        }

        session.CompletedAt = DateTime.UtcNow;
        session.TotalScore = totalScore;
        
        await _unitOfWork.CompleteAsync();
        return session.Adapt<ExamSessionDto>();
    }

    public async Task<IEnumerable<ExamSessionDto>> GetStudentExamHistoryAsync(Guid userId)
    {
        var sessions = await _unitOfWork.ExamSessions.FindAsync(s => s.UserId == userId && s.CompletedAt != null);
        
        var examIds = sessions.Select(s => s.ExamId).Distinct().ToList();
        var exams = await _unitOfWork.Exams.FindAsync(e => examIds.Contains(e.Id));
        var examDict = exams.ToDictionary(e => e.Id, e => e.Title);

        var dtos = sessions.OrderByDescending(s => s.CompletedAt).Adapt<List<ExamSessionDto>>();
        foreach(var dto in dtos)
        {
            if (examDict.TryGetValue(dto.ExamId, out var title))
            {
                dto.ExamTitle = title;
            }
        }
        
        return dtos;
    }

    public async Task<ExamSessionDetailsDto?> GetStudentSessionReviewAsync(Guid sessionId, Guid userId)
    {
        var session = await _unitOfWork.ExamSessions.GetSessionWithDetailsAsync(sessionId);
        if (session == null) return null;

        // Secure Authorization Check: Ensure student can only review their own sessions!
        if (session.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this exam review.");
        }

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
}
