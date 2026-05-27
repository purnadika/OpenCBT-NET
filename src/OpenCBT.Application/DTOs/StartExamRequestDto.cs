using System;

namespace OpenCBT.Application.DTOs;

public class StartExamRequestDto
{
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }
    public string? Token { get; set; }
}
