using Microsoft.AspNetCore.SignalR;

namespace OpenCBT.Web.Hubs;

public class ProctorHub : Hub
{
    // Join exam proctoring room
    public async Task JoinExamRoom(string examId, string role)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, examId);
        
        if (role == "Proctor")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{examId}_Proctors");
        }
        else
        {
            // Notify proctors that a student joined
            await Clients.Group($"{examId}_Proctors").SendAsync("StudentConnected", Context.ConnectionId, Context.User?.Identity?.Name);
        }
    }

    // Student reports progress
    public async Task ReportProgress(string examId, string studentName, int questionIndex, int totalQuestions)
    {
        await Clients.Group($"{examId}_Proctors").SendAsync("StudentProgressUpdated", Context.ConnectionId, studentName, questionIndex, totalQuestions);
    }

    // Proctor sends a broadcast message to all students in the exam
    public async Task SendBroadcastMessage(string examId, string message)
    {
        await Clients.Group(examId).SendAsync("BroadcastReceived", message);
    }

    // Proctor sends a personal message to a specific student connection ID
    public async Task SendPersonalMessage(string studentConnectionId, string message)
    {
        await Clients.Client(studentConnectionId).SendAsync("PersonalMessageReceived", Context.ConnectionId, message);
    }

    // Student replies to a proctor's personal message
    public async Task ReplyToProctor(string proctorConnectionId, string message)
    {
        await Clients.Client(proctorConnectionId).SendAsync("StudentReplyReceived", Context.ConnectionId, Context.User?.Identity?.Name ?? "Student", message);
    }

    // Proctor forces a specific student to submit their exam
    public async Task ForceSubmitStudent(string studentConnectionId)
    {
        await Clients.Client(studentConnectionId).SendAsync("ForceSubmitReceived");
    }

    // Proctor forces all students in the exam to submit immediately
    public async Task ForceSubmitAll(string examId)
    {
        await Clients.Group(examId).SendAsync("ForceSubmitReceived");
    }
}
