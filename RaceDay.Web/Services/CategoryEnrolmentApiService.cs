
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services
{
    public class CategoryEnrolmentApiService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _configuration;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public CategoryEnrolmentApiService(
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
                throw new InvalidOperationException(
                    "API BaseUrl is missing from appsettings.json.");

            client.BaseAddress = new Uri(
                baseUrl.TrimEnd('/') + "/");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            using var client = CreateClient();

            var response = await client.GetAsync("api/Categories");

            if (!response.IsSuccessStatusCode)
                return new List<CategoryViewModel>();

            return await response.Content.ReadFromJsonAsync<
                List<CategoryViewModel>>(_jsonOptions)
                ?? new List<CategoryViewModel>();
        }

        public async Task<bool> CreateCategoryAsync(
            CategoryViewModel model,
            string token)
        {
            using var client = CreateClient(token);

            var response = await client.PostAsJsonAsync(
                "api/Categories",
                model);

            return response.IsSuccessStatusCode;
        }

        public async Task<List<EnrolmentViewModel>> GetMyEnrolmentsAsync(
            string token)
        {
            using var client = CreateClient(token);

            var response = await client.GetAsync(
                "api/enrolments/my-enrolments");

            if (!response.IsSuccessStatusCode)
                return new List<EnrolmentViewModel>();

            return await response.Content.ReadFromJsonAsync<
                List<EnrolmentViewModel>>(_jsonOptions)
                ?? new List<EnrolmentViewModel>();
        }

        public async Task<bool> EnrolAsync(
            int categoryId,
            string token)
        {
            using var client = CreateClient(token);

            var response = await client.PostAsJsonAsync(
                "api/enrolments",
                new { CategoryId = categoryId });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CancelEnrolmentAsync(
            int enrolmentId,
            string token)
        {
            using var client = CreateClient(token);

            var response = await client.DeleteAsync(
                $"api/enrolments/{enrolmentId}");

            return response.IsSuccessStatusCode;
        }
    }
}
