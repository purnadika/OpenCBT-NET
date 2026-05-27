using OpenCBT.Domain.Enums;

namespace OpenCBT.Domain.Entities;

public class Exam : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    
    public DisplayMode DisplayMode { get; set; } = DisplayMode.Wizard;
    
    public bool RandomizeQuestions { get; set; } = false;
    
    public bool TokenRequired { get; set; } = false;
    public string? CurrentToken { get; set; }
    
    // Concurrency Token for optimistic concurrency (handling 1000 users starting simultaneously)
    public uint Version { get; set; }

    // Navigation properties
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
