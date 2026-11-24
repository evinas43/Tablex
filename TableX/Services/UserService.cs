using System.Collections.Generic;
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

        // GET api/users/all -> todos los usuarios (necesita el token).
        // Si la API responde con error (401, 404, 500...) lanza HttpRequestException con su StatusCode.
        public async Task<List<UserInfo>> GetAllAsync()
        {
            var response = await _client.GetAsync("api/users/all");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<UserInfo>>() ?? new List<UserInfo>();
        }

        // DELETE api/users/{id} -> borra el usuario (necesita el token).
        // Si la API responde con error (401, 403, 404, 500...) lanza HttpRequestException con su StatusCode.
        public async Task DeleteAsync(int id)
        {
            var response = await _client.DeleteAsync($"api/users/{id}");

            response.EnsureSuccessStatusCode();
        }

        // PUT api/users/{id} -> cambia nombre y rol, y devuelve el usuario actualizado (necesita el token).
        // La contraseña no se envía, así que la API mantiene la actual.
        // Si la API responde con error (400, 401, 403, 404, 409 nombre repetido...) lanza HttpRequestException con su StatusCode.
        public async Task<UserInfo> UpdateAsync(int id, string username, string role)
        {
            var response = await _client.PutAsJsonAsync($"api/users/{id}", new { username, role });

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserInfo>()
                ?? new UserInfo { Id = id, Username = username, Role = role };
        }
    }
}
