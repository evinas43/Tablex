using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TableX.Models;

namespace TableX.Services
{
    public class AuthService
    {
        private readonly HttpClient _client;

        public AuthService(HttpClient client)
        {
            _client = client;
        }

        // Devuelve token + usuario si el login es correcto, o null si el usuario/PIN no son válidos.
        // Cualquier otro error de la API (400, 404, 500...) lanza HttpRequestException con su StatusCode.
        public async Task<LoginResponse?> LoginAsync(string username, string pin)
        {
            // La API espera POST api/auth/login con { "username": "...", "password": "..." }
            var response = await _client.PostAsJsonAsync(
                "api/auth/login",
                new { username, password = pin });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
    }
}
