using Application.Enums;
using Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadFileAsync(IFormFile file, BlobFolder folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Файл порожній");

        var (containerName, folderPath) = GetStorageLocation(folder);

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var fileExtension = Path.GetExtension(file.FileName);
        var blobName = $"{folderPath}/{Guid.NewGuid()}{fileExtension}";

        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = file.ContentType
        };

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
        }

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileUrl, BlobFolder folder)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return;

        var (containerName, _) = GetStorageLocation(folder);
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var blobUriBuilder = new BlobUriBuilder(new Uri(fileUrl));
        var blobName = blobUriBuilder.BlobName;

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<string> ReplaceFileAsync(IFormFile newFile, string? oldFileUrl, BlobFolder folder)
    {
        if (!string.IsNullOrWhiteSpace(oldFileUrl))
        {
            await DeleteFileAsync(oldFileUrl, folder);
        }

        return await UploadFileAsync(newFile, folder);
    }

    public string? TryCreateReadUrl(string fileUrl, TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) || !Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var location = new BlobUriBuilder(uri);
        var blobClient = _blobServiceClient
            .GetBlobContainerClient(location.BlobContainerName)
            .GetBlobClient(location.BlobName);

        if (!blobClient.CanGenerateSasUri)
        {
            return null;
        }

        var sas = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(lifetime))
        {
            BlobContainerName = location.BlobContainerName,
            BlobName = location.BlobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        return blobClient.GenerateSasUri(sas).ToString();
    }

    private static (string ContainerName, string FolderPath) GetStorageLocation(BlobFolder folder) => folder switch
    {
        BlobFolder.Avatars => ("images", "avatars"),
        BlobFolder.AlbumsCovers => ("images", "albums"),
        BlobFolder.PlaylistsCovers => ("images", "playlists"),
        BlobFolder.PodcastsCovers => ("images", "podcasts"),

        BlobFolder.MusicTracks => ("audio", "tracks"),
        BlobFolder.PodcastAudio => ("audio", "podcasts"),

        _ => throw new ArgumentOutOfRangeException(nameof(folder), "Невідомий тип контенту")
    };
}
