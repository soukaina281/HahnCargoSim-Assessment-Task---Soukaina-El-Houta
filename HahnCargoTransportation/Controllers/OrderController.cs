using HahnCargoTransportation.Services;
using HahnCargoTransportation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HahnCargoTransportation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("Accepted")]
        public async Task<IActionResult> GetAcceptedOrders()
        {
            var acceptedOrders = await _orderService.GetAllAcceptedOrdersAsync();
            return Ok(acceptedOrders);
        }
    }
}
