using IdentityCore.DTOs;
using IdentityCore.Entities;
using IdentityCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCore.Controllers
{
    [ApiController]
    [Route("api/bo/auth")]
    [Produces("application/json")]
    public class BoAuthController : ControllerBase
    {
        private readonly UserManager<BoUser> _boUserManager;
        private readonly SignInManager<BoUser> _boSignInManager;
        private readonly IBoTokenService _boTokenService;

        public BoAuthController(UserManager<BoUser> boUserManager, SignInManager<BoUser> boSignInManager, IBoTokenService boTokenService)
        {
            _boUserManager = boUserManager;
            _boSignInManager = boSignInManager;
            _boTokenService = boTokenService;
        }

        /// <summary>
        /// Login as a BO user. Returns JWT access + refresh tokens.
        /// Supports lockout after repeated failed attempts (same policy as players).
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(BoAuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] BoLoginRequest request)
        {
            var boUser = request.UsernameOrEmail.Contains('@')
                ? await _boUserManager.FindByEmailAsync(request.UsernameOrEmail)
                : await _boUserManager.FindByNameAsync(request.UsernameOrEmail);

            if (boUser is null)
                return Unauthorized(new ErrorResponse("Invalid credentials."));

            var result = await _boSignInManager.CheckPasswordSignInAsync(
                boUser, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return Unauthorized(new ErrorResponse("Account is temporarily locked. Try again later."));

            if (!result.Succeeded)
                return Unauthorized(new ErrorResponse("Invalid credentials."));

            var accessToken = _boTokenService.GenerateAccessToken(boUser);
            var refreshToken = await _boTokenService.GenerateRefreshTokenAsync(boUser);

            return Ok(new BoAuthResponse(accessToken, refreshToken.Token, refreshToken.ExpiresAt, boUser));
        }

        /// <summary>
        /// Refresh an expired BO access token.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(BoAuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] BoRefreshTokenRequest request)
        {
            var (accessToken, newRefreshToken, boUser) =
                await _boTokenService.RotateRefreshTokenAsync(request.RefreshToken);

            return Ok(new BoAuthResponse(accessToken, newRefreshToken.Token, newRefreshToken.ExpiresAt, boUser));
        }

        /// <summary>
        /// Logout, revoke the provided refresh token.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout([FromBody] BoLogoutRequest request)
        {
            var boUserId = _boUserManager.GetUserId(User)!;
            await _boTokenService.RevokeRefreshTokenAsync(request.RefreshToken, boUserId);
            return NoContent();
        }
    }
}
