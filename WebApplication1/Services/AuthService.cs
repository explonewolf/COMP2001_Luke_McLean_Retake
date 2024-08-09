using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IAuthService
    {
        Task<bool> VerifyUserAsync(LoginModel loginModel);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyUserAsync(LoginModel loginModel)
        {
            var jsonContent = JsonSerializer.Serialize(loginModel);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://web.socem.plymouth.ac.uk/COMP2001/auth/api/users", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<string[]>(responseString);

                return result != null && result.Length == 2 && result[0] == "Verified" && result[1] == "True";
            }

            return false;
        }
    }
}
