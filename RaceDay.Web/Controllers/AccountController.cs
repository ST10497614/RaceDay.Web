using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        private HttpClient GetApiClient()
        {
            string baseUrl = _configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("API BaseUrl is not configured.");

            return _httpClientFactory.CreateClient();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Token") != null)
            {
                var role = HttpContext.Session.GetString("Role");
                return role == "Organiser"
                    ? RedirectToAction("Dashboard", "Organiser")
                    : RedirectToAction("Dashboard", "Participant");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = GetApiClient();
                string baseUrl = _configuration["ApiSettings:BaseUrl"]!;

                var response = await client.PostAsJsonAsync(
                    $"{baseUrl.TrimEnd('/')}/api/Auth/login",
                    new
                    {
                        model.Email,
                        model.Password,
                        model.Role
                    });

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Invalid email, password or account type.");
                    return View(model);
                }

                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var root = document.RootElement;

                string token = root.GetProperty("token").GetString()!;
                string role = root.GetProperty("role").GetString()!;
                string userId = root.GetProperty("userId").ToString();
                string userName = root.TryGetProperty("userName", out var name)
                    ? name.GetString() ?? model.Email
                    : model.Email;

                HttpContext.Session.SetString("Token", token);
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("UserId", userId);
                HttpContext.Session.SetString("Email", model.Email);
                HttpContext.Session.SetString("UserName", userName);

                if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Organiser");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Participant");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Login error: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Role != "Participant" && model.Role != "Organiser")
            {
                ModelState.AddModelError("Role", "Select a valid account type.");
                return View(model);
            }

            try
            {
                string baseUrl = _configuration["ApiSettings:BaseUrl"]!;
                string endpoint = model.Role == "Organiser" ? "register/organiser" : "register/participant";
                var client = GetApiClient();

                var response = await client.PostAsJsonAsync(
                    $"{baseUrl.TrimEnd('/')}/api/Auth/{endpoint}",
                    new
                    {
                        model.Name,
                        model.Surname,
                        model.Email,
                        model.Password
                    });

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", response.StatusCode == System.Net.HttpStatusCode.Conflict
                        ? "This email is already registered."
                        : "Registration failed. Please check your details.");
                    return View(model);
                }

                TempData["Success"] = "Registration successful. Please log in.";
                return RedirectToAction("Login");
            }
            catch
            {
                ModelState.AddModelError("", "Unable to connect to RaceDay API.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}