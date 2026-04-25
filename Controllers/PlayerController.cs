namespace IdentityCore.Controllers;

using IdentityCore.DTOs;
using IdentityCore.Entities;
using IdentityCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class PlayerController(
    UserManager<Player> userManager,
    ITokenService tokenService) : ControllerBase
{
    /// <summary>
    /// Reset the player's own password by providing the current password and a new one.
    /// The player must be logged in.
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var player = await userManager.GetUserAsync(User);
        if (player is null)
            return Unauthorized();

        var result = await userManager.ChangePasswordAsync(player, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        // Revoke all existing sessions, player must login again with new password
        await tokenService.RevokeAllPlayerTokensAsync(player.Id);

        return NoContent();
    }

    /// <summary>
    /// Request deletion of the player's own account.
    /// </summary>
    [HttpPost("request-deletion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestDeletion([FromBody] RequestAccountDeletionRequest request)
    {
        var player = await userManager.GetUserAsync(User);
        if (player is null)
            return Unauthorized();

        if (player.DeleteRequestedAt.HasValue)
            return Conflict(new { message = "Account deletion has already been requested." });

        var passwordValid = await userManager.CheckPasswordAsync(player, request.Password);
        if (!passwordValid)
            return BadRequest(new { message = "Invalid password." });

        player.DeleteRequestedAt = DateTime.UtcNow;

        await userManager.UpdateAsync(player);
        await tokenService.RevokeAllPlayerTokensAsync(player.Id);

        return NoContent();
    }
}