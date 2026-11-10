using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TableX.Services
{
    public class HttpService
    {
        // URL de TablexAPI (perfil "https" de Properties/launchSettings.json de la API).
        // Tiene que acabar en "/" y las rutas de los servicios NO deben empezar por "/".
        private const string BaseUrl = "https://TableX.somee.com/publish/";


        private static HttpClient? _client;

        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient
                    {
                        BaseAddress = new Uri(BaseUrl),
                        Timeout = TimeSpan.FromSeconds(15) // si la API no responde, no se queda colgado
                    };
                }

                return _client;
            }
        }

        public static void SetToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ClearToken()
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }
}
