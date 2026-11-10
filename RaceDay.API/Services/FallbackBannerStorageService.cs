using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaceDay.API.Services;

public class FallbackBannerStorageService : IEventBannerStorageService
{
    public Task UploadAsync(int eventId, Stream content, string contentType, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<(Stream Content, string ContentType)?> DownloadAsync(int eventId, CancellationToken cancellationToken = default)
        => Task.FromResult<(Stream Content, string ContentType)?>(null);

    public Task<bool> ExistsAsync(int eventId, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task DeleteAsync(int eventId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}