using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;


namespace WebServer
{
    public class WeatherApiClient
    {
        string appid;

        public WeatherApiClient(string appid){
            this.appid = appid;
        }

        private static HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://api.openweathermap.org")
        };

        public async Task<Root> GetWeatherAsync(CancellationToken cancellationToken, string lat, string lon)
        {
            var result = await _httpClient.GetFromJsonAsync<Root>($"/data/2.5/weather?lat={lat}&lon={lon}&appid={appid}",
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }, cancellationToken: cancellationToken);

            if (result == null)
            {
                throw new ApplicationException("No weather info available");
            }
            return result;
        }
    }
}
