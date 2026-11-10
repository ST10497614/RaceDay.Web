
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class EventApiService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _configuration;

        private readonly JsonSerializerOptions _jsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public EventApiService(
            IHttpClientFactory factory,
            IConfiguration configuration)
        {
            _factory = factory;
            _configuration = configuration;
        }

        private HttpClient CreateClient(string? token = null)
        {
            var client = _factory.CreateClient();

            string baseUrl =
                _configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "API URL is missing.");

            client.BaseAddress = new Uri(
                baseUrl.TrimEnd('/') + "/");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<List<EventViewModel>> GetEventsAsync(
            string? token = null)
        {
            using var client = CreateClient(token);

            var response = await client.GetAsync("api/events");

            if (!response.IsSuccessStatusCode)
                return new List<EventViewModel>();

            var result = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>(
                    _jsonOptions);

            var events = result ?? new List<EventViewModel>();
            foreach (var item in events)
            {
                if (await HasBannerAsync(item.EventId)) item.BannerImageUrl = $"/event-banners/{item.EventId}";
            }
            return events;
        }

        public async Task<EventViewModel?> GetEventAsync(
            int id, string? token = null)
        {
            using var client = CreateClient(token);

            var response = await client.GetAsync(
                $"api/events/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            var item = await response.Content.ReadFromJsonAsync<EventViewModel>(_jsonOptions);
            if (item is not null && await HasBannerAsync(item.EventId)) item.BannerImageUrl = $"/event-banners/{item.EventId}";
            return item;
        }

        public async Task<int?> CreateEventAsync(
            EventViewModel model,
            string token)
        {
            using var client = CreateClient(token);

            var request = new
            {
                Name = model.Name,
                Description = model.Description,
                Date = model.Date,
                Location = model.Location,
                OrganiserId = model.OrganiserId
            };

            var response = await client.PostAsJsonAsync(
                "api/events", request);

            if (!response.IsSuccessStatusCode) return null;
            var created = await response.Content.ReadFromJsonAsync<CreatedEventResponse>(_jsonOptions);
            return created?.EventId;
        }

        public async Task<bool> UploadBannerAsync(int eventId, IFormFile file, string token)
        {
            using var client = CreateClient(token);
            using var form = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fileContent, "file", Path.GetFileName(file.FileName));
            var response = await client.PostAsync($"api/events/{eventId}/banner", form);
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> HasBannerAsync(int eventId)
        {
            using var client = CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Head, $"api/events/{eventId}/banner");
            using var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<(byte[] Content, string ContentType)?> GetBannerAsync(int eventId)
        {
            using var client = CreateClient();
            var response = await client.GetAsync($"api/events/{eventId}/banner");
            if (!response.IsSuccessStatusCode) return null;
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return (bytes, response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream");
        }

        private sealed class CreatedEventResponse
        {
            public int EventId { get; set; }
        }

        public async Task<bool> UpdateEventAsync(
            EventViewModel model,
            string token)
        {
            using var client = CreateClient(token);

            var request = new
            {
                Name = model.Name,
                Description = model.Description,
                Date = model.Date,
                Location = model.Location,
                OrganiserId = model.OrganiserId
            };

            var response = await client.PutAsJsonAsync(
                $"api/events/{model.EventId}", request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteEventAsync(
            int id, string token)
        {
            using var client = CreateClient(token);

            var response = await client.DeleteAsync(
                $"api/events/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}
