// Here we will put functions to register user and issue tokens. Maybe also adding the email verification here.
// At the current time we have not agreed upon wether we are going to do that or not. 

using System.Net;
using System.Text;
using System.Text.Json;
using FirebaseAdmin.Auth;

namespace SmartFridgeBackend.Services
{
    public class FirebaseAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FirebaseAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<(string Uid, string Email)> RegisterAsync(string email, string password)
        {
            var args = new UserRecordArgs
            {
                Email = email,
                Password = password
            };

            var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);

            return (userRecord.Uid, userRecord.Email ?? email);
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Firebase:ApiKey is missing in appsettings.json");
            }

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";

            var payload = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Firebase login failed: {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);

            if (!document.RootElement.TryGetProperty("idToken", out var idTokenElement))
            {
                throw new Exception("Firebase returned no idToken");
            }

            var idToken = idTokenElement.GetString();

            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new Exception("Firebase returned empty idToken");
            }

            return idToken;
        }

        public async Task SendEmailVerificationAsync(string idToken)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";

            var payload = new
            {
                requestType = "VERIFY_EMAIL",
                idToken
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(url, content);

            if(!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception(body);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            var apiKey = _configuration["Firebase:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Firebase:ApiKey is missing in appsettings.json");
            }

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";

            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                throw new Exception($"Firebase password reset failed: {responseBody}");
            }
        }
    }
}