using HahnCargoTransportation.Models;
using HahnCargoTransportation.Services;
using HahnCargoTransportation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HahnCargoTransportation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Invalid login request");
            }

            var response = await _userService.LoginAsync(user.Username, user.Password);
            if (response.Token != null)
            {
                return Ok(response);
            }
            else
            {
                return Unauthorized("Invalid credentials");
            }
        }

        [HttpGet("CoinAmount")]
        public async Task<IActionResult> GetCoinAmount()
        {
            var coins = await _userService.GetCoinAmountAsync();
            return Ok(coins);
        }
    }
}
