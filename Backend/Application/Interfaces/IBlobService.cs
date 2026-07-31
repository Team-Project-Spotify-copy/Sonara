using Application.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(IFormFile file, BlobFolder folder);
        Task DeleteFileAsync(string fileUrl, BlobFolder folder);
        Task<string> ReplaceFileAsync(IFormFile newFile, string? oldFileUrl, BlobFolder folder);
    }
}