using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TableX.Models;

namespace TableX.Services
{
    public class MesaService
    {
        private readonly HttpClient _client;

        public MesaService(HttpClient client)
        {
            _client = client;
        }

        // GET api/mesas -> todas las mesas (necesita el token).
        // Si la API responde con error (401, 404, 500...) lanza HttpRequestException con su StatusCode.
        public async Task<List<Mesa>> GetAllAsync()
        {
            var response = await _client.GetAsync("api/mesas");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Mesa>>() ?? new List<Mesa>();
        }
    }
}
