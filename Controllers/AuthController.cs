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
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Player> _userManager;
        private readonly SignInManager<Player> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<Player> userManager,
            SignInManager<Player> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Register a new player account.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var player = new Player
            {
                Email = request.Email,
                UserName = request.Username,
            };

            var result = await _userManager.CreateAsync(player, request.Password);

            if (!result.Succeeded)
            {
                // Egységes ErrorResponse minden hibaválasznál.
                return BadRequest(new ErrorResponse(
                    "Registration failed.",
                    result.Errors.Select(e => e.Description)
                ));
            }

            return StatusCode(StatusCodes.Status201Created, new { message = "Registration successful." });
        }

        /// <summary>
        /// Login &amp; JWT access + refresh tokens.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Player? player = request.UsernameOrEmail.Contains('@')
                ? await _userManager.FindByEmailAsync(request.UsernameOrEmail)
                : await _userManager.FindByNameAsync(request.UsernameOrEmail);

            if (player is null)
            {
                return Unauthorized(new ErrorResponse("Invalid credentials."));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(player, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return Unauthorized(new ErrorResponse("Account is temporarily locked. Try again later."));
            }

            if (!result.Succeeded)
            {
                return Unauthorized(new ErrorResponse("Invalid credentials."));
            }

            string accessToken = _tokenService.GenerateAccessToken(player);
            RefreshToken refreshToken = await _tokenService.GenerateRefreshTokenAsync(player);

            return Ok(new AuthResponse(
                accessToken: accessToken,
                refreshToken: refreshToken.Token,
                expiresAt: refreshToken.ExpiresAt,
                username: player.UserName!,
                email: player.Email!
            ));
        }

        /// <summary>
        /// Refresh an expired access token.
        /// </summary>
        [HttpPost("refresh")]
        [Authorize]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var playerId = _userManager.GetUserId(User)!;

            var (accessToken, newRefreshToken) = await _tokenService.RotateRefreshTokenAsync(request.RefreshToken, playerId);

            Player? player = await _userManager.FindByIdAsync(newRefreshToken.PlayerId);
            if (player is null)
            {
                return Unauthorized(new ErrorResponse("Invalid credentials."));
            }

            return Ok(new AuthResponse(
                accessToken: accessToken,
                refreshToken: newRefreshToken.Token,
                expiresAt: newRefreshToken.ExpiresAt,
                username: player.UserName!,
                email: player.Email!
            ));
        }

        /// <summary>
        /// Log out by revoking the refresh token.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var playerId = _userManager.GetUserId(User)!;
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, playerId);
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
            var playerId = _userManager.GetUserId(User)!;
            await _tokenService.RevokeAllPlayerTokensAsync(playerId);
            return NoContent();
        }
    }
}