using HahnCargoTransportation.Models;
using HahnCargoTransportation.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace HahnCargoTransportation.Services
{
    public class TransporterService : ITransporterService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public TransporterService(IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _httpClient = httpClientFactory.CreateClient("HahnApiClient");
            _userService = userService;
        }

        public async Task<List<CargoTransporter>> GetTransportersAsync()
        {
            AuthenticateRequest();
            var response = await _httpClient.GetAsync("/CargoTransporter/GetAll");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CargoTransporter>>(content);
        }

        public async Task BuyTransporterAsync(int nodeId)
        {
            AuthenticateRequest();
            var response = await _httpClient.PostAsync($"/CargoTransporter/Buy?positionNodeId=0", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task MoveTransporterAsync(int transporterId, int toNodeId)
        {
            AuthenticateRequest();
            var response = await _httpClient.PutAsync($"/CargoTransporter/Move?transporterId={transporterId}&targetNodeId={toNodeId}", null);
            response.EnsureSuccessStatusCode();
        }

        private void AuthenticateRequest()
        {
            var token = _userService.GetJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
