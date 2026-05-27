using Microsoft.AspNetCore.Http;

namespace OpenCBT.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to local or cloud storage and returns the relative or absolute public URL path.
    /// </summary>
    Task<string> SaveFileAsync(IFormFile file, string folderName);

    /// <summary>
    /// Deletes a file from storage if it exists.
    /// </summary>
    Task DeleteFileAsync(string fileUrl);
}
