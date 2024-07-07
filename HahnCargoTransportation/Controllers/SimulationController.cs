using HahnCargoTransportation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HahnCargoTransportation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationController : ControllerBase
    {
        private readonly ISimulationService _simulationService;

        public SimulationController(ISimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        [HttpPost("Start")]
        public async Task<IActionResult> Start()
        {
            await _simulationService.StartAsync();
            return Ok();
        }

        [HttpPost("Stop")]
        public async Task<IActionResult> StopAsync()
        {
            await _simulationService.StopAsync();
            return Ok();
        }
    }
}
