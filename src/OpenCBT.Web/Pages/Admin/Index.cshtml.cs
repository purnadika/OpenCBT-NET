using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalExams { get; set; }
    public int ActiveExams { get; set; }
    public int TotalStudents { get; set; }
    public int CompletedSessions { get; set; }

    // Chart Data
    public string TopStudentsJson { get; set; } = "[]";
    public string SessionHistoryJson { get; set; } = "[]";
    public string ScoreStatsJson { get; set; } = "[]";

    public class LeaderboardEntry
    {
        public string Name { get; set; } = string.Empty;
        public decimal Score { get; set; }
    }
    
    public class LeaderboardBoard
    {
        public string ExamTitle { get; set; } = string.Empty;
        public List<LeaderboardEntry> TopScores { get; set; } = new();
    }

    public List<LeaderboardBoard> Leaderboards { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalExams = await _context.Exams.CountAsync();
        ActiveExams = await _context.Exams.CountAsync(e => e.IsActive);
        
        // Count users in role Student
        var studentRoleId = await _context.Roles.Where(r => r.Name == "Student").Select(r => r.Id).FirstOrDefaultAsync();
        TotalStudents = await _context.UserRoles.CountAsync(ur => ur.RoleId == studentRoleId);
        
        CompletedSessions = await _context.ExamSessions.CountAsync(s => s.CompletedAt != null);

        // Fetch completed sessions with users and exams
        var completedSessions = await _context.ExamSessions
            .Include(s => s.User)
            .Include(s => s.Exam)
            .Where(s => s.CompletedAt != null && s.TotalScore != null)
            .ToListAsync();

        // 1. Top Students (Doughnut)
        var topStudents = completedSessions
            .GroupBy(s => s.UserId)
            .Select(g => new 
            { 
                Name = g.First().User?.FullName ?? "Unknown", 
                AvgScore = g.Average(s => s.TotalScore.Value) 
            })
            .OrderByDescending(x => x.AvgScore)
            .Take(10)
            .ToList();

        var colors = new[] { "#ff4d6d", "#2196f3", "#ffca28", "#26a69a", "#7e57c2", "#ffa726", "#00bfa5", "#ef5350", "#ab47bc", "#8d6e63" };
        var topStudentsFormatted = topStudents.Select((s, index) => new 
        { 
            label = s.Name, 
            value = Math.Round(s.AvgScore, 1), 
            color = colors[index % colors.Length] 
        });
        TopStudentsJson = System.Text.Json.JsonSerializer.Serialize(topStudentsFormatted);

        // 2. Session History (Line) - Last 6 months
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var sessionHistory = completedSessions
            .Where(s => s.CompletedAt >= sixMonthsAgo)
            .GroupBy(s => new { s.CompletedAt.Value.Year, s.CompletedAt.Value.Month })
            .Select(g => new 
            { 
                Year = g.Key.Year, 
                Month = g.Key.Month, 
                Count = g.Count() 
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        var historyFormatted = sessionHistory.Select(x => new 
        { 
            date = new DateTime(x.Year, x.Month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("MMM yyyy"), 
            count = x.Count 
        });
        
        // Ensure there's some default data if no history yet
        if (!historyFormatted.Any())
        {
            historyFormatted = new[] { new { date = DateTime.UtcNow.ToString("MMM yyyy"), count = 0 } };
        }
        SessionHistoryJson = System.Text.Json.JsonSerializer.Serialize(historyFormatted);

        // 3. Score Stats (Bar) - Average per Exam
        var scoreStats = completedSessions
            .GroupBy(s => s.ExamId)
            .Select(g => new 
            { 
                ExamTitle = g.First().Exam?.Title ?? "Unknown", 
                AvgScore = g.Average(s => s.TotalScore.Value) 
            })
            .OrderByDescending(x => x.AvgScore)
            .Take(10)
            .ToList();

        var scoreStatsFormatted = scoreStats.Select(x => new 
        { 
            exam = x.ExamTitle, 
            score = Math.Round(x.AvgScore, 1) 
        });
        ScoreStatsJson = System.Text.Json.JsonSerializer.Serialize(scoreStatsFormatted);

        // 4. Leaderboards (Top 2 recent exams)
        var recentExams = completedSessions
            .GroupBy(s => s.ExamId)
            .OrderByDescending(g => g.Max(s => s.CompletedAt))
            .Take(2)
            .ToList();

        foreach (var examGroup in recentExams)
        {
            var examTitle = examGroup.First().Exam?.Title ?? "Unknown Exam";
            var topScores = examGroup
                .OrderByDescending(s => s.TotalScore)
                .Take(5)
                .Select(s => new LeaderboardEntry 
                { 
                    Name = s.User?.FullName ?? "Unknown", 
                    Score = s.TotalScore ?? 0 
                })
                .ToList();

            Leaderboards.Add(new LeaderboardBoard 
            { 
                ExamTitle = examTitle, 
                TopScores = topScores 
            });
        }
    }
}
