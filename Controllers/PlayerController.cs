using IdentityCore.DTOs;
using IdentityCore.Entities;
using IdentityCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class PlayerController : ControllerBase
    {
        private readonly UserManager<Player> _userManager;
        private readonly ITokenService _tokenService;

        public PlayerController(
            UserManager<Player> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Reset the player's own password by providing the current password and a new one.
        /// The player must be logged in.
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var player = await _userManager.GetUserAsync(User);
            if (player is null)
                return Unauthorized(new ErrorResponse("Invalid credentials."));

            var result = await _userManager.ChangePasswordAsync(player, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponse(
                    "Password change failed.",
                    result.Errors.Select(e => e.Description)
                ));
            }

            await _tokenService.RevokeAllPlayerTokensAsync(player.Id);

            return NoContent();
        }

        /// <summary>
        /// Request deletion of the player's own account.
        /// </summary>
        [HttpPost("request-deletion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RequestDeletion([FromBody] RequestAccountDeletionRequest request)
        {
            var player = await _userManager.GetUserAsync(User);

            if (player is null)
                return Unauthorized(new ErrorResponse("Invalid credentials."));

            if (player.DeleteRequestedAt.HasValue)
                return Conflict(new ErrorResponse("Account deletion has already been requested."));

            var passwordValid = await _userManager.CheckPasswordAsync(player, request.Password);

            if (!passwordValid)
                return BadRequest(new ErrorResponse("Invalid password."));

            player.DeleteRequestedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(player);
            await _tokenService.RevokeAllPlayerTokensAsync(player.Id);

            return NoContent();
        }
    }
}