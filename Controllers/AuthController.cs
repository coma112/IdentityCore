using IdentityCore.Entities;
using IdentityCore.Services;
using IdentityCore.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCore.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController(
        UserManager<Player> userManager,
        SignInManager<Player> signInManager,
        ITokenService tokenService) : ControllerBase
    {
        /// <summary>
        /// Register a new player account.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var player = new Player
            {
                Email = request.Email,
                UserName = request.Username,
            };

            var result = await userManager.CreateAsync(player, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
                

            return StatusCode(StatusCodes.Status201Created, new { message = "Registration successful." });
        }
    }
}
