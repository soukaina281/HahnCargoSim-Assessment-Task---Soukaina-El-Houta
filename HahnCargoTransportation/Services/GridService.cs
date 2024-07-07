using HahnCargoTransportation.Models;
using HahnCargoTransportation.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace HahnCargoTransportation.Services
{
    public class GridService : IGridService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public GridService(IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _httpClient = httpClientFactory.CreateClient("HahnApiClient");
            _userService = userService;
        }

        public async Task<GridData> GetGridDataAsync()
        {
            AuthenticateRequest();
            var response = await _httpClient.GetAsync("/Grid/Get");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GridData>(content);
        }

        private void AuthenticateRequest()
        {
            var token = _userService.GetJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
