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
        private readonly IWalletService _walletService;

        public AuthController(
            UserManager<Player> userManager,
            SignInManager<Player> signInManager,
            ITokenService tokenService,
            IWalletService walletService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _walletService = walletService;
        }

        /// <summary>
        /// Register a new player account. Automatically creates a wallet.
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
                return BadRequest(new ErrorResponse(
                    "Registration failed.",
                    result.Errors.Select(e => e.Description)
                ));
            }

            await _walletService.CreateWalletForPlayerAsync(player.Id);

            return StatusCode(StatusCodes.Status201Created, new { message = "Registration successful." });
        }

        /// <summary>
        /// Login & JWT access + refresh tokens.
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
                return Unauthorized(new ErrorResponse("Invalid credentials."));

            var result = await _signInManager.CheckPasswordSignInAsync(player, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return Unauthorized(new ErrorResponse("Account is temporarily locked. Try again later."));

            if (!result.Succeeded)
                return Unauthorized(new ErrorResponse("Invalid credentials."));

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
        /// Refresh an expired access token. Does not require a valid access token.
        /// The player ID is extracted from the refresh token itself.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var (accessToken, newRefreshToken, player) = await _tokenService.RotateRefreshTokenAsync(request.RefreshToken);

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