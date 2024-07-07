using HahnCargoTransportation.Services;
using HahnCargoTransportation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HahnCargoTransportation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GridController : ControllerBase
    {
        private readonly IGridService _gridService;

        public GridController(IGridService gridService)
        {
            _gridService = gridService;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetGrid()
        {
            var gridData = await _gridService.GetGridDataAsync();
            return Ok(gridData);
        }
    }
}
