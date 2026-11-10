using System.Net.Http.Json;

public class ApiHttpClient
{
    private readonly HttpClient _httpClient;

    public ApiHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001/api/");
    }

    public async Task<T?> GetAsync<T>(string endpoint) => await _httpClient.GetFromJsonAsync<T>(endpoint);

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data) => await _httpClient.PostAsJsonAsync(endpoint, data);

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data) => await _httpClient.PutAsJsonAsync(endpoint, data);

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint) => await _httpClient.DeleteAsync(endpoint);
}