using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LoginController> _logger;

        public LoginController(HttpClient httpClient, ILogger<LoginController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginModel loginModel)
        {
            // Debug log the incoming model
            _logger.LogDebug("Incoming login model: {Model}", JsonSerializer.Serialize(loginModel));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login data submitted. ModelState: {ModelState}", JsonSerializer.Serialize(ModelState));
                return View(loginModel);
            }

            _logger.LogInformation("Login attempt for user: {UserName} with email: {Email}", loginModel.UserName, loginModel.Email);

            // Create the request body
            var requestBody = new
            {
                userName = loginModel.UserName,
                email = loginModel.Email,
                password = loginModel.Password
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            _logger.LogDebug("Request Body: {RequestBody}", jsonBody);
            var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

            var targetUrl = "https://web.socem.plymouth.ac.uk/COMP2001/auth/api/users";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, targetUrl)
            {
                Content = content
            };
            requestMessage.Headers.Add("x-requested-with", "XMLHttpRequest");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(requestMessage);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("HTTP request failed: {Exception}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "External API request failed");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("API response: {ResponseBody}", responseBody);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("API call successful for user: {UserName}", loginModel.UserName);

                string[] result;
                try
                {
                    result = JsonSerializer.Deserialize<string[]>(responseBody);
                }
                catch (JsonException ex)
                {
                    _logger.LogError("Failed to deserialize API response: {Exception}", ex);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process API response");
                }

                if (result.Length == 2 && result[1] == "True")
                {
                    _logger.LogInformation("Authentication successful for user: {UserName}", loginModel.UserName);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginModel.UserName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    _logger.LogInformation("User {UserName} signed in.", loginModel.UserName);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning("Invalid credentials provided for user: {UserName}", loginModel.UserName);
                    ModelState.AddModelError("", "Invalid credentials.");
                    return View(loginModel);
                }
            }
            else
            {
                _logger.LogError("API call failed for user: {UserName} with status code: {StatusCode}", loginModel.UserName, response.StatusCode);
                return StatusCode((int)response.StatusCode, "External API error");
            }
        }
    }
}
