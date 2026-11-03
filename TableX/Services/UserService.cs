using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TableX.Models;

namespace TableX.Services
{
    public class UserService
    {
        private readonly HttpClient _client;

        public UserService(HttpClient client)
        {
            _client = client;
        }

        // GET api/auth/Me -> usuario logueado (necesita el token puesto con HttpService.SetToken).
        // El login ya devuelve el usuario, así que esto solo hace falta si lo quieres refrescar más tarde.
        public async Task<UserInfo?> GetMeAsync()
        {
            var response = await _client.GetAsync("api/auth/Me");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserInfo>();
        }
    }
}
