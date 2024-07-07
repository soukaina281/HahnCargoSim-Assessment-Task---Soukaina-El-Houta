using HahnCargoTransportation.Models;
using HahnCargoTransportation.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace HahnCargoTransportation.Services
{
    public class SimulationService : ISimulationService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public SimulationService(IHttpClientFactory httpClientFactory, IUserService userService, IServiceProvider serviceProvider)
        {
            _httpClient = httpClientFactory.CreateClient("HahnApiClient");
            _userService = userService;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync()
        {
            AuthenticateRequest();
            await _httpClient.PostAsync("/Sim/Start", null);

            using (var scope = _serviceProvider.CreateScope())
            {
                var transporterService = scope.ServiceProvider.GetRequiredService<ITransporterService>();
                await transporterService.BuyTransporterAsync(0);
            }
        }

        public async Task StopAsync()
        {
            AuthenticateRequest();
            await _httpClient.PostAsync("/Sim/Stop", null);
        }

        private void AuthenticateRequest()
        {
            var token = _userService.GetJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
