using HahnCargoTransportation.Models;
using HahnCargoTransportation.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace HahnCargoTransportation.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private string _jwtToken;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("HahnApiClient");
        }

        public async Task<LoginResponse> LoginAsync(string username, string password = "Hahn")
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { username, password }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/User/Login", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

            _jwtToken = loginResponse.Token;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            return loginResponse;
        }

        public async Task<int> GetCoinAmountAsync()
        {
            var response = await _httpClient.GetAsync("/User/CoinAmount");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<int>(content);
        }

        public string GetJwtToken()
        {
            return _jwtToken;
        }
    }
}
