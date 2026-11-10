namespace RaceDay.API.Services;

public interface IEventBannerStorageService
{
    Task UploadAsync(int eventId, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<(Stream Content, string ContentType)?> DownloadAsync(int eventId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int eventId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int eventId, CancellationToken cancellationToken = default);
}
