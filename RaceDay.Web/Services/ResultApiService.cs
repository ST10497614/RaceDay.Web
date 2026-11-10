
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class ResultApiService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _configuration;

        private readonly JsonSerializerOptions _options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public ResultApiService(
            IHttpClientFactory factory,
            IConfiguration configuration)
        {
            _factory = factory;
            _configuration = configuration;
        }

        private HttpClient CreateClient(string? token = null)
        {
            var client = _factory.CreateClient();

            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "API BaseUrl is missing from appsettings.json.");
            }

            client.BaseAddress = new Uri(
                baseUrl.TrimEnd('/') + "/");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<List<ResultViewModel>> GetEventResultsAsync(
            int eventId,
            string token)
        {
            using var client = CreateClient(token);

            var response = await client.GetAsync(
                $"api/events/{eventId}/results");

            if (!response.IsSuccessStatusCode)
                return new List<ResultViewModel>();

            return await response.Content.ReadFromJsonAsync<
                List<ResultViewModel>>(_options)
                ?? new List<ResultViewModel>();
        }

        public async Task<bool> SubmitResultAsync(
            int eventId,
            ResultEntryViewModel model,
            string token)
        {
            using var client = CreateClient(token);

            var request = new
            {
                EnrolmentId = model.EnrolmentId,
                Position = model.Position,
                FinishTime = model.FinishTime
            };

            var response = await client.PostAsJsonAsync(
                $"api/events/{eventId}/results",
                request);

            return response.IsSuccessStatusCode;
        }

        public async Task<List<ResultViewModel>> GetMyResultsAsync(
            string token)
        {
            using var client = CreateClient(token);

            var response = await client.GetAsync(
                "api/users/me/results");

            if (!response.IsSuccessStatusCode)
                return new List<ResultViewModel>();

            return await response.Content.ReadFromJsonAsync<
                List<ResultViewModel>>(_options)
                ?? new List<ResultViewModel>();
        }
    }
}
