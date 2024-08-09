using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;

        public LoginController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new[] { "Error", "Invalid data" });
            }

            // Create the request body
            var requestBody = new
            {
                userName = loginModel.UserName,
                email = loginModel.Email,
                password = loginModel.Password
            };

            // Convert to JSON
            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // POST request to the external API
            var apiUrl = "https://corsproxy.io/?https://web.socem.plymouth.ac.uk/COMP2001/auth/api/users";
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<string[]>(responseBody);

                if (result.Length == 2 && result[1] == "True")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginModel.UserName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    HttpContext.Session.SetString("IsAuthenticated", "True");

                    return RedirectToAction("Index", "Home"); // Redirect to a protected page
                }
                else
                {
                    ModelState.AddModelError("", "Invalid credentials.");
                    return View(loginModel);
                }
            }
            else
            {
                return StatusCode((int)response.StatusCode, new[] { "Error", "External API error" });
            }
        }
    }
}
