using IdentityCore.DTOs;
using IdentityCore.Exceptions;
using IdentityCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityCore.Controllers
{
    /// <summary>
    /// Role based access is enforced / endpoint via [Authorize(Roles = "...")].
    /// </summary>
    [ApiController]
    [Route("api/bo")]
    [Authorize]
    [Produces("application/json")]
    public class BoController : ControllerBase
    {
        private readonly IBoUserService _boUserService;

        public BoController(IBoUserService boUserService)
        {
            _boUserService = boUserService;
        }

        private string CurrentBoUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // players

        /// <summary>
        /// Get a paged list of all players.
        /// </summary>
        [HttpGet("players")]
        [Authorize(Roles = "Viewer,BonusManager,SuperAdmin")]
        [ProducesResponseType(typeof(PagedResponse<PlayerSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                throw new BadRequestException("Invalid pagination parameters.");

            var result = await _boUserService.GetPlayersAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific player's wallet balance.
        /// </summary>
        [HttpGet("players/{playerId}/balance")]
        [Authorize(Roles = "Viewer,BonusManager,SuperAdmin")]
        [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlayerBalance(string playerId)
        {
            var wallet = await _boUserService.GetPlayerBalanceAsync(playerId);
            return Ok(wallet);
        }

        /// <summary>
        /// Get a specific player's transaction history.
        /// </summary>
        [HttpGet("players/{playerId}/transactions")]
        [Authorize(Roles = "Viewer,BonusManager,SuperAdmin")]
        [ProducesResponseType(typeof(PagedResponse<TransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlayerTransactions(
            string playerId,
            [FromQuery] TransactionHistoryRequest request)
        {
            var result = await _boUserService.GetPlayerTransactionsAsync(playerId, request);
            return Ok(result);
        }

        // bonus

        /// <summary>
        /// Give a bonus cash amount to a player. BonusManager + SuperAdmin only.
        /// </summary>
        [HttpPost("players/{playerId}/bonus")]
        [Authorize(Roles = "BonusManager,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GiveBonus(string playerId, [FromBody] GiveBonusRequest request)
        {
            await _boUserService.GiveBonusAsync(CurrentBoUserId, playerId, request.Amount, request.Notes);
            return NoContent();
        }

        // team

        /// <summary>
        /// Get the list of all BO users (team members).
        /// </summary>
        [HttpGet("team")]
        [Authorize(Roles = "Viewer,BonusManager,SuperAdmin")]
        [ProducesResponseType(typeof(List<BoUserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeam()
        {
            var team = await _boUserService.GetTeamAsync();
            return Ok(team);
        }

        /// <summary>
        /// Invite / create a new BO user. SuperAdmin only.
        /// </summary>
        [HttpPost("team/invite")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(BoUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> InviteBoUser([FromBody] InviteBoUserRequest request)
        {
            var boUser = await _boUserService.InviteBoUserAsync(request, CurrentBoUserId);
            return StatusCode(StatusCodes.Status201Created, new BoUserResponse(boUser));
        }

        // audit

        /// <summary>
        /// Get audit log entries (paged). SuperAdmin only.
        /// </summary>
        [HttpGet("audit")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(PagedResponse<AuditLogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                throw new BadRequestException("Invalid pagination parameters.");

            var result = await _boUserService.GetAuditLogsAsync(page, pageSize);
            return Ok(result);
        }
    }
}