using HahnCargoTransportation.Models;
using HahnCargoTransportation.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace HahnCargoTransportation.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public OrderService(IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _httpClient = httpClientFactory.CreateClient("HahnApiClient");
            _userService = userService;
        }

        public async Task<List<Order>> GetAllAvailableOrdersAsync()
        {
            AuthenticateRequest();
            var response = await _httpClient.GetAsync("/Order/GetAllAvailable");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Order>>(content);
        }

        public async Task AcceptOrderAsync(int orderId)
        {
            AuthenticateRequest();
            var response = await _httpClient.PostAsync($"/Order/Accept?orderId={orderId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Order>> GetAllAcceptedOrdersAsync()
        {
            AuthenticateRequest();
            var response = await _httpClient.GetAsync("/Order/GetAllAccepted");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Order>>(content);
        }

        private void AuthenticateRequest()
        {
            var token = _userService.GetJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
