using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services;

public sealed class EventBannerStorageService : IEventBannerStorageService
{
    private readonly BlobContainerClient _container;

    public EventBannerStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        var containerName = configuration["BlobStorage:ContainerName"] ?? "event-banners";
        _container = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task UploadAsync(int eventId, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var blob = _container.GetBlobClient($"event-{eventId}-banner");
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);
    }

    public async Task<(Stream Content, string ContentType)?> DownloadAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient($"event-{eventId}-banner");
        try
        {
            var response = await blob.DownloadContentAsync(cancellationToken);
            var content = new MemoryStream(response.Value.Content.ToArray());
            return (content, response.Value.Details.ContentType ?? "application/octet-stream");
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<bool> ExistsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient($"event-{eventId}-banner");
        var exists = await blob.ExistsAsync(cancellationToken);
        return exists.Value;
    }

    public async Task DeleteAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient($"event-{eventId}-banner");
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
