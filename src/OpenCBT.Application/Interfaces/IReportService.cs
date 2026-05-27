namespace OpenCBT.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateExcelReportAsync(Guid examId);
}
