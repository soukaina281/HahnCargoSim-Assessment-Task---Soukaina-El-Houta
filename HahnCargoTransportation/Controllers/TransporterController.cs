using HahnCargoTransportation.Services;
using HahnCargoTransportation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HahnCargoTransportation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransporterController : ControllerBase
    {
        private readonly ITransporterService _transporterService;

        public TransporterController(ITransporterService transporterService)
        {
            _transporterService = transporterService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllTransporters()
        {
            var transporters = await _transporterService.GetTransportersAsync();
            return Ok(transporters);
        }
    }
}
