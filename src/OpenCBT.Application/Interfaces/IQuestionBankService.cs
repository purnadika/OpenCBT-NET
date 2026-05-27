using OpenCBT.Application.DTOs;

namespace OpenCBT.Application.Interfaces;

public interface IQuestionBankService
{
    Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto createQuestionDto);
    Task<QuestionDto> UpdateQuestionAsync(Guid id, UpdateQuestionDto updateQuestionDto);
    Task<string> UploadQuestionImageAsync(Stream imageStream, string fileName, string contentType);
}
