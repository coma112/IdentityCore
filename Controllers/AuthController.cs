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

        /// <summary>
        /// Login & JWT access + refresh tokens.
        /// </summary>

        /// <summary>
        /// Refresh an expires access token.
        /// </summary>

        /// <summary>
        /// Log out by revoking the refresh token.
        /// </summary>

        /// <summary>
        /// Log out from all devices by revoking every refresh token.
        /// </summary>
        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LogoutAll()
        {
            var playerId = userManager.GetUserId(User)!;
            await tokenService.RevokeAllPlayerTokensAsync(playerId);
            return NoContent();
        }
    }
}
