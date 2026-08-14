using Application.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(IFormFile file, BlobFolder folder);
        Task DeleteFileAsync(string fileUrl, BlobFolder folder);
        Task<string> ReplaceFileAsync(IFormFile newFile, string? oldFileUrl, BlobFolder folder);

        /// <summary>
        /// Builds a temporary read-only URL (Azure Blob SAS) for a stored file.
        /// Returns null when the configured credentials cannot sign URLs - the caller
        /// then falls back to the stored URL. Throws only on storage/URL failures.
        /// </summary>
        string? TryCreateReadUrl(string fileUrl, TimeSpan lifetime);
    }
}
