using Application.Enums;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Sonara.Tests.Infrastructure;

public sealed class FakeBlobService : IBlobService
{
    public Func<string, TimeSpan, string?> ReadUrlFactory { get; set; } =
        (fileUrl, lifetime) => $"{fileUrl}?sig=fake&ttl={(int)lifetime.TotalMinutes}";

    public List<string> Uploaded { get; } = new();
    public List<string> Deleted { get; } = new();

    public Task<string> UploadFileAsync(IFormFile file, BlobFolder folder)
    {
        var url = $"https://cdn.test/{folder}/{Guid.NewGuid()}";
        Uploaded.Add(url);
        return Task.FromResult(url);
    }

    public Task DeleteFileAsync(string fileUrl, BlobFolder folder)
    {
        Deleted.Add(fileUrl);
        return Task.CompletedTask;
    }

    public async Task<string> ReplaceFileAsync(IFormFile newFile, string? oldFileUrl, BlobFolder folder)
    {
        if (!string.IsNullOrWhiteSpace(oldFileUrl))
        {
            await DeleteFileAsync(oldFileUrl, folder);
        }

        return await UploadFileAsync(newFile, folder);
    }

    public string? TryCreateReadUrl(string fileUrl, TimeSpan lifetime) => ReadUrlFactory(fileUrl, lifetime);
}
