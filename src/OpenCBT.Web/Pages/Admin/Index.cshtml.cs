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

    public async Task OnGetAsync()
    {
        TotalExams = await _context.Exams.CountAsync();
        ActiveExams = await _context.Exams.CountAsync(e => e.IsActive);
        
        // Count users in role Student
        var studentRoleId = await _context.Roles.Where(r => r.Name == "Student").Select(r => r.Id).FirstOrDefaultAsync();
        TotalStudents = await _context.UserRoles.CountAsync(ur => ur.RoleId == studentRoleId);
        
        CompletedSessions = await _context.ExamSessions.CountAsync(s => s.CompletedAt != null);

        // Mock data for charts to match UI exactly
        TopStudentsJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { label = "Phoebe", value = 85, color = "#ff4d6d" },
            new { label = "Zevan", value = 82, color = "#2196f3" },
            new { label = "Robi", value = 80, color = "#ffca28" },
            new { label = "Denny", value = 78, color = "#26a69a" },
            new { label = "Jokowi JK", value = 75, color = "#7e57c2" },
            new { label = "Agum Gumelar", value = 72, color = "#ffa726" },
            new { label = "Lintar", value = 70, color = "#00bfa5" },
            new { label = "Corbuzier", value = 68, color = "#ef5350" }
        });

        SessionHistoryJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { date = "Mar 2025", count = 2 },
            new { date = "Apr 2025", count = 1 },
            new { date = "May 2025", count = 8 }
        });

        ScoreStatsJson = System.Text.Json.JsonSerializer.Serialize(new[]
        {
            new { exam = "BINDO7-1", score = 95 },
            new { exam = "SR9-01", score = 80 },
            new { exam = "MAT9-02", score = 78 },
            new { exam = "IPA9-01", score = 40 }
        });
    }
}
