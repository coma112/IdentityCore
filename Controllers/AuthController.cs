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
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Player? player = request.UsernameOrEmail.Contains('@')
                ? await userManager.FindByEmailAsync(request.UsernameOrEmail)
                : await userManager.FindByNameAsync(request.UsernameOrEmail);

            if (player is null)
            {
                return Unauthorized(new { message = "Invalid Credentials." });
            }

            var result = await signInManager.CheckPasswordSignInAsync(player, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return Unauthorized(new { message = "Account is temporarly locked. Try again later:)" });
            }

            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid Credentials." });
            }

            string accessToken = tokenService.GenerateAccessToken(player);
            RefreshToken refreshToken = await tokenService.GenerateRefreshTokenAsync(player);

            return Ok(new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token,
                ExpiresAt: refreshToken.ExpiresAt,
                Username: player.UserName!,
                Email: player.Email!
            ));
        }

        /// <summary>
        /// Refresh an expires access token.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var (accessToken, newRefreshToken) = await tokenService.RotateRefreshTokenAsync(request.RefreshToken);
                Player? player = await userManager.FindByIdAsync(newRefreshToken.PlayerId);

                return Ok(new AuthResponse( 
                    AccessToken: accessToken,
                    RefreshToken: newRefreshToken.Token,
                    ExpiresAt: newRefreshToken.ExpiresAt,
                    Username: player!.UserName!,
                    Email: player!.Email!
                ));
            } catch (InvalidOperationException exception)
            {
                return Unauthorized(new { message = exception.Message });
            }
        }

        /// <summary>
        /// Log out by revoking the refresh token.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            return NoContent();
        }

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
