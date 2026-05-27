using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Students;

public class PrintCardsModel : PageModel
{
    private readonly IStudentManagementService _studentService;
    private readonly IAdminExamService _examService;

    public PrintCardsModel(IStudentManagementService studentService, IAdminExamService examService)
    {
        _studentService = studentService;
        _examService = examService;
    }

    public IEnumerable<StudentDto> Students { get; set; } = new List<StudentDto>();
    public IEnumerable<ExamDto> Exams { get; set; } = new List<ExamDto>();

    public async Task OnGetAsync()
    {
        Students = await _studentService.GetAllStudentsAsync();
        Exams = await _examService.GetAllExamsAsync();
    }
}
